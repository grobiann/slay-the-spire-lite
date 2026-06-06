using Cysharp.Threading.Tasks;
using STSLite.Core.Maps;
using STSLite.Core.Runs;

namespace STSLite.Core.Models
{
    public abstract class Room
    {

        public abstract ERoomType RoomType { get; }
        public int Id { get; private set; }
        //public RoomDefinition Definition { get; }

        //public Room(RoomDefinition definition)
        //{
        //    Definition = definition;
        //}

        //public async UniTask Exit() { }

        public async UniTask Enter(RunState runState)
        {
            Id = 0; // runState.GetAndIncrementNextRoomId();
            await EnterInternal(runState);
        }

        public abstract UniTask EnterInternal(RunState runState);
        public abstract UniTask Exit();
        public abstract UniTask Resume();

    }

    public class TreasureRoom : Room
    {
        public override ERoomType RoomType => throw new System.NotImplementedException();

        public override UniTask EnterInternal(RunState runState)
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Exit()
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Resume()
        {
            throw new System.NotImplementedException();
        }
    }

    public class MerchantRoom : Room
    {
        public override ERoomType RoomType => throw new System.NotImplementedException();

        public override UniTask EnterInternal(RunState runState)
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Exit()
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Resume()
        {
            throw new System.NotImplementedException();
        }
    }

    public class EventRoom : Room
    {
        public override ERoomType RoomType => throw new System.NotImplementedException();

        public override UniTask EnterInternal(RunState runState)
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Exit()
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Resume()
        {
            throw new System.NotImplementedException();
        }
    }

    public class RestRoom : Room
    {
        public override ERoomType RoomType => throw new System.NotImplementedException();

        public override UniTask EnterInternal(RunState runState)
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Exit()
        {
            throw new System.NotImplementedException();
        }

        public override UniTask Resume()
        {
            throw new System.NotImplementedException();
        }
    }
}
