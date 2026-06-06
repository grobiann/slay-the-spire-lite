using Cysharp.Threading.Tasks;
using STSLite.Core.Maps;
using STSLite.Core.Models;
using STSLite.Core.Random;
using STSLite.Core.Saves;
using STSLite.UI;
using System;
using System.Collections.Generic;
using System.Data;

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

        public event Action? OnRoomEntered;
        public event Action? OnRoomExited;
        public event Action? OnActEntered;


        public void SetupNewSinglePlayer(RunState runState)
        {
            RunState = runState;
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
                await EnterMapCoord(RunState.Map.StartingMapPoint.Coord);
                //    NMapScreen.Instance?.RefreshAllMapPointVotes();
                //}
                //else
                {
                    //await EnterRoomInternal(null);  // TODO: Create room instance
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
            // TODO: Set rng
            ActMap map = new StandardActMap(DefinitionDB.Act<Act1>(), new Rng());
            RunState.Map = map;
            //RunState.RemoveStaleVisitedMapCoords(map);

            UIMapScreen uiMapScreen = UIManager.Instance.Show<UIMapScreen>();
            uiMapScreen.SetMap(RunState.Map, RunState.RngSet.Seed);
            uiMapScreen.ClearDrawings();
        }


        private async UniTask EnterMapCoord(MapCoord coord)
        {
            if (!RunState.Map.ContainsCoord(coord))
            {
                throw new Exception($"Attempted to enter invalid map coord {coord}");
            }
            MapPoint mapPoint = RunState.Map.GetPoint(coord);
            //NMapScreen.Instance?.InitMarker(coord);
            //NMapScreen.Instance?.RefreshAllMapPointVotes();
            await EnterMapPointInternal(coord.Row + 1, mapPoint.PointType);
        }

        private async UniTask EnterMapPointInternal(int actFloor, MapPointType pointType)
        {
            // using(new NetLoadingHandle(NetService))
            {
                //if (State.MapPointHistory.Count > 0)
                //{
                //    UpdatePlayerStatsInMapPointHistory();
                //}
                RunState.ActFloor = actFloor;
                await ExitCurrentRooms();
                //if (preFinishedRoom == null)
                //{
                //    CombatStateSynchronizer.StartSync();
                //}
                ClearScreens();
                //if (preFinishedRoom == null)
                //{
                //    await CombatStateSynchronizer.WaitForSync();
                //}
                //if (saveGame)
                //{
                //    await SaveManager.Instance.SaveRun(null);
                //}
                //if (CombatReplayWriter.IsEnabled)
                //{
                //    CombatReplayWriter.RecordInitialState(ToSave(null));
                //}
                //RoomType roomType;
                //if (pointType == MapPointType.Unknown && preFinishedRoom != null)
                //{
                //    roomType = RoomType.Monster;
                //}
                //else
                //{
                //HashSet<RoomType> blacklist = BuildRoomTypeBlacklist(State.CurrentMapPointHistoryEntry, State.CurrentMapPoint?.Children ?? new HashSet<MapPoint>());
                HashSet<RoomType> blacklist = new HashSet<RoomType>();
                RoomType roomType = RollRoomTypeFor(pointType, blacklist);
                //}
                Room room = CreateRoom(roomType, pointType);
                //AbstractRoom abstractRoom = ((preFinishedRoom == null) ? CreateRoom(roomType, pointType) : preFinishedRoom);
                //ActionExecutor.Pause();
                //if (preFinishedRoom == null)
                //{
                //    State.AppendToMapPointHistory(pointType, abstractRoom.RoomType, abstractRoom.ModelId);
                //}
                //if (abstractRoom is CombatRoom { IsPreFinished: not false, ParentEventId: not null } combatRoom)
                //{
                //    EventRoom room = new EventRoom(ModelDb.GetById<EventModel>(combatRoom.ParentEventId));
                //    await EnterRoomInternal(room, isRestoringRoomStackBase: true);
                //    await EnterRoomInternal(combatRoom);
                //}
                //else
                //{
                await EnterRoomInternal(room);
                //}
                //if (NRun.Instance != null)
                //{
                //    NRun.Instance.GlobalUi.MapScreen.IsTraveling = false;
                //}
                //AfterMapLocationChanged();
                await UIBlackScreen.Off();
                //await FadeIn();
            }
        }

        private RoomType RollRoomTypeFor(MapPointType pointType, HashSet<RoomType> blacklist)
        {
            return pointType switch
            {
                MapPointType.NormalMonster => RoomType.NormalMonster,
                MapPointType.EliteMonster => RoomType.EliteMonster,
                MapPointType.BossMonster => RoomType.BossMonster,
                MapPointType.Shop => RoomType.Shop,
                MapPointType.Rest => RoomType.RestSite,
                MapPointType.Ancient => RoomType.Event,
                MapPointType.Unknown => RunState.OddsSet.UnknownMapPoint.Roll(blacklist, RunState),
                _ => throw new Exception($"Unsupported map point type {pointType}")
            };
        }

        private Room CreateRoom(RoomType roomType, MapPointType pointType)
        {
            return new CombatRoom(RunState);
            switch (roomType)
            {
                case RoomType.NormalMonster:
                case RoomType.EliteMonster:
                case RoomType.BossMonster:
                    //return new CombatRoom();
                case RoomType.Shop:
                    return new MerchantRoom();
                case RoomType.RestSite:
                    return new RestRoom();
                case RoomType.Event:
                    return new EventRoom();
                default:
                    throw new Exception($"Unsupported room type {roomType}");
            }
        }

        private async UniTask EnterRoomInternal(Room room)
        {
            RunState.PushRoom(room);
            await room.Enter(RunState);
            this.OnRoomEntered?.Invoke();
        }

        private async UniTask ExitCurrentRooms()
        {
            while (RunState.CurrentRoomCount > 0)
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