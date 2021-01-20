using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Entity
{
    public interface IControllable
    {
        public abstract void ControllerMove(Vector2 vel);
    }
}
