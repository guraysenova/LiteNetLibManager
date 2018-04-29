﻿using System;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

namespace LiteNetLibHighLevel
{
    public abstract class LiteNetLibSyncField : LiteNetLibElement
    {
        public SendOptions sendOptions;
        [Tooltip("Interval to send network data")]
        [Range(0f, 2f)]
        public float sendInterval = 0.1f;
        [Tooltip("If this is TRUE, this will update for owner object only")]
        public bool forOwnerOnly;
        public bool hasUpdate { get; protected set; }
        protected float lastSentTime;

        internal void NetworkUpdate()
        {
            if (!ValidateBeforeAccess())
                return;

            if (Time.realtimeSinceStartup - lastSentTime < sendInterval)
                return;

            lastSentTime = Time.realtimeSinceStartup;
            SendUpdate();
        }

        internal abstract void SendUpdate();
        internal abstract void SendUpdate(NetPeer peer);
        internal abstract void Deserialize(NetDataReader reader);
        internal abstract void Serialize(NetDataWriter writer);
    }
    
    public class LiteNetLibSyncField<TField, TFieldType> : LiteNetLibSyncField
        where TField : LiteNetLibNetField<TFieldType>, new()
    {
        public Action<TFieldType> onChange;

        protected TField field;
        public TField Field
        {
            get
            {
                if (field == null)
                    field = new TField();
                return field;
            }
        }

        [LiteNetLibReadOnlyAttribute, SerializeField]
        protected TFieldType value;
        public TFieldType Value
        {
            get { return value; }
            set
            {
                if (!ValidateBeforeAccess())
                    return;

                if (Field.IsValueChanged(value))
                {
                    Field.Value = this.value = value;
                    hasUpdate = true;
                    if (onChange != null)
                        onChange(value);
                }
            }
        }

        public static implicit operator TFieldType(LiteNetLibSyncField<TField, TFieldType> field)
        {
            return field.Value;
        }

        protected override bool ValidateBeforeAccess()
        {
            if (Behaviour == null)
            {
                Debug.LogError("[LiteNetLibElement] Error while set value, behaviour is empty");
                return false;
            }
            if (!Behaviour.IsServer)
            {
                Debug.LogError("[LiteNetLibElement] Error while set value, not the server");
                return false;
            }
            return true;
        }

        internal override sealed void SendUpdate()
        {
            if (!hasUpdate)
                return;

            if (!ValidateBeforeAccess())
                return;

            var manager = Manager;
            if (!manager.IsServer)
                return;

            hasUpdate = false;
            var peers = manager.Peers;
            if (forOwnerOnly)
            {
                var connectId = Behaviour.ConnectId;
                NetPeer foundPeer;
                if (peers.TryGetValue(connectId, out foundPeer))
                    SendUpdate(foundPeer);
            }
            else
            {
                var peerValues = peers.Values;
                foreach (var peer in peerValues)
                {
                    if (Behaviour.Identity.IsSubscribedOrOwning(peer.ConnectId))
                        SendUpdate(peer);
                }
            }
        }

        internal override sealed void SendUpdate(NetPeer peer)
        {
            if (!ValidateBeforeAccess())
                return;

            var manager = Manager;
            if (!manager.IsServer)
                return;

            manager.SendPacket(sendOptions, peer, LiteNetLibGameManager.GameMsgTypes.ServerUpdateSyncField, SerializeForSend);
        }

        protected void SerializeForSend(NetDataWriter writer)
        {
            LiteNetLibElementInfo.SerializeInfo(GetInfo(), writer);
            Serialize(writer);
        }

        internal override sealed void Deserialize(NetDataReader reader)
        {
            Field.Deserialize(reader);
            value = Field.Value;
            if (onChange != null)
                onChange(value);
        }

        internal override sealed void Serialize(NetDataWriter writer)
        {
            Field.Value = value;
            Field.Serialize(writer);
        }
    }

    #region Implement for general usages and serializable
    [Serializable]
    public class SyncFieldBool : LiteNetLibSyncField<NetFieldBool, bool>
    {
    }

    [Serializable]
    public class SyncFieldByte : LiteNetLibSyncField<NetFieldByte, byte>
    {
    }

    [Serializable]
    public class SyncFieldChar : LiteNetLibSyncField<NetFieldChar, char>
    {
    }

    [Serializable]
    public class SyncFieldColor : LiteNetLibSyncField<NetFieldColor, Color>
    {
    }

    [Serializable]
    public class SyncFieldDouble : LiteNetLibSyncField<NetFieldDouble, double>
    {
    }

    [Serializable]
    public class SyncFieldFloat : LiteNetLibSyncField<NetFieldFloat, float>
    {
    }

    [Serializable]
    public class SyncFieldInt : LiteNetLibSyncField<NetFieldInt, int>
    {
    }

    [Serializable]
    public class SyncFieldLong : LiteNetLibSyncField<NetFieldLong, long>
    {
    }

    [Serializable]
    public class SyncFieldQuaternion : LiteNetLibSyncField<NetFieldQuaternion, Quaternion>
    {
    }

    [Serializable]
    public class SyncFieldSByte : LiteNetLibSyncField<NetFieldSByte, sbyte>
    {
    }

    [Serializable]
    public class SyncFieldShort : LiteNetLibSyncField<NetFieldShort, short>
    {
    }

    [Serializable]
    public class SyncFieldString : LiteNetLibSyncField<NetFieldString, string>
    {
    }

    [Serializable]
    public class SyncFieldUInt : LiteNetLibSyncField<NetFieldUInt, uint>
    {
    }

    [Serializable]
    public class SyncFieldULong : LiteNetLibSyncField<NetFieldULong, ulong>
    {
    }

    [Serializable]
    public class SyncFieldUShort : LiteNetLibSyncField<NetFieldUShort, ushort>
    {
    }

    [Serializable]
    public class SyncFieldVector2 : LiteNetLibSyncField<NetFieldVector2, Vector2>
    {
    }

    [Serializable]
    public class SyncFieldVector2Int : LiteNetLibSyncField<NetFieldVector2Int, Vector2Int>
    {
    }

    [Serializable]
    public class SyncFieldVector3 : LiteNetLibSyncField<NetFieldVector3, Vector3>
    {
    }

    [Serializable]
    public class SyncFieldVector3Int : LiteNetLibSyncField<NetFieldVector3Int, Vector3Int>
    {
    }

    [Serializable]
    public class SyncFieldVector4 : LiteNetLibSyncField<NetFieldVector4, Vector4>
    {
    }

    [Serializable]
    public class SyncFieldStruct<T> : LiteNetLibSyncField<NetFieldStruct<T>, T>
        where T : struct
    {
    }
    #endregion
}
