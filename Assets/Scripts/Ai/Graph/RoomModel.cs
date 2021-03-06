﻿using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class RoomModel {

	private List<Edge> mRawEdges = new List<Edge> ();
	private List<LadderModel> mLadderModels = new List<LadderModel> ();
	private List<DoorModel> mDoorModels = new List<DoorModel> ();

	public RoomModel (IEnumerable<Edge> edges,
		IEnumerable<LadderModel> ladders = null, IEnumerable<DoorModel> doors = null) {

		this.mRawEdges.AddRange (edges);
		if (ladders != null) {
			this.mLadderModels.AddRange (ladders);
		} if (doors != null) {
			this.mDoorModels.AddRange (doors);
		}
	}

	public IEnumerable<Edge> GetRawEdges () {
		return new ReadOnlyCollection<Edge> (mRawEdges);
	}

	public IEnumerable<LadderModel> GetLadders () {
		return new ReadOnlyCollection<LadderModel> (mLadderModels);
	}

	public IEnumerable<DoorModel> GetDoors () {
		return new ReadOnlyCollection<DoorModel> (mDoorModels);
	}

	public LadderModel GetLadder (Vector2 pos) {
		foreach (LadderModel ladderModel in mLadderModels) {
			if (ladderModel.GetRect ().Contains (pos)) {
				return ladderModel;
			}
		}
		return null;
	}
}

