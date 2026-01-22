using HM.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHighlightingData : MonoBehaviour
{
    private bool _isOverlap = false;
    public bool IsBlock = false;
    private List<int[]> _unavailableWallInfoList = new List<int[]>(); // 설치할 수 없는 벽 정보 리스트
    public List<GameObject> WallObjectList = new List<GameObject>(); // 설치된 벽 오브젝트 리스트
    public int[,] MapGraph = new int[81, 81]; //DFS용 맵 그래프
    // 벽이 건설 가능 여부 확인 (겹쳐있거나 누군가 갇히게 된다면 false)
    public bool CanBuild
    {
        get
        {
            if (_isOverlap) return false;
            List<WallData> wallList = new List<WallData>(GameManager.Instance.WallList);
            // Vector2Int gridPosition = GameManager.ToGridPosition(transform.position - new Vector3(0.5f, 0.5f, 0) * GameManager.GridSize);
            int rotation = (int)((transform.rotation.eulerAngles.z / 90) % 2);
            wallList.Add(new WallData(transform.position, rotation != 0));
            var mapGraph = PathFindingUtils.GetMapGraph(GameManager.Instance.CharacterController.GetAllCharactersPosition(), wallList, true);
            if (PathFindingUtils.CheckStuck(mapGraph)) return false;
            return true;
        }
    }

    // 다른 벽과 겹치는지 확인
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            _isOverlap = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            _isOverlap = false;
        }
    }
}
