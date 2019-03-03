﻿using System;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

namespace LiteNetLibManager
{
    public abstract class LiteNetLibSyncField : LiteNetLibElement
    {
        public DeliveryMethod deliveryMethod;
        [Tooltip("Interval to send network data")]
        [Range(0.01f, 2f)]
        public float sendInterval = 0.1f;
        [Tooltip("If this is TRUE it will syncing although no changes")]
        public bool alwaysSync;
        [Tooltip("If this is TRUE, this will update to owner client only")]
        public bool forOwnerOnly;
        public bool hasUpdate { get; protected set; }
        protected float lastSentTime;

        internal void NetworkUpdate()
        {
            if (!ValidateBeforeAccess())
                return;

            if (Time.unscaledTime - lastSentTime < sendInterval)
                return;

            lastSentTime = Time.unscaledTime;
            
            SendUpdate();
        }

        internal abstract void SendUpdate();
        internal abstract void SendUpdate(long connectionId);
        internal abstract void SendUpdate(long connectionId, DeliveryMethod deliveryMethod);
        internal abstract void Deserialize(NetDataReader reader);
        internal abstract void Serialize(NetDataWriter writer);
    }
    
    public class LiteNetLibSyncField<TType> : LiteNetLibSyncField
    {
        public Action<TType> onChange;

        [LiteNetLibReadOnly, SerializeField]
        protected TType value;
        public TType Value
        {
            get { return value; }
            set
            {
                if (!ValidateBeforeAccess())
                    return;

                if (IsValueChanged(value))
                {
                    this.value = value;
                    hasUpdate = true;
                    // If never updates, force update it as initialize state
                    if (!updatedOnce)
                        SendUpdate();
                    if (onChange != null)
                        onChange.Invoke(value);
                }
            }
        }

        protected bool updatedOnce;

        protected virtual bool IsValueChanged(TType newValue)
        {
            return value == null || !value.Equals(newValue);
        }

        public static implicit operator TType(LiteNetLibSyncField<TType> field)
        {
            return field.Value;
        }

        protected override bool ValidateBeforeAccess()
        {
            if (Behaviour == null)
            {
                Debug.LogError("[LiteNetLibSyncField] Error while set value, behaviour is empty");
                return false;
            }
            if (!Behaviour.IsServer)
            {
                Debug.LogError("[LiteNetLibSyncField] Error while set value, not the server");
                return false;
            }
            return true;
        }

        internal override sealed void SendUpdate()
        {
            if (!alwaysSync && !hasUpdate)
                return;

            if (!ValidateBeforeAccess())
                return;

            LiteNetLibGameManager manager = Manager;
            if (!manager.IsServer)
                return;

            hasUpdate = false;
            if (forOwnerOnly)
            {
                long connectionId = Behaviour.ConnectionId;
                if (manager.ContainsConnectionId(connectionId))
                {
                    if (!updatedOnce)
                        SendUpdate(connectionId, DeliveryMethod.ReliableOrdered);
                    else
                        SendUpdate(connectionId);
                }
            }
            else
            {
                foreach (long connectionId in manager.GetConnectionIds())
                {
                    if (Behaviour.Identity.IsSubscribedOrOwning(connectionId))
                    {
                        if (!updatedOnce)
                            SendUpdate(connectionId, DeliveryMethod.ReliableOrdered);
                        else
                            SendUpdate(connectionId);
                    }
                }
            }
            updatedOnce = true;
        }

        internal override sealed void SendUpdate(long connectionId)
        {
            SendUpdate(connectionId, deliveryMethod);
        }

        internal override sealed void SendUpdate(long connectionId, DeliveryMethod deliveryMethod)
        {
            if (!ValidateBeforeAccess())
                return;

            if (!Manager.IsServer)
                return;

            Manager.ServerSendPacket(connectionId, deliveryMethod, LiteNetLibGameManager.GameMsgTypes.ServerUpdateSyncField, SerializeForSend);
        }

        protected void SerializeForSend(NetDataWriter writer)
        {
            LiteNetLibElementInfo.SerializeInfo(GetInfo(), writer);
            Serialize(writer);
        }

        internal override sealed void Deserialize(NetDataReader reader)
        {
            DeserializeValue(reader);
            if (onChange != null)
                onChange.Invoke(value);
        }

