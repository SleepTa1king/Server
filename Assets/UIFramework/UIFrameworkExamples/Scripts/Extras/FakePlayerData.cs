using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils;


public class PlayerDataUpdatedSignal : ASignal<List<PlayerDataEntry>>
{

}

[Serializable]
public class PlayerDataEntry
{
    public string LevelName;
    [Range(0, 3)]
    public int Stars;
}

[CreateAssetMenu(fileName = "PlayerData",menuName = "UI/Fake Player Data")]
public class FakePlayerData : ScriptableObject
{
    [SerializeField]
    private List<PlayerDataEntry> levelProgress = null;

    public List<PlayerDataEntry > LevelProgress
    {
        get { return levelProgress; }
    }

    private void OnValidate()
    {
        Signals.Get<PlayerDataUpdatedSignal>().Dispatch(levelProgress);
    }
}

