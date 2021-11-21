using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.Utility
{
    public class PowerNetwork
    {
        public List<IPowerProducer> Producers = new List<IPowerProducer>();

        public void AddNode(IPowerProducer node)
        {
            if (!Producers.Contains(node))
            {
                Producers.Add(node);
                node.OnNetworkJoin(this);
            }
        }

        public void RemoveNode(IPowerProducer node)
        {
            if (Producers.Contains(node))
            {
                Producers.Remove(node);
                node.OnNetworkLeave(this);
            }
        }

        public List<IPowerConsumer> Consumers = new List<IPowerConsumer>();

        public void AddNode(IPowerConsumer node)
        {
            if (!Consumers.Contains(node))
            {
                Consumers.Add(node);
                node.OnNetworkJoin(this);
            }
        }

        public void RemoveNode(IPowerConsumer node)
        {
            if (Consumers.Contains(node))
            {
                Consumers.Remove(node);
                node.OnNetworkLeave(this);
            }
        }

        public List<IPowerStorage> Storages = new List<IPowerStorage>();

        public void AddNode(IPowerStorage node)
        {
            if (!Storages.Contains(node))
            {
                Storages.Add(node);
                node.OnNetworkJoin(this);
            }
        }

        public void RemoveNode(IPowerStorage node)
        {
            if (Storages.Contains(node))
            {
                Storages.Remove(node);
                node.OnNetworkLeave(this);
            }
        }

        public void Update(GameTime gameTime)
        {
            float RequestedPower = 0f;
            foreach(IPowerConsumer node in Consumers)
            {
                RequestedPower += node.GetRequested();
            }

            float ProducedPower = 0f;
            foreach(IPowerProducer node in Producers)
            {
                ProducedPower += node.GetPower();
            }

            float StoredPower = 0f;
            float StorageCapacity = 0f;
            foreach(IPowerStorage node in Storages)
            {
                StoredPower += node.GetStored();
                StorageCapacity += node.GetCapacity();
            }

            if(ProducedPower >= RequestedPower)
            {
                foreach(IPowerConsumer node in Consumers)
                {
                    node.StorePower(node.GetRequested());
                }

                float considered = 0f;

                foreach (IPowerStorage node in Storages)
                {
                    considered += node.GetCapacity();
                }

                float powertostore = ProducedPower - RequestedPower;
                float perstorage = powertostore / considered;

                foreach(IPowerStorage node in Storages)
                {
                    float nodeSpace = node.GetCapacity() - node.GetStored();
                    if(nodeSpace > 0)
                    {
                        if (nodeSpace < perstorage * node.GetCapacity())
                        {
                            node.StorePower(nodeSpace);
                            powertostore -= nodeSpace;
                            considered -= node.GetCapacity();
                            perstorage = powertostore / considered;
                        }
                        else
                        {
                            node.StorePower(perstorage * node.GetCapacity());
                            powertostore -= perstorage * node.GetCapacity();
                            considered -= node.GetCapacity();
                        }
                    }
                }


            }

            if(ProducedPower < RequestedPower)
            {
                float considered = 0f;

                foreach (IPowerConsumer node in Consumers)
                {
                    considered += node.GetRequested();
                }

                float powertostore = ProducedPower - RequestedPower;
                float perstorage = powertostore / considered;

                //Add if statement for if batteries have power stored
                foreach (IPowerConsumer node in Consumers)
                {
                    if (node.GetRequested() > 0)
                    {
                        if (node.GetRequested() < perstorage * node.GetRequested())
                        {
                            node.StorePower(node.GetRequested());
                            powertostore -= node.GetRequested();
                            considered -= node.GetRequested();
                            perstorage = powertostore / considered;
                        }
                        else
                        {
                            node.StorePower(perstorage * node.GetRequested());
                            powertostore -= perstorage * node.GetRequested();
                            considered -= node.GetRequested();
                        }
                    }
                }
            }

        }
    }

    public interface IPowerProducer
    {
        public abstract float GetPower();
        public abstract float TakePower(float amount);
        public abstract void OnNetworkJoin(PowerNetwork powerNetwork);
        public abstract void OnNetworkLeave(PowerNetwork powerNetwork);
    }

    public interface IPowerConsumer
    {
        public abstract float GetRequested();
        public abstract float GetStored();
        public abstract float StorePower(float amount);
        public abstract void OnNetworkJoin(PowerNetwork powerNetwork);
        public abstract void OnNetworkLeave(PowerNetwork powerNetwork);
    }

    public interface IPowerStorage
    {
        public abstract float GetStored();
        public abstract float GetCapacity();
        public abstract void StorePower(float amount);
        public abstract void TakePower(float amount);
        public abstract void OnNetworkJoin(PowerNetwork powerNetwork);
        public abstract void OnNetworkLeave(PowerNetwork powerNetwork);
    }
}