        internal override sealed void Serialize(NetDataWriter writer)
        {
            SerializeValue(writer);
        }

        protected virtual void DeserializeValue(NetDataReader reader)
        {
            value = (TType)reader.GetValue(typeof(TType));
        }

        protected virtual void SerializeValue(NetDataWriter writer)
        {
            writer.PutValue(value);
        }
    }

    #region Implement for general usages and serializable
    [Serializable]
    public class SyncFieldBool : LiteNetLibSyncField<bool>
    {
    }

    [Serializable]
    public class SyncFieldBoolArray : LiteNetLibSyncField<bool[]>
    {
    }

    [Serializable]
    public class SyncFieldByte : LiteNetLibSyncField<byte>
    {
    }

    [Serializable]
    public class SyncFieldByteArray : LiteNetLibSyncField<byte[]>
    {
    }

    [Serializable]
    public class SyncFieldChar : LiteNetLibSyncField<char>
    {
    }

    [Serializable]
    public class SyncFieldDouble : LiteNetLibSyncField<double>
    {
    }

    [Serializable]
    public class SyncFieldDoubleArray : LiteNetLibSyncField<double[]>
    {
    }

    [Serializable]
    public class SyncFieldFloat : LiteNetLibSyncField<float>
    {
    }

    [Serializable]
    public class SyncFieldFloatArray : LiteNetLibSyncField<float[]>
    {
    }

    [Serializable]
    public class SyncFieldInt : LiteNetLibSyncField<int>
    {
    }

    [Serializable]
    public class SyncFieldIntArray : LiteNetLibSyncField<int[]>
    {
    }

    [Serializable]
    public class SyncFieldLong : LiteNetLibSyncField<long>
    {
    }

    [Serializable]
    public class SyncFieldLongArray : LiteNetLibSyncField<long[]>
    {
    }

    [Serializable]
    public class SyncFieldSByte : LiteNetLibSyncField<sbyte>
    {
    }

    [Serializable]
    public class SyncFieldShort : LiteNetLibSyncField<short>
    {
    }

    [Serializable]
    public class SyncFieldShortArray : LiteNetLibSyncField<short[]>
    {
    }

    [Serializable]
    public class SyncFieldString : LiteNetLibSyncField<string>
    {
    }

    [Serializable]
    public class SyncFieldUInt : LiteNetLibSyncField<uint>
    {
    }

    [Serializable]
    public class SyncFieldUIntArray : LiteNetLibSyncField<uint[]>
    {
    }

    [Serializable]
    public class SyncFieldULong : LiteNetLibSyncField<ulong>
    {
    }

    [Serializable]
    public class SyncFieldULongArray : LiteNetLibSyncField<ulong[]>
    {
    }

    [Serializable]
    public class SyncFieldUShort : LiteNetLibSyncField<ushort>
    {
    }

    [Serializable]
    public class SyncFieldUShortArray : LiteNetLibSyncField<ushort[]>
    {
    }

    [Serializable]
    public class SyncFieldColor : LiteNetLibSyncField<Color>
    {
    }

    [Serializable]
    public class SyncFieldQuaternion : LiteNetLibSyncField<Quaternion>
    {
    }

    [Serializable]
    public class SyncFieldVector2 : LiteNetLibSyncField<Vector2>
    {
    }

    [Serializable]
    public class SyncFieldVector2Int : LiteNetLibSyncField<Vector2Int>
    {
    }

    [Serializable]
    public class SyncFieldVector3 : LiteNetLibSyncField<Vector3>
    {
    }

    [Serializable]
    public class SyncFieldVector3Int : LiteNetLibSyncField<Vector3Int>
    {
    }

    [Serializable]
    public class SyncFieldVector4 : LiteNetLibSyncField<Vector4>
    {
    }

    [Serializable]
    public class SyncFieldPackedUShort : LiteNetLibSyncField<PackedUShort>
    {
    }

    [Serializable]
    public class SyncFieldPackedUInt : LiteNetLibSyncField<PackedUInt>
    {
    }

    [Serializable]
    public class SyncFieldPackedULong : LiteNetLibSyncField<PackedULong>
    {
    }
    #endregion
}
