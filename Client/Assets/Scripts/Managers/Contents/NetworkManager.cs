using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;

public class NetworkManager
{
	ServerSession _session = new ServerSession();

	public void Send(IMessage sendBuff)
	{
		_session.Send(sendBuff);
	}

	public void Init()
	{
		{
            // 예: 서버 집의 공인 IP
            //string serverIp = "123.45.67.89"; // 실제로는 ipconfig / whatismyip 등으로 확인

            //IPAddress ipAddr = IPAddress.Parse(serverIp);
            //IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            //Connector connector = new Connector();
            //connector.Connect(endPoint,
            //    () => { return _session; },
            //    1);
        }

		// DNS (Domain Name System)
		string host = Dns.GetHostName();
		IPHostEntry ipHost = Dns.GetHostEntry(host);
		// IPAddress ipAddr = ipHost.AddressList[0];
        IPAddress ipAddr = IPAddress.Loopback; // 127.0.0.1
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

		Connector connector = new Connector();

		connector.Connect(endPoint, () => { return _session; }, 1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}	
	}

}
