using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Protocol;
using Shared.Bits;
using System.Diagnostics;
using UnityEngine;


namespace Shared.Network
{
    public static class ClientMessageSender
    {
        public static void SendLetsStartPvPGame(NetPeer serverPeer, ushort carKind, ushort dieEffectId, ushort trailId, string nickname) {
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected) {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 StartPvP 전송 실패");
                return;
            }

            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.LetsStart, 4); // 4비트 사용
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            writer.Put(carKind);
            writer.Put(dieEffectId);
            writer.Put(trailId);
            writer.Put(nickname);

            serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            UnityEngine.Debug.Log($"[Sender] sending /LetsStart/ packet: {(PacketType)packet[0]} | car: {carKind}, effect: {dieEffectId}, trail: {trailId}, name: {nickname}");
        }

        public static void SendClientIsReady(NetPeer serverPeer)
        {
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected)
            {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 입력 전송 실패");
                return;
            }
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.ClientIsReady, 4);

            byte[] packet = packetMaking.ToArray();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            UnityEngine.Debug.Log("[Sender] sending /ClientIsReady/ packet: " + (PacketType)packet[0]);
        }
        public static void SendStopFinding(NetPeer serverPeer)
        {
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected)
            {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 StartPvP 전송 실패");
                return;
            }
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.StopFinding, 4);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            UnityEngine.Debug.Log("[Sender] sending /StopFinding/ packet: " + (PacketType)packet[0]);
        }
        // 추가 메시지 전송 메서드들 계속 추가 가능
        public static void SendPlayerInput(NetPeer serverPeer, byte inputBits)
        {
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected)
            {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 입력 전송 실패");
                return;
            }
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.PlayerInput, 4);
            packetMaking.WriteBits((int)inputBits & 0b111, 3); // 0b111 for the double check

            byte[] packet = packetMaking.ToArray();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            serverPeer.Send(writer, DeliveryMethod.Unreliable);
            UnityEngine.Debug.Log("[Sender] sending /PlayerInput/ packet: " + (PacketType)packet[0]);
        }
        public static void SendPlayerEffect(NetPeer serverPeer, EffectType effectType)
        {
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected)
            {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 입력 전송 실패");
                return;
            }
            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.Effect, 4);
            packetMaking.WriteBits((int)effectType & 0b111, 3); // 0b111 for the double check

            byte[] packet = packetMaking.ToArray();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            serverPeer.Send(writer, DeliveryMethod.ReliableUnordered);
            UnityEngine.Debug.Log("[Sender] sending /Effect/ packet: " + (PacketType)packet[0]);
        }
        public static void SendPlayerTransform(NetPeer serverPeer, Transform carTransform)
        {
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected)
            {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 입력 전송 실패");
                return;
            }
            Vector3 pos = carTransform.position;
            Quaternion rot = carTransform.rotation;

            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.TransformUpdate, 4);
            byte[] packet = packetMaking.ToArray();

            NetDataWriter writer = new NetDataWriter();

            //Packet Type
            writer.Put(packet);

            //Position
            writer.Put(pos.x);
            writer.Put(pos.y);
            writer.Put(pos.z);

            //Rotation
            writer.Put(rot.x);
            writer.Put(rot.y);
            writer.Put(rot.z);
            writer.Put(rot.w);

            serverPeer.Send(writer, DeliveryMethod.Unreliable);
            UnityEngine.Debug.Log("[Sender] sending /PositionUpdate/ packet: " + (PacketType)packet[0]);
        }

        public static void SendScoreToServer(int score)
        {
            var serverPeer = LiteNetLibManager.Instance.GetServerPeer();
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected)
            {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] 서버에 연결되지 않아 점수 전송 실패");
                return;
            }

            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.ScoreUpdate, 4);
            packetMaking.WriteBits(score, 16); // 16bit 정수 점수 전송

            byte[] packet = packetMaking.ToArray();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);
            serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);

            UnityEngine.Debug.Log($"[Sender] ScoreUpdate sent: {score}");
        }

        public static void SendReachedFinishLine(ushort score) {
            var serverPeer = LiteNetLibManager.Instance.GetServerPeer();
            if (serverPeer == null || serverPeer.ConnectionState != ConnectionState.Connected) {
                UnityEngine.Debug.LogWarning("[ClientMessageSender] Failed to send finish line message: not connected to server.");
                return;
            }

            var packetMaking = new BitWriter();
            packetMaking.WriteBits((int)PacketType.ReachedFinishLine, 4); // 4-bit packet type header

            byte[] packet = packetMaking.ToArray();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(packet);         // packet header
            writer.Put(score);          // score as ushort

            serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);

            UnityEngine.Debug.Log($"[Sender] ReachedFinishLine sent: score={score}");
        }
    }
}
