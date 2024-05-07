using UnityEngine.PlayerLoop;

public interface INetObjectToClean
{
    public void CleanData();
    public void OnSpawn();
    bool shutingDown { get; set; }
}