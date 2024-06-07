using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;
    public PlayerStatsController playerStatsController;
    public PostProccesingManager postProccesingManager;
    
    private void Awake()
    {
        instance = this;
    }
    
    
    
    public void SpawnCoin()
    {
        playerStatsController.SpawnCoin(playerStatsController.coin, playerStatsController.transform.forward * 5);
    }
    
    
    public void LevelUp()
    {
        playerStatsController.LevelUp();
    }
    
    public void FastBattleRoyale()
    {
        GameController.instance.SetMapLogicClientServerRpc(GameController.instance.mapLogic.Value.numberOfPlayers,
                GameController.instance.mapLogic.Value.numberOfPlayersAlive,
                100,
                1,
                GameController.instance.mapLogic.Value.enemiesSpawnRate,
                GameController.instance.zoneRadius);
        GameController.instance.mapLogic.Value.damagePerTick = 5;
        GameController.instance.currentFarmStageTimer = GameController.instance.timeToFarm;
        GameController.instance.started.Value = true;
        GameController.instance.mapLogic.Value.isBattleRoyale = true;
        GameController.instance.waitingTime = 0;
    }
    
    public void FreezeTime()
    {
        GameController.instance.SetMapLogicClientServerRpc(GameController.instance.mapLogic.Value.numberOfPlayers,
            GameController.instance.mapLogic.Value.numberOfPlayersAlive,
            GameController.instance.reduceZoneSpeed,
            999,
            GameController.instance.mapLogic.Value.enemiesSpawnRate,
            GameController.instance.zoneRadius);
    }
    public void FreezeZoneBattleRoyale()
    {
        GameController.instance.SetMapLogicClientServerRpc(GameController.instance.mapLogic.Value.numberOfPlayers,
            GameController.instance.mapLogic.Value.numberOfPlayersAlive,
            0,
            GameController.instance.timeToFarm,
            GameController.instance.mapLogic.Value.enemiesSpawnRate,
            GameController.instance.zoneRadius);
    }
    
    public void ContinueGame()
    {
        GameController.instance.SetMapLogicClientServerRpc(GameController.instance.mapLogic.Value.numberOfPlayers,
            GameController.instance.mapLogic.Value.numberOfPlayersAlive,
            GameController.instance.reduceZoneSpeed,
            GameController.instance.timeToFarm,
            GameController.instance.mapLogic.Value.enemiesSpawnRate,
            GameController.instance.zoneRadius);
    }
    
    
    
}

#endif