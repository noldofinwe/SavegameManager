using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Xml.Linq;
using XmppDotNet;
using XmppDotNet.Xml;
using XmppDotNet.Xmpp;
using XmppDotNet.Xmpp.Client;
using XmppDotNet.Xmpp.PubSub;

public class PubSubManager
{
    private readonly XmppClient _client;
    private readonly string _pubsubService;

    public event EventHandler<PubSubItemEventArgs> OnItemReceived;

    public PubSubManager(XmppClient client, string pubsubService)
    {
        _client = client;
        _pubsubService = pubsubService;

        _client.XmppXElementReceived
            .Where(el => el is Message)
            .Subscribe(el =>
            {
                // handle the message here
                Debug.WriteLine(el.ToString());
            });
    }

    // ------------------------------------------------------------
    // Create a node
    // ------------------------------------------------------------
    public void CreateNode(string node)
    {
        var iq = new Iq("TO", _pubsubService);
        var pubsub = new PubSub();

        var create = new Create { Node = node };
        pubsub.Add(create);

        // Optional: add <configure/> if you want default config
        pubsub.Add(new Configure());

        iq.Add(pubsub);
        _client.SendIqAsync(iq);
    }

    // ------------------------------------------------------------
    // Subscribe to a node
    // ------------------------------------------------------------
    public void Subscribe(string node)
    {
        var iq = new Iq("TO", _pubsubService);
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

    // ------------------------------------------------------------
    // Publish an item
    // ------------------------------------------------------------
    public void Publish(string node, XElement payload, string itemId = null)
    {
        var iq = new Iq("TO", _pubsubService);
        var pubsub = new PubSub();

        var publish = new Publish { Node = node };

        var item = new Item();
        if (!string.IsNullOrEmpty(itemId))
            item.Id = itemId;

        item.Add(payload);
        publish.Add(item);

        pubsub.Add(publish);
        iq.Add(pubsub);

        _client.SendIqAsync(iq);
    }

    // ------------------------------------------------------------
    // Handle incoming <message> with <event/>
    // ------------------------------------------------------------
    private void HandleMessage(object sender, Message msg)
    {
        // var evt = msg.E("event", Event.Ns);
        // if (evt == null)
        //     return;
        //
        // var items = evt.SelectSingleElement("items");
        // if (items == null)
        //     return;
        //
        // foreach (var item in items.GetElements<Item>())
        // {
        //     var payload = item.FirstChild;
        //     if (payload != null)
        //     {
        //         OnItemReceived?.Invoke(this, new PubSubItemEventArgs(
        //             items.Node,
        //             item.Id,
        //             payload
        //         ));
        //     }
        // }
    }
}

// ------------------------------------------------------------
// Event args for received items
// ------------------------------------------------------------
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
