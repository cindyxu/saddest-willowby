﻿using System;
using UnityEngine;

public class PlayerBehaviour : IBehaviour {

	public static readonly PlayerBehaviour instance = new PlayerBehaviour ();

	public void Begin (InputFeedSwitcher inputFeedSwitcher) {
		inputFeedSwitcher.StartPlayerInputFeeder ();
	}
}

