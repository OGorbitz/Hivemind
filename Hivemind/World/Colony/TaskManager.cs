using Hivemind.World.Tile;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hivemind.World.Colony
{
    public enum TaskType { BUILD, DESTROY, ATTACK, REPAIR }

    public class TaskManager
    {
        public List<BaseTask> Tasks = new List<BaseTask>();
        public TileMap Parent;

        public TaskManager(TileMap parent)
        {
            Parent = parent;
        }

        public void AddTask(BaseTask task)
        {
            Tasks.Add(task);
        }
    }

    public abstract class BaseTask
    {
        public TaskManager Parent;
        public TaskType Type;
        public int WorkRequired, WorkDone;

        public BaseTask(int workRequired, TaskManager parent)
        {
            WorkRequired = workRequired;
            Parent = parent;
        }

        public virtual void DoWork(int work)
        {
            WorkDone += work;
            if (WorkDone > WorkRequired)
                TaskFinished();
        }

        public virtual void TaskFinished()
        {
            Parent.Tasks.Remove(this);
        }
    }

    public class BuildTask : BaseTask
    {
        HoloTile tile;

        public BuildTask(int workRequired, TaskManager parent, BaseTile tile) : base(workRequired, parent)
        {

        }

        public override void TaskFinished()
        {
            base.TaskFinished();
            Parent.Parent.SetTile(tile.Child);
        }
    }
}
