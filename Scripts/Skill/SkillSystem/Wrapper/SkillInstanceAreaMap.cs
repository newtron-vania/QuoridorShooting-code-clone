using System.Collections.Generic;
using UnityEngine;

public class SkillInstanceAreaMap 
{
    private Dictionary<Vector2Int, SkillAreaPrefab> _skillAreaMap =new Dictionary<Vector2Int, SkillAreaPrefab>();

    public SkillAreaPrefab GetAreaPrefab(Vector2Int position)
    {
        if (_skillAreaMap.TryGetValue(position, out SkillAreaPrefab areaPrefab))
        {
            return areaPrefab;
        }
        return null;
    }

    public void AddAreaPrefab(Vector2Int position, SkillAreaPrefab areaPrefab)
    {
        _skillAreaMap[position] = areaPrefab;
    }


    public void RemoveAreaPrefab(Vector2Int position)
    {
        if (_skillAreaMap.TryGetValue(position, out var prefab))
        {
            // Pool 반환 처리 노멀이 기본값이라 알아서 잘 처리됨
            PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillArea_Normal).ReturnController(prefab);
            _skillAreaMap.Remove(position);
        }
    }

    public void ClearAll()
    {
        foreach (var prefab in _skillAreaMap.Values)
        {
            PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillArea_Normal).ReturnController(prefab);
        }
        _skillAreaMap.Clear();
    }

}
