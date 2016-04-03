﻿using UnityEngine;
using System.Collections;

public class DialogueActionTrigger : ActionTrigger {

	public string dialogueName;

	public override bool Execute(GameObject gameObject) {
		Cutscene.Builder builder = new Cutscene.Builder ();
		builder.Play (new DialogueCutsceneEvent (GameState.dialogueLibrary.GetDialogue (dialogueName)).StartEvent);
		GameState.cutsceneController.PlayCutscene (builder.Build());
		return true;
	}

	public override int GetPriority() {
		return 1;
	}
}
