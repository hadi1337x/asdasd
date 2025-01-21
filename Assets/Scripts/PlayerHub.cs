using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHub : MonoBehaviour
{
    public TMP_InputField WorldName;

    public void EnterWorld()
    {
        if(WorldName.text != string.Empty)
        {
            Connection.Packet.currentWorld = WorldName.text;
            Connection.Packet.RequestJoinWorld(WorldName.text);
        }
        else
        {
            Connection.Packet.currentWorld = "START";
            Connection.Packet.RequestJoinWorld("START");
        }
    }
}
