﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class WalkerHeuristicTest {

	WalkerParams wp = new WalkerParams (new Vector2 (1, 1), 5, 18, -50, -100);

	[Test]
	public void WalkerHeuristic_getWalkTime_goalToRight_getsCorrectWalkTime()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Assert.AreEqual (1f / wp.walkSpd, heuristic.GetWalkTime (0, 1, 2, 3));
	}

	[Test]
	public void WalkerHeuristic_getWalkTime_goalToLeft_getsCorrectWalkTime()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Assert.AreEqual (1f / wp.walkSpd, heuristic.GetWalkTime (0, 1, -2, -1));
	}

	[Test]
	public void WalkerHeuristic_getWalkTime_goalOverlaps_getsNoWalkTime()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Assert.AreEqual (0, heuristic.GetWalkTime (0, 1, 0.5f, 1.5f));
	}

	[Test]
	public void WalkerHeuristic_estTotalTime_nearAndFarRight_prefersNear()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Edge startEdge0 = new Edge (0, 0, 1, 0);
		Edge startEdge1 = new Edge (3, 0, 4, 0);
		Edge endEdge = new Edge (6, 0, 7, 0);
		float est0 = heuristic.EstTotalTime (startEdge0, startEdge0.left, startEdge0.right, 
			             endEdge, endEdge.left, 0);
		float est1 = heuristic.EstTotalTime (startEdge1, startEdge1.left, startEdge1.right, 
			endEdge, endEdge.left, 0);
		Assert.Less (est1, est0);
	}

	[Test]
	public void WalkerHeuristic_estTotalTime_nearAndFarLeft_prefersNear()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Edge startEdge0 = new Edge (0, 0, 1, 0);
		Edge startEdge1 = new Edge (-3, 0, -2, 0);
		Edge endEdge = new Edge (-6, 0, -5, 0);
		float est0 = heuristic.EstTotalTime (startEdge0, startEdge0.left, startEdge0.right, 
			endEdge, endEdge.left, 0);
		float est1 = heuristic.EstTotalTime (startEdge1, startEdge1.left, startEdge1.right, 
			endEdge, endEdge.left, 0);
		Assert.Less (est1, est0);
	}

	[Test]
	public void WalkerHeuristic_estTotalTime_nearAndFarUp_prefersNear()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Edge startEdge0 = new Edge (0, 0, 1, 0);
		Edge startEdge1 = new Edge (0, 2, 1, 2);
		Edge endEdge = new Edge (0, 4, 1, 4);
		float est0 = heuristic.EstTotalTime (startEdge0, startEdge0.left, startEdge0.right, 
			endEdge, endEdge.left, 0);
		float est1 = heuristic.EstTotalTime (startEdge1, startEdge1.left, startEdge1.right, 
			endEdge, endEdge.left, 0);
		Assert.Less (est1, est0);
	}

	[Test]
	public void WalkerHeuristic_estTotalTime_nearAndFarDown_prefersNear()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Edge startEdge0 = new Edge (0, 0, 1, 0);
		Edge startEdge1 = new Edge (0, -1, 1, -1);
		Edge endEdge = new Edge (0, -2, 1, -2);
		float est0 = heuristic.EstTotalTime (startEdge0, startEdge0.left, startEdge0.right, 
			endEdge, endEdge.left, 0);
		float est1 = heuristic.EstTotalTime (startEdge1, startEdge1.left, startEdge1.right, 
			endEdge, endEdge.left, 0);
		Assert.Less (est1, est0);
	}

	[Test]
	public void WalkerHeuristic_estTotalTime_closeEnoughToJumpTo_hasNoPreference()
	{
		WalkerHeuristic heuristic = new WalkerHeuristic (wp);
		Edge startEdge0 = new Edge (0, 0, 1, 0);
		Edge startEdge1 = new Edge (2, 0, 3, 0);
		Edge endEdge = new Edge (4, 0, 5, 0);
		float est0 = heuristic.EstTotalTime (startEdge0, startEdge0.left, startEdge0.right, 
			endEdge, endEdge.left, 0);
		float est1 = heuristic.EstTotalTime (startEdge1, startEdge1.left, startEdge1.right, 
			endEdge, endEdge.left, 0);
		Assert.AreEqual (est0, est1);
	}
}