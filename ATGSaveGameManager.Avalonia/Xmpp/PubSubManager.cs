using ATGSaveGameManager;
using ATGSaveGameManager.Avalonia.Xmpp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using XmppDotNet;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.PubSub;
using Configure = XmppDotNet.Xmpp.PubSub.Owner.Configure;
using Delete = XmppDotNet.Xmpp.PubSub.Owner.Delete;
using PubSub = XmppDotNet.Xmpp.PubSub.Owner.PubSub;

public class PubSubManager
{
    private readonly XmppClient _client;
    private readonly string _pubsubService;


    public PubSubManager(XmppClient client, string pubsubService)
    {
        _client = client;
        _pubsubService = pubsubService;
    }

    // ----------------- Create node -----------------
    public async Task CreateGame(GameInfoModel gameInfo)
    {
        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService,
            Id = Guid.NewGuid().ToString("N")
        };

        var pubsub = new PubSub();
        var create = new Create { Node = "game/" + gameInfo.Id };

        pubsub.Add(create);
        pubsub.Add(new Configure());   // optional: default config

        iq.Add(pubsub);
        var result = await _client.SendIqAsync(iq);

      //  await Publish(gameInfo.Id, XmppSerializer.ToXElement(gameInfo), "metadata");

    }

    // ----------------- Subscribe -----------------
    public void Subscribe(string node)
    {
        var iq = new Iq
        {
            Type = IqType.Set,
            To = _pubsubService
        };

        var pubsub = new PubSub();
        var subscribe = new Subscribe
        {
            Node = node,
            Jid = _client.Jid
        };

        pubsub.Add(subscribe);
        iq.Add(pubsub);

        _client.SendIqAsync(iq);
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
    public async Task<List<string>> ListNodesAsync()
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

        return result;
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

        var pubsub = new PubSub();
        var publish = new Delete { Node = id };
        
        pubsub.Add(publish);
        iq.Add(pubsub);

        var result = await _client.SendIqAsync(iq);
    }
}

public class PubSubItemEventArgs : EventArgs
{
    public string Node { get; }
    public string ItemId { get; }
    public XElement Payload { get; }

    public PubSubItemEventArgs(string node, string itemId, XElement payload)
    {
        Node = node;
        ItemId = itemId;
        Payload = payload;
    }
}
