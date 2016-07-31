﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Priority_Queue;

public class AStarEdgeSearch {

	private readonly FastPriorityQueue<EdgeNode> mOpenQueue;
	private readonly Dictionary<EdgePath, EdgeHeuristicRange<EdgeNode>> mBestHeuristics = 
		new Dictionary<EdgePath, EdgeHeuristicRange<EdgeNode>> ();

	private readonly Dictionary<Edge, List<EdgePath>> mEdgePaths;
	private readonly Edge mStart;
	private readonly float mStartX;
	private readonly Edge mDest;
	private readonly float mDestX;

	private readonly WalkerParams mWp;
	private readonly WalkerHeuristic mHeuristic;
	private List<EdgePath> mPathChain;

	public AStarEdgeSearch (Dictionary <Edge, List<EdgePath>> edgePaths, WalkerParams wp, 
		Edge start, float startX, Edge dest, float destX) {

		Assert.AreNotEqual (start, null);
		Assert.AreNotEqual (dest, null);

		Log.logger.Log (Log.AI_SEARCH, "<b>starting Astar: from " + start + " to " + dest + ",</b>");

		mWp = wp;
		mHeuristic = new WalkerHeuristic (wp);
		mOpenQueue = new FastPriorityQueue<EdgeNode> (edgePaths.Count * edgePaths.Count);
		mEdgePaths = edgePaths;
		mStart = start;
		mStartX = startX;
		mDest = dest;
		mDestX = destX;

		startSearch ();
	}

	private void startSearch () {
		EdgeNode startNode = new EdgeNode (null, mStart, mStartX, 
			mStartX + mWp.size.x, 0);
		mOpenQueue.Enqueue (startNode, mHeuristic.EstRemainingTime (mStart, startNode.xlf, 
			startNode.xrf, mDest, mDestX));
	}

	public IEnumerator<EdgeNode> getQueueEnumerator () {
		return mOpenQueue.GetEnumerator ();
	}

	public EdgeNode peekQueue () {
		return mOpenQueue.First;
	}

	public List<EdgePath> reconstructChain (EdgeNode end) {
		List<EdgePath> chain = new List<EdgePath> ();
		EdgeNode curr = end;
		while (curr != null && curr.edgePath != null) {
			chain.Add (curr.edgePath);
			EdgeHeuristicRange<EdgeNode> ranges = mBestHeuristics [curr.edgePath];
			if (ranges == null) break;
			float xli, xri;
			curr.edgePath.GetStartRange (out xli, out xri);
			int idx = ranges.getMinRangeIndex (delegate (float xl, float xr, EdgeNode pnode) {
				if (pnode == null) return Mathf.Infinity;
				return pnode.g + mHeuristic.GetWalkTime (pnode.xlf, pnode.xrf, xli, xri);
			});
			float rxl, rxr;
			EdgeNode node;
			ranges.getRangeAtIndex (idx, out rxl, out rxr, out node);
			curr = node;
		}
		chain.Reverse ();
		return chain;
	}

	public bool Step (out List<EdgePath> result) {
		Log.logger.Log (Log.AI_SEARCH, "A STAR STEP ********************************");
		Log.logger.Log (Log.AI_SEARCH, "queue has " + mOpenQueue.Count + " items");
		result = null;
		if (mPathChain != null) {
			Log.logger.Log (Log.AI_SEARCH, "already found path");
			result = mPathChain;
			return false;
		}
		if (mOpenQueue.Count == 0) {
			Log.logger.Log (Log.AI_SEARCH, "<b>no paths left</b>");
			return false;
		}

		EdgeNode bestNode = mOpenQueue.Dequeue ();
		if (bestNode.edge == mDest) {
			Log.logger.Log (Log.AI_SEARCH, "<b>found best path!</b>");
			result = mPathChain = reconstructChain (bestNode);
			string s = "";
			foreach (EdgePath path in result) {
				s += path.GetEndEdge () + " ";
			}
			Log.logger.Log (Log.AI_SEARCH, s);
			return false;
		}
		Log.logger.Log (Log.AI_SEARCH, "continue - current edge " + bestNode.edge);
		if (!mEdgePaths.ContainsKey (bestNode.edge)) {
			Log.logger.Log (Log.AI_SEARCH, "no paths from edge!");
			return true;
		}
		List<EdgePath> neighborPaths = mEdgePaths [bestNode.edge];
		Log.logger.Log (Log.AI_SEARCH, neighborPaths.Count + " paths");
		foreach (EdgePath neighborPath in neighborPaths) {
			processNeighborPath (bestNode, neighborPath);
		}
		return true;
	}

	private void getTaperedStartRange (float pxlf, float pxrf, EdgePath neighborPath, 
		out float tnxli, out float tnxri) {

		float nxli, nxri;
		neighborPath.GetStartRange (out nxli, out nxri);

		tnxli = Mathf.Min (Mathf.Max (pxlf, nxli), nxri - mWp.size.x);
		tnxri = Mathf.Min (Mathf.Max (pxrf, nxli + mWp.size.x), nxri);
	}

	private void getTaperedEndRange (EdgePath neighborPath, float xli, float xri, 
		out float xlf, out float xrf) {

		neighborPath.GetEndRange (out xlf, out xrf);
		xlf = Mathf.Max (xlf, xli - neighborPath.GetMovement ());
		xrf = Mathf.Min (xrf, xri + neighborPath.GetMovement ());
	}

	private void processNeighborPath (EdgeNode parentNode, EdgePath neighborPath) {
		Edge endEdge = neighborPath.GetEndEdge ();

		Log.logger.Log (Log.AI_SEARCH, "process path to " + endEdge);
		float xli, xri;
		getTaperedStartRange (parentNode.xlf, parentNode.xrf, neighborPath, out xli, out xri);

		float exli, exri;
		neighborPath.GetStartRange (out exli, out exri);
		Log.logger.Log (Log.AI_SEARCH, "tapered start range: " + xli + ", " + xri);

		float walkTime = mHeuristic.GetWalkTime (parentNode.xlf, parentNode.xrf, xli, xri);
		float tentativeG = parentNode.g + walkTime + 
			neighborPath.GetTravelTime () * neighborPath.GetPenaltyMult ();

		float xlf, xrf;
		getTaperedEndRange (neighborPath, xli, xri, out xlf, out xrf);

		float exlf, exrf;
		neighborPath.GetEndRange (out exlf, out exrf);
		Log.logger.Log (Log.AI_SEARCH, "tapered end range: " + xlf + ", " + xrf);

		if (!mBestHeuristics.ContainsKey (neighborPath)) {
			mBestHeuristics[neighborPath] = new EdgeHeuristicRange<EdgeNode> (exrf - exlf);
		}
		EdgeHeuristicRange<EdgeNode> heuristic = mBestHeuristics [neighborPath];
		bool writeRange, newRange;
		heuristic.addTentativeHeuristic (xlf - exlf, xrf - exlf, parentNode, out writeRange, out newRange);
		if (!newRange) {
			Log.logger.Log (Log.AI_SEARCH, "  did not add new heuristic");
			return;
		}
		EdgeNode node = new EdgeNode (neighborPath, neighborPath.GetEndEdge (), xlf, xrf, tentativeG);

		float f = tentativeG + mHeuristic.EstRemainingTime (neighborPath.GetEndEdge (), xlf, xrf, mDest, mDestX);
		Log.logger.Log (Log.AI_SEARCH, "  new node! " + f);
		mOpenQueue.Enqueue (node, f);
	}
}