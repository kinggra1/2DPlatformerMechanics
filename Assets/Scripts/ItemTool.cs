using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTool : Item {

    public enum ToolType { Axe }
    public ToolType toolType = ToolType.Axe;

    override public bool CanUse() {
        return true;
    }


    override public void Use() {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (player != null) {
            player.UseTool(toolType);
        } else {
            Debug.LogError("Unable to find player by tag.");
        }
        
    }
}
