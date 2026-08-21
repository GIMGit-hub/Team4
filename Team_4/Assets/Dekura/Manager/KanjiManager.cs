using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class KanjiManager : MonoBehaviour
{
    [SerializeField] private List<KanjiData> allKanjiDatas;
    [SerializeField] private List<KanjiData> initKanjiDatas;
    [SerializeField] private List<KanjiCombination> kanjiCombinations;

    private Dictionary<KanjiData, bool> kanjiDatas = new Dictionary<KanjiData, bool>();
    private void Awake()
    {
        //kanjiDatasの初期化/全データをfalseに
        foreach (var kanjiData in allKanjiDatas)
        {
            kanjiDatas.TryAdd(kanjiData, false);
        }

        foreach (var kanjiData in initKanjiDatas) 
        {
            ActiveKanji(kanjiData.kanjiName);
        }
    }

    //public float CalculateAtkPower(string name_A,string name_B)
    //{
    //    KanjiData kanji_A;
    //    KanjiData kanji_B;
    //    foreach (var kanjiData in kanjiDatas)
    //    {
    //        if ( == name_A) 

    //    }
    //}

    /// <summary>
    /// 漢字取得時処理。
    /// 失敗 = false
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool ActiveKanji(string name)
    {
        bool tryResult = false;
        foreach (var kanjiData in allKanjiDatas)
        {
            if (kanjiData.kanjiName != name) continue;

            kanjiDatas[kanjiData] = true;
            tryResult = true;
            break;
        }      

        return tryResult;
    }
}
