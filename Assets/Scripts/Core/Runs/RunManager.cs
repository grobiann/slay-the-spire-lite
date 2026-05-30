using Cysharp.Threading.Tasks;
using STSLite.Core.Models;
using STSLite.UI;
using System;

namespace STSLite.Core.Runs
{
    public class RunManager
    {
        public static RunManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RunManager();
                }
                return _instance;
            }
        }
        private static RunManager _instance;

        private long _startTime;

        public RunState RunState { get; set; }

        public event Action OnRoomEntered;
        public event Action OnRoomExited;
        public event Action OnActEntered;


        public void SetupNewSinglePlayer(RunState runState)
        {
        }

        public void SetupSavedSinglePlayer(RunState runState)
        {
        }

        public void Launch()
        {

        }

        public async UniTask EnterAct(int actIndex)
        {
            //using (new NetLoadingHandle(NetService))
            {
                ClearScreens();
                await ExitCurrentRooms();
                await SetActInternal(actIndex);
                //if (actIndex == 0 && RunState.ExtraFields.StartedWithNeow)
                //{
                //    if (NRun.Instance != null)
                //    {
                //        NMapScreen.Instance?.InitMarker(State.Map.StartingMapPoint.coord);
                //    }
                //    await EnterMapCoord(State.Map.StartingMapPoint.coord);
                //    NMapScreen.Instance?.RefreshAllMapPointVotes();
                //}
                //else
                {
                    await EnterRoomInternal(new Room(DefinitionDB.Room<MapRoomDefinition>()));
                    this.OnActEntered?.Invoke();
                    await UIBlackScreen.Off(UIBlackScreen.DEFAULT_FADE_DURATION);
                }
                //await Hook.AfterActEntered(State);
            }
        }

        private async UniTask SetActInternal(int actIndex)
        {
            RunState.CurrentActIndex = actIndex;
            //RunState.ClearVisitedMapCoordsDebug();
            //RunState.Odds.UnknownMapPoint.ResetToBase();
            //AfterMapLocationChanged();
            await PreloadManager.LoadActAssets(RunState.Act);
            await GenerateMap();
            //NMapScreen.Instance?.SetTravelEnabled(enabled: false);
            //NRunMusicController.Instance?.UpdateMusic();
            //UpdateRichPresence();

        }

        private async UniTask GenerateMap()
        {
            ActMap map = new ActMap();
            RunState.Map = map;
            //RunState.RemoveStaleVisitedMapCoords(map);

            UIMapScreen uiMapScreen = UIManager.Instance.Show<UIMapScreen>();
            uiMapScreen.SetMap(RunState.Map, RunState.RngSet.Seed);
            uiMapScreen.ClearDrawings();
        }

        private async UniTask EnterRoomInternal(Room room)
        {
            //RunState.PushCurrentRoom(room);
            //await room.Enter();
            //this.OnRoomEntered?.Invoke();
        }

        private async UniTask ExitCurrentRooms()
        {
            while(RunState.CurrentRoomCount > 0)
            {
                await ExitCurrentRoom();
            }
        }

        private async UniTask<Room> ExitCurrentRoom()
        {
            Room currentRoom = RunState.PopCurrentRoom();
            await currentRoom.Exit();
            this.OnRoomExited?.Invoke();
            return currentRoom;
        }

        private void ClearScreens()
        {
            //UIManager.Instance.CloseAll();
        }

        public async UniTask FinalizeStartingRelics()
        {

        }

        public void GenerateRooms()
        {

        }
    }
}