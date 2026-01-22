using System;
using System.Collections.Generic;
using CharacterDefinition;
using HM.Utils;
using UnityEngine;

namespace CharacterSystem
{
    public class BuildWallState : State
    {
        public BuildWallState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override CharacterStateId StateId => CharacterStateId.BuildWall;

        protected override void RegisterConditions()
        {
            
        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            BuildWall();
            _stateMachine.ChangeState(CharacterStateId.Idle);
        }

        public override void Exit(BaseCharacter character)
        {

        }


        private bool BuildWall()
        {
            List<WallData> cadidatWallDatas = new();
            for (int x = GameManager.Instance.MinGridx; x < GameManager.Instance.MaxGridx; x++)
            {
                for (int y = GameManager.Instance.MinGridy; y < GameManager.Instance.MaxGridy; y++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        WallData wall = new WallData(new Vector3(x, y) * 1.3f, i % 2 == 0 ? true : false);

                        if (GameManager.Instance.WallList.Contains(wall)) continue;
                        cadidatWallDatas.Add(wall);
                    }
                }
            }

            int minDamage = Int32.MaxValue;
            int wallIndex = Int32.MinValue;

            List<Vector2Int> characterPositions = _stateMachine.CharacterController.GetAllCharactersPosition();
            
            for (int i = 0; i < cadidatWallDatas.Count; i++)
            {
                var wallData = cadidatWallDatas[i];
                GameManager.Instance.WallList.Add(wallData);

                if (CheckWallIsStuck(characterPositions, GameManager.Instance.WallList)) continue;
                
                GameManager.Instance.TileController.SetDamageFieldGraph(_stateMachine.CharacterController.StageCharacter[
                    _agentCharacter.Playerable ? CharacterIdentification.Player : CharacterIdentification.Enemy]);
                
                int damage = 0;
                foreach (var oppositCharacter in GameManager.Instance.TileController.GetDamageGraphTileCharacters(_agentCharacter.Position))
                {
                    damage += oppositCharacter.Atk;
                }

                if (damage < minDamage)
                {
                    minDamage = damage;
                    wallIndex = i;
                }

                GameManager.Instance.WallList.Remove(wallData);
            }

            if (wallIndex < 0) return false;

            _stateMachine.CharacterController.SetWallInstantly(cadidatWallDatas[wallIndex].Position, cadidatWallDatas[wallIndex].IsHorizontal);
            _agentCharacter.Build();
            return true;
        }
        
        private bool CheckWallIsStuck(List<Vector2Int> CharacterList, List<WallData> WallList)
        {
            var mapGraph = PathFindingUtils.GetMapGraph(CharacterList, WallList, true);
            return PathFindingUtils.CheckStuck(mapGraph);
        }
    }
    
    
}