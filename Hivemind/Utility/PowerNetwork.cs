using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public enum NodeType
    {
        WIRE,
        PRODUCER,
        CONSUMER,
        STORAGE,
        MIXED
    }

    public class PowerNetwork
    {
        public List<IPowerNode> Nodes = new List<IPowerNode>();

        public bool IsActive = false;

        public void Merge(PowerNetwork powerNetwork)
        {
            foreach(IPowerNode n in powerNetwork.Nodes)
            {
                n.OnNetworkLeave();
                n.SetNetwork(this);
            }
        }

        public void AddNode(IPowerNode node)
        {
            if (!Nodes.Contains(node))
            {
                Nodes.Add(node);
                node.OnNetworkJoin();
            }
        }

        public void RemoveNode(IPowerNode node)
        {
            if (Nodes.Contains(node))
            {
                Nodes.Remove(node);
                node.OnNetworkLeave();
            }
        }

        public void PowerOn()
        {
            IsActive = true;

            foreach(IPowerNode n in Nodes)
            {
                n.PowerOn();
            }
        }

        public void PowerOff()
        {
            IsActive = false;

            foreach(IPowerNode n in Nodes)
            {
                n.PowerOff();
            }
        }

        public void Update(GameTime gameTime)
        {
            float consumedPower = 0f;
            float producedPower = 0f;

            foreach (IPowerNode n in Nodes)
            {
                n.UpdatePower();

                switch (n.GetNodeType())
                {
                    case NodeType.PRODUCER:
                        producedPower += n.GetPower();
                        break;
                    case NodeType.CONSUMER:
                        consumedPower += n.GetPower();
                        break;
                }
            }

            if (IsActive)
            {
                if (consumedPower > producedPower)
                    PowerOff();
            }
            else
            {

            }
        }
    }

    public interface IPowerNode
    {
        public abstract NodeType GetNodeType();
        public abstract float GetPower();
        public abstract void UpdatePower();
        public abstract void PowerOn();
        public abstract void PowerOff();

        public abstract void SetNetwork(PowerNetwork powerNetwork);
        public abstract void OnNetworkJoin();
        public abstract void OnNetworkLeave();
    }
}
