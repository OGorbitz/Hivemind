using Hivemind.World.Entity;
using Hivemind.World.Entity.Moving;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World
{
    public interface Inventory
    {
        /// <summary>
        /// Attempts to withdraw amount <paramref name="a"/> of material <paramref name="m"/>
        /// </summary>
        /// <param name="m">Material type</param>
        /// <param name="a">Amount of material</param>
        /// <returns>Amount of material successfully withdrawn</returns>
        public float Withdraw(Material m, float a);
        public float Deposit(Material m, float a);
        public Dictionary<Material, float> GetMaterials();
        public Point GetPosition();
    }
}
