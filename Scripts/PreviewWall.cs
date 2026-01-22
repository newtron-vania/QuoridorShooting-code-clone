using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HM.Utils;

public class PreviewWall : MonoBehaviour
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

    #region Lagacy
    public void SetWallPreview(int x, int y, int rotation, ref GameObject previewObject)
    {
        if (previewObject == null)
        {
            throw new ArgumentNullException("previewObject is null");
        }
        bool? resultOrNull = CanSetWall(x, y, rotation, true);
        if (resultOrNull == null)
        {
            previewObject.SetActive(false);
            return;
        }
        bool result = (bool)resultOrNull;
        previewObject.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0) * GameManager.GridSize;
        previewObject.transform.rotation = Quaternion.Euler(0, 0, rotation * 90);
        previewObject.SetActive(true);
        if (previewObject.TryGetComponent<PreviewWall>(out var previewWall))
        {
            previewWall.IsBlock = !result;
        }
        else
        {
            throw new ArgumentException("previewObject is not a PreviewWall");
        }
    }

    public void SetWallBasedPreview(GameObject previewObject, ref GameObject wallObject)
    {
        if (previewObject == null)
        {
            throw new ArgumentNullException("previewObject is null");
        }
        if (wallObject == null)
        {
            throw new ArgumentNullException("wallObject is null");
        }
        if (previewObject.activeSelf)
        {
            Vector2Int pos = GameManager.ToGridPosition(previewObject.transform.position - new Vector3(0.5f, 0.5f, 0) * GameManager.GridSize);
            int rotation = (int)((previewObject.transform.rotation.eulerAngles.z / 90) % 2);
            Debug.Log($"pos: {pos}, rotation: {rotation}");
            SetWall(pos.x, pos.y, rotation, ref wallObject);
        }
        else
        {
            wallObject.SetActive(false);
        }
    }

    public void SetWall(int x, int y, int rotation, ref GameObject wallObject)
    {
        if (wallObject == null)
        {
            throw new ArgumentNullException("wallObject is null");
        }
        bool? resultOrNull = CanSetWall(x, y, rotation);
        if (resultOrNull == null)
        {
            throw new ArgumentOutOfRangeException("좌표가 범위를 벗어났습니다.");
        }
        bool result = (bool)resultOrNull;
        if (result)
        {
            wallObject.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0) * GameManager.GridSize;
            wallObject.transform.rotation = Quaternion.Euler(0, 0, rotation * 90);
            wallObject.SetActive(true);
            WallObjectList.Add(wallObject);

            _unavailableWallInfoList.Add(new int[] { x, y, rotation });
            _unavailableWallInfoList.Add(new int[] { x, y, (rotation + 1) % 2 });
            if (rotation == 0)
            {
                _unavailableWallInfoList.Add(new int[] { x, y + 1, rotation });
                _unavailableWallInfoList.Add(new int[] { x, y - 1, rotation });
            }
            else
            {
                _unavailableWallInfoList.Add(new int[] { x + 1, y, rotation });
                _unavailableWallInfoList.Add(new int[] { x - 1, y, rotation });
            }
        }
        else
        {
            throw new ArgumentException("설치할 수 없는 벽 구조입니다.");
        }
    }

    public bool? CanSetWall(int x, int y, int rotation, bool isFake = false)
    {
        if (x < -4 || x > 3 || y < -4 || y > 3)
        {
            return null;
        }
        if (_unavailableWallInfoList.Any(wallInfo => wallInfo.SequenceEqual(new int[] { x, y, rotation })))
        {
            return false;
        }
        int[,] tempMapGraph = (int[,])MapGraph.Clone();
        if (rotation == 0) // 세로 벽이면
        {
            int wallGraphPosition = (y + 4) * 9 + x + 4; // 벽좌표를 그래프좌표로 변환
                                                         // 벽 넘어로 못넘어가게 그래프에서 설정
            tempMapGraph[wallGraphPosition, wallGraphPosition + 1] = 0;
            tempMapGraph[wallGraphPosition + 1, wallGraphPosition] = 0;
            tempMapGraph[wallGraphPosition + 9, wallGraphPosition + 10] = 0;
            tempMapGraph[wallGraphPosition + 10, wallGraphPosition + 9] = 0;
        }
        if (rotation == 1) // 가로 벽이면
        {
            int wallGraphPosition = (y + 4) * 9 + x + 4;// 벽좌표를 그래프좌표로 변환
                                                        // 벽 넘어로 못넘어가게 그래프에서 설정
            tempMapGraph[wallGraphPosition, wallGraphPosition + 9] = 0;
            tempMapGraph[wallGraphPosition + 9, wallGraphPosition] = 0;
            tempMapGraph[wallGraphPosition + 1, wallGraphPosition + 10] = 0;
            tempMapGraph[wallGraphPosition + 10, wallGraphPosition + 1] = 0;
        }

        if (!_isOverlap)
        {
            if (!isFake) MapGraph = tempMapGraph.Clone() as int[,];
            return true;
        }
        var mapGraph = PathFindingUtils.GetMapGraph(GameManager.Instance.CharacterController.GetAllCharactersPosition(), GameManager.Instance.WallList, true);
        if (!PathFindingUtils.CheckStuck(mapGraph))
        {
            if (!isFake) MapGraph = tempMapGraph.Clone() as int[,];
            return true;
        }
        return false;
    }
    #endregion
}