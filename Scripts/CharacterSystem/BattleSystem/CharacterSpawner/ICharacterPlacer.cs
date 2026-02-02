 using System.Collections.Generic;
 using CharacterDefinition;
 using HM.Utils;
 using UnityEngine;

 public interface ICharacterPlacer
 {
     void PlaceCharacter(BaseCharacter character, Vector2Int position);
 }
    
 public class GenericCharacterPlacer : ICharacterPlacer
 {
     private readonly BattleSystem _controller;
     private readonly GameObject _prefab;
     private readonly CharacterIdentification _type;
// Removed the unused _positionStrategy field.
     private readonly List<Vector2Int> _usedPositions;

     public GenericCharacterPlacer(BattleSystem controller, GameObject prefab, CharacterIdentification type, List<Vector2Int> usedPositions)
     {
         _controller = controller;
         _prefab = prefab;
         _type = type;
         _usedPositions = usedPositions;
     }

     public void PlaceCharacter(BaseCharacter character, Vector2Int position)
     {
         if (character == null)
         {
             Debug.LogWarning($"[Warning] GenericCharacterPlacer::PlaceCharacter:spawned character is null! character can't spawn");
             return;
         }
         if (!PathFindingUtils.IsPositionValidWithOffset(position))
         {
             Debug.LogWarning($"[Warning] GenericCharacterPlacer::PlaceCharacter:{position} is not valid");
             return;
         }
         var obj = ResourceManager.Instance.Instantiate(_prefab, GameManager.ToWorldPosition(position));
          new CharacterFactory(_controller).SpawnCharacter(character, obj, position, _controller.CharacterObjects, _controller.StageCharacter);
         _controller.StageCharacter[_type].Add(character);

         if (_type == CharacterIdentification.Player)
         {
             var ui = UIManager.Instance.MakeUIToParent<PlayerActionUI>(obj.transform);
             ui.GetToken = obj;
             ui.SetCharacter(_controller, character as PlayerCharacter);
             (character as PlayerCharacter).ActionUi = ui;
             _controller.CharacterActionUis.Add(ui);
         }
         else if (_type == CharacterIdentification.Enemy)
         {
             _controller._enemyController.RegisterEnemy(character as EnemyCharacter);
         }
     }
 }