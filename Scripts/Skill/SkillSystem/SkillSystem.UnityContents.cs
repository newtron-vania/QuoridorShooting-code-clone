using System.Collections.Generic;
using UnityEngine;

public partial class SkillSystem : MonoBehaviour
{
    //private PrefabMakerSystem _prefabMakerSystem;

    private Dictionary<SkillInstance, SkillInstanceAreaMap> _areaPrefabDict = new Dictionary<SkillInstance, SkillInstanceAreaMap>();

    private void ShowSkillHit(SkillInstance skillInstance)
    {
        SkillInstanceAreaMap areaMap = new SkillInstanceAreaMap();
        foreach (var pos in skillInstance.Area)
        {
            SkillAreaPrefab areaPrefabController;
            switch (skillInstance.State)
            {
                case SkillStateType.Normal:
                    areaPrefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillArea_Normal).GetController(pos) as SkillAreaPrefab;
                    break;
                case SkillStateType.Rotten:
                    areaPrefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillArea_Rotten).GetController(pos) as SkillAreaPrefab;
                    break;
                case SkillStateType.TimeDistortion:
                    areaPrefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillArea_TimeDistortion).GetController(pos) as SkillAreaPrefab;
                    break;
                default:
                    areaPrefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillArea_Normal).GetController(pos) as SkillAreaPrefab;
                    break;
            }

            StartCoroutine(areaPrefabController.SkillAreaComponent.ShowZoneHit());
           

            areaMap.AddAreaPrefab(pos, areaPrefabController);

        }
        _areaPrefabDict.Add(skillInstance, areaMap);
    }

    private void ReturnSkillArea(SkillInstance skillInstance, HashSet<Vector2Int> area)
    {
        //UnityContents
        foreach (var pos in area)
        {
            _areaPrefabDict[skillInstance].RemoveAreaPrefab(pos);
        }
    }
}