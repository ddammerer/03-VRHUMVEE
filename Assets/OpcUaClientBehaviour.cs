// Copyright (c) Traeger Industry Components GmbH. All Rights Reserved.

using System;

using UnityEngine;
using UnityEngine.UI;

using Opc.UaFx;
using Opc.UaFx.Client;

public class OpcUaClientBehaviour : MonoBehaviour
{
    private OpcClient client;
    private Text statusText;
    private Text statusText4;
    private Text statusText3;
    private OpcSubscription subscription;


    /// <summary>
    /// This sample demonstrates how to implement a primitive OPC UA client in Unity.
    /// </summary>
    /// <remarks>Start is called before the first frame update.</remarks>
    void Start()
    {
        this.statusText = GameObject.Find("statusText").GetComponent<Text>();
        this.statusText4 = GameObject.Find("statusText4").GetComponent<Text>();
        this.statusText3 = GameObject.Find("statusText3").GetComponent<Text>();
        this.statusText.text = "Connecting...";
        this.statusText4.text = "Connecting4...";
        this.statusText3.text = "Info3...";

        try
        {
            // this.client = new OpcClient("opc.tcp://localhost:4840/");
            this.client = new OpcClient("opc.tcp://192.168.1.60:4840");
            this.client.Security.UserIdentity = new OpcClientIdentity("opcuser1", ".opcuser1");


            this.client.Connect();
            this.statusText.text = "Connected!";

            //this.client.SubscribeDataChange("ns=6;i=300001", HandleDataChanged);

/*            string[] nodeIds = {
                    "ns=6;i=300001",
                    "ns=6;i=300007",
                    "ns=6;i=300008"
             };*/

            string[] nodeIds = {
                    "ns=6;s=::opctest:mySinValue",             //::opctest:mySinValue
                    "ns=6;s=::AsGlobalPV:gSchweibsChange",
                    "ns=6;s=::AsGlobalPV:gSchweibsWrite",
                    "ns=6; s =::room1:Lampe",
                    "ns=6;s=::room1:SwitchValueW",
                    "ns=6;s=::room1:SwitchValue"

             };




            // Create an (empty) subscription to which we will addd OpcMonitoredItems.
            this.subscription = this.client.SubscribeNodes();

            for (int index = 0; index < nodeIds.Length; index++)
            {
                // Create an OpcMonitoredItem for the NodeId.
                var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;

                // You can set your own values on the "Tag" property
                // that allows you to identify the source later.
                item.Tag = index;

                // Set a custom sampling interval on the 
                // monitored item.
                item.SamplingInterval = 200;

                // Add the item to the subscription.
                this.subscription.AddMonitoredItem(item);
            }

            // After adding the items (or configuring the subscription), apply the changes.
            this.subscription.ApplyChanges();



            this.statusText3.text = "Subscribed!";
        }
        catch (Exception ex) {
            if (ex is TypeInitializationException tiex)
                ex = tiex.InnerException;

            this.statusText.text += Environment.NewLine
                    + ex.GetType().Name
                    + ": " + ex.Message
                    + Environment.NewLine
                    + ex.StackTrace;
        }
    }




    public void chatButton()
    {
        Debug.Log("Button yo");
        Console.WriteLine("Button");
        OpcStatus status = this.client.WriteNode("ns=6;s=::AsGlobalPV:gSchweibsWrite", (float)DateTime.Now.Second);

        if (DateTime.Now.Second % 2 == 0)
            { status = this.client.WriteNode("ns=6;s=::room1:SwitchValueW",(Boolean)true); }
        else
        { status = this.client.WriteNode("ns=6;s=::room1:SwitchValueW", (Boolean)false); }

        //Console.WriteLine("Write Status: " + status);
        //Debug.Log("Write Status: " + status);

    }

    // Update is called once per frame
    void Update()
    {

        //this.statusText = GameObject.Find("statusText").GetComponent<Text>();
        //Vector3 temp = new Vector3(0.1  f, 0, 0);
        //this.statusText.transform.position += temp;

        //OpcStatus status = this.client.WriteNode("ns=6;s=::AsGlobalPV:gSchweibsWrite", (float)DateTime.Now.Second);
        //Console.WriteLine("Write Status: " + status);
        //Debug.Log("Write Status: " + status);


    }

    void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
    {
        // The tag property contains the previously set value.
        OpcMonitoredItem item = (OpcMonitoredItem)sender;

        if (item.NodeId.ToString().Contains("ns=6;s=::AsGlobalPV:gSchweibsChange"))
        { 
            this.statusText.text = e.Item.Value.Value?.ToString() ?? "null";

        }
        else if (item.NodeId.ToString().Contains("ns=6;s=::opctest:mySinValue"))
        {
            this.statusText4.text = e.Item.Value.Value?.ToString() ?? "null";

        }
        else
        {
            //this.statusText.text = item.NodeId.ToString();

            Console.WriteLine(
                        "Data Change from Index {0}: {1}: {2}: {3}",
                        item.Tag,
                        
                        item.NodeId.ToString(),
                        e.Item.Value,
                        e.Item.Value.DataType);

            Debug.Log("Data Change from Index : " +
                    item.Tag + " : " + item.NodeId.ToString() + " : " + e.Item.Value + ":" + e.Item.Value.DataType);

        }


    }
}
