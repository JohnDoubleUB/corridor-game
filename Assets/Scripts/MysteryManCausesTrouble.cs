using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MysteryManCausesTrouble : MonoBehaviour, ICustomCorridorEvent
{
    private bool activated;
    public void TriggerCustomEvent()
    {
        if (!activated)
        {
            //Find level 13, layout 9
            var levelAndIndex = CorridorChangeManager.current.LoadedLevels.Select((level, index) => new { level, index}).FirstOrDefault(x => x.level.LevelNumber == 13);

            if (levelAndIndex != null)
            {
                //Get layout and its index
                var layoutAndIndex = levelAndIndex.level.CorridorLayoutData.Select((layout, index) => new { layout, index }).FirstOrDefault(x => x.layout.LayoutID == "13_9");
                if (layoutAndIndex != null)
                {
                    //Get the plinth controller data
                    if (layoutAndIndex.layout.puzzleData.Any())
                    {
                        PlinthControllerData puzzleData = layoutAndIndex.layout.puzzleData[0] as PlinthControllerData;
                        //Get a random placed item if there are any
                        var placedPuzzlePieceToRemove = puzzleData.PlinthItemsPlaced.Select((isPlaced, index) => new { isPlaced, index }).Shuffle().FirstOrDefault(x => x.isPlaced);

                        if (placedPuzzlePieceToRemove != null)
                        {
                            puzzleData.PlinthItemsPlaced[placedPuzzlePieceToRemove.index] = false; //Change this item to not be placed
                            layoutAndIndex.layout.puzzleData[0] = puzzleData; //Set this back into the layout
                            layoutAndIndex.layout.spawnableItems[0] = true; //Set note to appear
                            CorridorChangeManager.current.ReplaceLevelLayoutData(levelAndIndex.index, layoutAndIndex.index, layoutAndIndex.layout); //Replace this data

                            //Find the plinth controller
                            PlinthController plinthController = CorridorChangeManager.current.Levels[levelAndIndex.index].CorridorLayouts[layoutAndIndex.index].PuzzleElements[0] as PlinthController;

                            if (plinthController != null)
                            {
                                //Find the name of the object we just removed from the puzzle
                                string pieceName = plinthController.Plinths[placedPuzzlePieceToRemove.index].RequiredObject.ObjectName;

                                //Find the layout data for the layout we are putting it in
                                var layoutAndIndex2 = levelAndIndex.level.CorridorLayoutData.Select((layout, index) => new { layout, index }).FirstOrDefault(x => x.layout.LayoutID == "13_2");

                                //Find the LayoutHandler so we can find the index of the collected item
                                CorridorLayoutHandler trueLayout = CorridorChangeManager.current.Levels[levelAndIndex.index].CorridorLayouts[layoutAndIndex2.index];
                                var pieceWithIndex = trueLayout.Pickups.Select((item, index) => new { item, index }).FirstOrDefault(x => x.item.PickupItemPrefab.ObjectName == pieceName);

                                layoutAndIndex2.layout.collectedItems.Remove(pieceWithIndex.index);

                                CorridorChangeManager.current.ReplaceLevelLayoutData(levelAndIndex.index, layoutAndIndex2.index, layoutAndIndex2.layout); //Replace this data
                            }

                            //var pickupToPutElsewhere = GameManager.current.PickupablesIndex.Pickupables.Select((item, index) => new { item, index}).FirstOrDefault(x=>x.item.ObjectName == layoutAndIndex.layout.)

                            //Set another peice to hold this piece
                            //var layoutAndIndex2 = levelAndIndex.level.CorridorLayoutData.Select((layout, index) => new { layout, index }).FirstOrDefault(x => x.layout.LayoutID == "13_2");
                            //if()
                        }
                    }
                }
            }

            activated = true;
        }
    }
}
