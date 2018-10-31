using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTarget : Shooteable {

	public ButtonCommands Command = ButtonCommands.Restart;

	public override void Hit (float Damage) {
		GetComponentInParent<Group> ().CallCommand (Command);
	}
}

public enum ButtonCommands {
	Restart,
	Stop,
	Start,
	Toggle
}
