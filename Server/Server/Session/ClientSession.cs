using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;
using Server.Game.Object;
using Server.Game.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player   MyPlayer { get; set; }
		public int      SessionId { get; set; }

        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
		{
            Console.WriteLine($"OnConnected : {endPoint}");

			{
                MyPlayer = ObjectManager.Instance.Add<Player>();
                {
                    MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
                    MyPlayer.Info.PosInfo.State = CState.Idle;
                    MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                    MyPlayer.Info.PosInfo.PosX = 0;
                    MyPlayer.Info.PosInfo.PosY = 0;

                    Stat stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);
                    MyPlayer.Stat.Level = stat.level;
                    MyPlayer.Stat.Hp = stat.maxHp;
                    MyPlayer.Stat.MaxHp = stat.maxHp;
                    MyPlayer.Stat.Attack = stat.attack;
                    MyPlayer.Stat.TotalExp = stat.totalExp;
                    MyPlayer.Stat.Speed = stat.speed;

                    MyPlayer.Session = this;
                }
                RoomManager.Instance.Find(1).EnterGame(MyPlayer);
            }
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Find(1).LeaveGame(MyPlayer.Info.ObjectId);

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
