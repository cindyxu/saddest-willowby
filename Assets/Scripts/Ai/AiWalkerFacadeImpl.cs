﻿using System;
using System.Collections.Generic;
using LRUCache;
using UnityEngine;

public class AiWalkerFacadeImpl : IAiWalkerFacade {

	public event OnAiGroundedEvent onGrounded;

	private readonly WalkerParams mWp;

	private SceneModelConverter mConverter;

	private RoomModel mRoomModel;
	private Dictionary<RoomModel, RoomGraph> mRoomGraphs = new Dictionary<RoomModel, RoomGraph> ();
	private SceneGraph mSceneGraph;

	private LadderModel mLadder;
	private Edge mEdge;

	private InhabitantFacade mFacade;
	private HumanoidFacade mHFacade;

	private bool mObserving = false;

	public AiWalkerFacadeImpl (WalkerParams wp, InhabitantFacade facade, HumanoidFacade hFacade) {
		mWp = wp;
		mFacade = facade;
		mHFacade = hFacade;
	}

	public void Initialize (SceneModelConverter converter) {
		mConverter = converter;
		mSceneGraph = mConverter.CreateSceneGraph (mWp);
		initializeState ();
	}

	public Edge GetEdge () {
		return mEdge;
	}

	public LadderModel GetLadder () {
		return mLadder;
	}

	public RoomModel GetRoomModel () {
		return mRoomModel;
	}

	public Vector2 GetPosition () {
		return mFacade.GetPosition () - new Vector2 (mWp.size.x / 2, mWp.size.y / 2);
	}

	public Vector2 GetVelocity () {
		return mFacade.GetVelocity ();
	}

	public RoomGraph GetRoomGraph (RoomModel room) {
		if (!mRoomGraphs.ContainsKey (room)) {
			mRoomGraphs[room] = new RoomGraph (room, mWp);
		}
		return mRoomGraphs[room];
	}

	public void StartObserving () {
		mHFacade.onClimbLadder += OnClimbLadder;
		mHFacade.onGrounded += OnGrounded;
		mFacade.GetRoomTraveller ().onEnterRoom += OnEnterRoom;
		mObserving = true;
		initializeState ();
	}

	public void StopObserving () {
		mHFacade.onClimbLadder -= OnClimbLadder;
		mHFacade.onGrounded -= OnGrounded;
		mFacade.GetRoomTraveller ().onEnterRoom -= OnEnterRoom;
		mObserving = false;
	}

	public void OnEnterRoom (RoomTraveller traveller, Room room) {
		mRoomModel = mConverter.GetRoomModel (room);
	}

	public void OnClimbLadder (Ladder ladder) {
		if (ladder != null) {
			mLadder = mConverter.GetLadderModel (ladder).Item2;
		} else mLadder = null;
	}

	public void OnGrounded (SortedEdge sortedEdge) {
		if (sortedEdge != null) mEdge = getGroundedEdge (sortedEdge);
		else mEdge = null;
		mLadder = null;
		if (onGrounded != null) onGrounded (mEdge);
	}

	private void initializeState () {
		if (mConverter != null && mObserving) {
			Room room = mFacade.GetRoomTraveller ().GetCurrentRoom ();
			mRoomModel = (room != null ? mConverter.GetRoomModel (room) : null);
			mEdge = mHFacade.GetSortedEdge () != null ? getGroundedEdge (mHFacade.GetSortedEdge ()) : null;
			mLadder = mHFacade.GetLadder () != null ?
				mConverter.GetLadderModel (mHFacade.GetLadder ()).Item2 : null;
		}
	}

	private Edge getGroundedEdge (SortedEdge sortedEdge) {
		Vector2 position = GetPosition ();
		return EdgeUtil.FindOnEdge (mRoomModel.edges,
			position.x, position.x + mWp.size.x, sortedEdge.transform.position.y);
	}

}
