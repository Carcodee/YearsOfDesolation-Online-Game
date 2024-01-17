using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class MapLogic:INetworkSerializable
{
    public int numberOfPlayers;
    public int numberOfPlayersAlive;
    public float zoneRadius;
    public float zoneRadiusExpandSpeed;
    public float totalTime;
    public float enemiesSpawnRate;
    public bool isBattleRoyale;
    public int damagePerTick;

      
    public MapLogic()
    {

    }

    public void SetMap(int numberOfPlayers, int numberOfPlayersAlive, float zoneRadiusExpandSpeed, float totalTime, float enemiesSpawnRate, float zoneRadius)
    {
        this.numberOfPlayers = numberOfPlayers;
        this.numberOfPlayersAlive = numberOfPlayersAlive;
        this.zoneRadiusExpandSpeed = zoneRadiusExpandSpeed;
        this.totalTime = totalTime;
        this.enemiesSpawnRate = enemiesSpawnRate;
        this.zoneRadius = zoneRadius;
        this.isBattleRoyale = false;
        this.damagePerTick = 1;
    }


    public void ExpandZone()
    {
        zoneRadius+= zoneRadiusExpandSpeed * Time.deltaTime;
    }
 
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {            

        serializer.SerializeValue(ref numberOfPlayers);
        serializer.SerializeValue(ref numberOfPlayersAlive);
        serializer.SerializeValue(ref zoneRadius);
        serializer.SerializeValue(ref zoneRadiusExpandSpeed);
        serializer.SerializeValue(ref totalTime);
        serializer.SerializeValue(ref enemiesSpawnRate);
        serializer.SerializeValue(ref isBattleRoyale);
        serializer.SerializeValue(ref damagePerTick);
    }

 

}


