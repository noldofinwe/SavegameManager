using ATGSaveGameManager;
using ATGSaveGameManager.Avalonia.Models;
using ATGSaveGameManager.Avalonia.Xmpp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using XmppDotNet;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.HttpUpload;
using XmppDotNet.Xmpp.PubSub;
using XmppDotNet.Xmpp.XData;
using Affiliations = XmppDotNet.Xmpp.PubSub.Owner.Affiliations;
using Configure = XmppDotNet.Xmpp.PubSub.Owner.Configure;
using Delete = XmppDotNet.Xmpp.PubSub.Owner.Delete;
using Item = XmppDotNet.Xmpp.PubSub.Item;

public class PubSubManager
{
    private readonly XmppClient _client;
    private readonly string _pubsubService;
    private readonly string _uploadService;


    public PubSubManager(XmppClient client, string pubsubService)
    {
        _client = client;
        _pubsubService = pubsubService;
        _uploadService = "upload.bobbinhold.net";
    }

    // ----------------- Create node -----------------
    public async Task CreateGame(GameInfoModel gameInfo, GameType type)
    {
        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub =  new PubSub();
        var create = new Create { Node = "game/" + gameInfo.Id };
        pubsub.Add(create);

        var configure = new XmppDotNet.Xmpp.PubSub.Configure();

        var x = new Data
        {
            Type = FormType.Submit
        };

        // Required hidden FORM_TYPE
        x.AddField(new Field("FORM_TYPE", "http://jabber.org/protocol/pubsub#node_config", FieldType.Hidden));

        // publish_model = publishers
        x.AddField(new Field("pubsub#publish_model", "publishers"));

        // access_model = open (optional)
        x.AddField(new Field("pubsub#access_model", "open"));


        configure.Add(x);
        pubsub.Add(configure);
        iq.Add(pubsub);
        var result = await _client.SendIqAsync(iq);

        await Publish(gameInfo.Id, XmppSerializer.ToXElement(gameInfo), "metadata");

        await UploadGameTurn(gameInfo, type, gameInfo.Players[0]);

        await SetPublishers(gameInfo.Id, gameInfo.Players.ToList());

        await TestAffiliations(gameInfo.Id);
    }

    private async Task TestAffiliations(string gameInfoId)
    {
        var iq = new Iq
        {
            Type = IqType.Get,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub =  new XmppDotNet.Xmpp.PubSub.Owner.PubSub();
        var create = new Affiliations
        {
            Node = "game/" + gameInfoId,
        };
        pubsub.Add(create);
        iq.Add(pubsub);
        var result = await _client.SendIqAsync(iq);

        
    }

    public async Task UploadGameTurn(GameInfoModel gameInfo, GameType type, string player)
    {
        var path = Path.Combine(type.Savegames, gameInfo.FileName);

        var fileInfo = new FileInfo(path);
        var length = fileInfo.Length;

        var uploadUrl = await GetUploadUrl(gameInfo.FileName, (int)length);

        await UploadSaveFileAsync(uploadUrl, path);

        var gameTurn = new GameTurnModel
        {
            LastPlayer = player,
            LastTurnTime = DateTime.Now,
            Url = uploadUrl,
        };

        await Publish(gameInfo.Id, XmppSerializer.ToXElement(gameTurn));
    }

    private async Task<string> GetUploadUrl(string fileName, int length)
    {
        //    < iq type = 'get' to = 'upload.yourserver.net' id = 'upload1' >
        //  < request xmlns = 'urn:xmpp:http:upload:0'
        //           filename = 'turn5.sav'
        //           size = '123456'
        //           content - type = 'application/octet-stream' />
        //</ iq >

        var iq = new Iq
        {
            Type = IqType.Get,
            To = _uploadService,
            Id = Guid.NewGuid().ToString("N")
        };
        var request = new Request()
        {
            Filename = fileName,
            Size = length,
            ContentType = "application/octet-stream"
        };

        iq.Add(request);

        var result = await _client.SendIqAsync(iq);

        XNamespace nsPubSub = "urn:xmpp:http:upload:0";

        // Step 1: navigate to the <item>
        var slot = result.Element(nsPubSub + "slot");
        var put = slot?.Element(nsPubSub + "put");
        var get = slot?.Element(nsPubSub + "get");
        var puturl = put.Attribute("url");
        var geturl = get.Attribute("url");


        return puturl.Value;
        //    < iq type = 'result' id = 'upload1' from = 'upload.yourserver.net' >
        //  < slot xmlns = 'urn:xmpp:http:upload:0' >
        //    < put url = 'https://upload.yourserver.net/AbCdEf' />
        //    < get url = 'https://upload.yourserver.net/AbCdEf' />
        //  </ slot >
        //</ iq >


    }


    private async Task UploadSaveFileAsync(string putUrl, string filePath)
    {
        using var http = new HttpClient();
        var bytes = await File.ReadAllBytesAsync(filePath);

        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        var response = await http.PutAsync(putUrl, content);
        response.EnsureSuccessStatusCode();
    }

    // ----------------- Subscribe -----------------
    public async Task Subscribe(string node)
    {
        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new PubSub();
        var subscribe = new Subscribe
        {
            Node = "game/" + node,
            Jid = _client.Jid
        };

        pubsub.Add(subscribe);
        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);
    }

