using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[Serializable]
public class SupplymentPool
{
    private List<SupplymentData> _normalShuffleList;
    private List<SupplymentData> _rareShuffleList;
    private List<SupplymentData> _uniqueShuffleList;

    private List<SupplymentData> _normalData = new();
    private List<SupplymentData> _rareData   = new();
    private List<SupplymentData> _uniqueData = new();

    public SupplymentPool()
    {
        DataManager dataManager = DataManager.Instance;
        int supplyDatasCount = dataManager.SupplyDatasCount;
        SupplymentData supplyData;

        for (int i = 1; i <= supplyDatasCount; i++)
        {
            supplyData = dataManager.GetSupplyData(i);

            switch (supplyData.Grade)
            {
                case SupplymentData.SupplyGrade.Normal:
                    _normalData.Add(supplyData);
                    break;
                case SupplymentData.SupplyGrade.Rare:
                    _rareData.Add(supplyData);
                    break;
                case SupplymentData.SupplyGrade.Unique:
                    _uniqueData.Add(supplyData);
                    break;
            }
        }
    }

    public void ListShuffle()
    {
        RefillAndShuffle(_normalShuffleList, _normalData);
        RefillAndShuffle(_rareShuffleList, _rareData);
        RefillAndShuffle(_uniqueShuffleList, _uniqueData);
    }

    private void RefillAndShuffle(List<SupplymentData> targetList, List<SupplymentData> sourceList)
    {
        targetList.Clear();
        targetList.AddRange(sourceList);
        ListUtil.Shuffle(targetList);
    }

    /// <summary>
    /// 보급품 데이터 반환
    /// </summary>
    /// <param name="grade"></param>
    /// <returns></returns>
    public SupplymentData Pop(GradeType grade)
    {
        List<SupplymentData> supplymentList = null;

        switch (grade)
        {
            case GradeType.Normal:
                supplymentList = _normalShuffleList;
                break;
            case GradeType.Rare:
                supplymentList = _rareShuffleList;
                break;
            case GradeType.Unique:
                supplymentList = _uniqueShuffleList;
                break;
        }

        if (supplymentList == null || supplymentList.Count == 0)
        {
            return null;
        }

        int end = supplymentList.Count - 1;
        SupplymentData supplyData = supplymentList[end];
        supplymentList.RemoveAt(end);

        return supplyData;
    }
}