    // ----------------- Publish -----------------
    public async Task Publish(string node, XElement payload, string itemId = null)
    {
        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new PubSub();
        var publish = new Publish { Node = "game/" + node };

        var item = new Item();
        if (!string.IsNullOrEmpty(itemId))
            item.Id = itemId;

        item.Add(payload);
        publish.Add(item);
        pubsub.Add(publish);
        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);
    }
    public async Task<List<GameInfoModel>> ListNodesAsync()
    {
        var iq = new Iq
        {
            Type = IqType.Get,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        XNamespace disco = "http://jabber.org/protocol/disco#items";
        var query = new XElement(disco + "query");

        iq.Add(query);

        // Send IQ and get response
        var response = await _client.SendIqAsync(iq);

        // Find <query> element in disco#items namespace
        XNamespace ns = "http://jabber.org/protocol/disco#items";
        var queryElement = response.Element(ns + "query");

        var result = new List<string>();

        if (queryElement != null)
        {
            foreach (var item in queryElement.Elements(ns + "item"))
            {
                var node = (string)item.Attribute("node");
                if (!string.IsNullOrEmpty(node))
                    result.Add(node);
            }
        }

        var games = new List<GameInfoModel>();
        foreach (var node in result)
        {
            var game = await GetMetadata(node);
            var turn = await GetLastTurns(node);
            game.GameTurnModel = turn;
            games.Add(game);
        }

        var subscriptions = await GetSubscriptions();

        foreach (var subscription in subscriptions)
        {
            var game = games.FirstOrDefault(x => subscription.Contains(x.Id));
            if (game != null)
            {
                game.Subscribed = true;
            }
        }
        return games;
    }

    public async Task SetPublishers(string node, List<string> jids)
    {
        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new XmppDotNet.Xmpp.PubSub.Owner.PubSub();

        var affiliations = new XmppDotNet.Xmpp.PubSub.Owner.Affiliations
        {
            Node = "game/" + node
        };
        
        affiliations.Add(new XmppDotNet.Xmpp.PubSub.Owner.Affiliation
        {
            Jid = _client.Jid,
            AffiliationType = AffiliationType.Owner
        });

        foreach (var jid in jids.Where(j => j != _client.Jid))
        {
            affiliations.Add(new XmppDotNet.Xmpp.PubSub.Owner.Affiliation
            {
                Jid = jid,
                AffiliationType = AffiliationType.Publisher
            });
        }


        pubsub.Add(affiliations);
        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);
    }


    private async Task<List<string>> GetSubscriptions()
    {
        var iq = new Iq
        {
            Type = IqType.Get,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new PubSub();
        pubsub.Add(new Subscriptions()); // no node = list all subscriptions
        iq.Add(pubsub);

        var response = await _client.SendIqAsync(iq);

        XNamespace ns = "http://jabber.org/protocol/pubsub";

        var subscriptions = response
            .Element(ns + "pubsub")?
            .Element(ns + "subscriptions")?
            .Elements(ns + "subscription")
            .Select(s => (string)s.Attribute("node"))
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        return subscriptions ?? new List<string>();
    }


    private async Task<GameInfoModel> GetMetadata(string node)
    {
        var iq = new Iq
        {
            Type = IqType.Get,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new PubSub();

        var items = new Items()
        {
            Node = node
        };
        var metadataItem = new Item()
        {
            Id = "metadata"
        };

        items.Add(metadataItem);
        pubsub.Add(items);

        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);

        return GetGameInfo(result);
    }


    private async Task<GameTurnModel> GetLastTurns(string node)
    {
        var iq = new Iq
        {
            Type = IqType.Get,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new PubSub();

        var items = new Items()
        {
            Node = node,
            MaxItems = 1
        };
      

        pubsub.Add(items);

        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);

        return GetGameTurn(result);
    }


    private GameInfoModel GetGameInfo(Iq result)
    {
        XNamespace nsPubSub = "http://jabber.org/protocol/pubsub";

        // Step 1: navigate to the <item>
        var pubsub = result.Element(nsPubSub + "pubsub");
        var items = pubsub?.Element(nsPubSub + "items");
        var item = items?.Element(nsPubSub + "item");

        // Step 2: extract the payload element
        var gameInfoElement = item?.Elements().FirstOrDefault();
        if (gameInfoElement == null)
            return null; // or throw

        // Step 3: deserialize

        var serializer = new XmlSerializer(typeof(GameInfoModel));
        using var reader = gameInfoElement.CreateReader();
        var model = (GameInfoModel)serializer.Deserialize(reader);

        // model now contains your metadata
        return model;

    }

    private GameTurnModel GetGameTurn(Iq result)
    {
        XNamespace nsPubSub = "http://jabber.org/protocol/pubsub";

        // Step 1: navigate to the <item>
        var pubsub = result.Element(nsPubSub + "pubsub");
        var items = pubsub?.Element(nsPubSub + "items");
        var item = items?.Element(nsPubSub + "item");

        // Step 2: extract the payload element
        var gameInfoElement = item?.Elements().FirstOrDefault();
        if (gameInfoElement == null)
            return null; // or throw

        // Step 3: deserialize
        if (gameInfoElement.Name.LocalName == "GameTurnModel")
        {
            var serializer = new XmlSerializer(typeof(GameTurnModel));
            using var reader = gameInfoElement.CreateReader();
            var model = (GameTurnModel)serializer.Deserialize(reader);

            // model now contains your metadata
            return model;
        }

        return null;
    }
    public async Task DeleteNode(string id)
    {
        // <iq type='set' to='pubsub.example.com' id='delete1'>
        //     <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
        //     <delete node='shadow-empire/game-1234'/>
        //     </pubsub>
        //     </iq>

        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new XmppDotNet.Xmpp.PubSub.Owner.PubSub();
        var publish = new Delete { Node = "game/" + id };

        pubsub.Add(publish);
        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);
    }

    internal async Task DownloadFile(GameInfoModel model, GameType type)
    {
        var url = model.GameTurnModel.Url;

        using var http = new HttpClient();

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        // Where do we store it?

        var destination = Path.Combine(type.Savegames, model.FileName);

        await using var remote = await response.Content.ReadAsStreamAsync();
        await using var local = File.Create(destination);

        await remote.CopyToAsync(local);

        Debug.WriteLine($"Saved: {destination}");
    }
}
