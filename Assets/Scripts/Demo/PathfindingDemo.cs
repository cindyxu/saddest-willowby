﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathfindingDemo : MonoBehaviour {

	public float jumpSpd;
	public float walkSpd;
	public float terminalV;
	public float gravity;

	public BoxCollider2D walker;
	public UnityEngine.UI.Button startButton;
	public UnityEngine.UI.Button stepButton;

	private List<Edge> mEdges;
	private Scan mScan;

	private List<GameObject> mDrawSteps = new List<GameObject> ();
	private List<GameObject> mDrawPatches = new List<GameObject> ();

	void Awake () {
		startButton.onClick.AddListener (StartScan);
		stepButton.onClick.AddListener (StepScan);

		EdgeCollider2D[] edgeColliders = FindObjectsOfType<EdgeCollider2D> ();
		mEdges = RoomScanner.CreateEdges (edgeColliders);

		foreach (Edge edge in mEdges) {
			Debug.Log ("created edge: " + edge + " " +
				(edge.isLeft ? "isLeft" : "") +
				(edge.isRight ? "isRight" : "") +
				(edge.isUp ? "isUp" : "") + 
				(edge.isDown ? "isDown" : ""));

			CreateEdge (edge, new Color (1, 1, 1, 0.1f));
		}
	}

	public void StartScan () {
		Edge underEdge = findUnderEdge (mEdges, 
			walker.transform.position.x - walker.size.x/2f, 
			walker.transform.position.x + walker.size.x/2f, 
			walker.transform.position.y);
		if (underEdge != null) {
			mScan = new Scan (new Vector2 (walker.size.x, walker.size.y), walkSpd, -gravity, 
				terminalV, underEdge, walker.transform.position.x - walker.size.x/2f, jumpSpd, mEdges);
		} else {
			mScan = null;
		}
		UpdateGraph ();
	}

	private static Edge findUnderEdge (List<Edge> edges, float x0, float x1, float y) {
		List<Edge> downEdges = edges.FindAll ((Edge edge) => edge.isDown && edge.y0 <= y);
		// descending
		downEdges.Sort ((Edge edge0, Edge edge1) => edge1.y0.CompareTo (edge0.y0));
		foreach (Edge edge in downEdges) {
			if (edge.x0 <= x1 && edge.x1 >= x0) {
				return edge;
			}
		}
		return null;
	}

	public void StepScan () {
		mScan.Step ();
		UpdateGraph ();
	}

	private void UpdateGraph () {
		foreach (GameObject go in mDrawSteps) {
			if (go != null) {
				Destroy (go.transform.root.gameObject);
			}
		}
		mDrawSteps.Clear ();
		foreach (GameObject go in mDrawPatches) {
			if (go != null) {
				Destroy (go.transform.root.gameObject);
			}
		}
		mDrawPatches.Clear ();

		if (mScan != null) {
			Scan.ScanStep[] steps = mScan.GetQueuedSteps ();
			foreach (Scan.ScanStep step in steps) {
				ScanArea area = step.scanArea;
				mDrawSteps.Add (RenderArea (area));
			}
			List<ScanPatch> patches = mScan.GetPatches ();
			foreach (ScanPatch patch in patches) {
				GameObject go = CreatePatchLine (patch);
				GameObject areaChain = RenderArea (patch.scanArea);
				if (areaChain != null) {
					areaChain.transform.parent = go.transform;
				}
				mDrawPatches.Add (go);
			}
		}
	}

	private GameObject RenderArea(ScanArea area) {
		GameObject parentMesh = null;
		GameObject currMesh = null;

		int depth = 0;
		while (area != null) {
			if (area.start != null) {
				GameObject mesh = CreateStepMesh (area, depth);
				if (parentMesh == null) parentMesh = mesh;
				if (currMesh != null) mesh.transform.parent = currMesh.transform;
				currMesh = mesh;
			}
			area = area.parent;
			depth--;
		}
		return parentMesh;
	}

	private GameObject CreatePatchLine (ScanPatch patch) {
		GameObject go = new GameObject ("patch");
		go.AddComponent<LineRenderer> ();
		LineRenderer renderer = go.GetComponent<LineRenderer> ();
		renderer.SetVertexCount (2);
		renderer.SetPositions (new Vector3[] {
			new Vector3 (patch.scanArea.end.xl, patch.edge.y0, 0), 
			new Vector3 (patch.scanArea.end.xr, patch.edge.y0, 0)
		});
		renderer.material = new Material(Shader.Find("Particles/Additive"));
		renderer.SetWidth (0.1f, 0.1f);
		renderer.SetColors (Color.yellow, Color.yellow);
		return go;
	}

	private GameObject CreateEdge (Edge edge, Color color) {
		GameObject go = new GameObject ("patch");
		go.AddComponent<LineRenderer> ();
		LineRenderer renderer = go.GetComponent<LineRenderer> ();
		renderer.SetVertexCount (2);
		renderer.SetPositions (new Vector3[] {
			new Vector3 (edge.x0, edge.y0, 0), 
			new Vector3 (edge.x1, edge.y1, 0)
		});
		renderer.material = new Material(Shader.Find("Particles/Additive"));
		renderer.SetWidth (0.1f, 0.1f);
		renderer.SetColors (color, color);
		return go;
	}

	private GameObject CreateStepMesh (ScanArea area, int depth) {
		GameObject go = new GameObject("step " + depth);
		go.AddComponent<MeshRenderer> ();
		go.AddComponent<MeshFilter> ();
		go.AddComponent<PolyDrawer> ();
		Material material = new Material(Shader.Find("Sprites/Default"));
		List<Vector2> pts = new List<Vector2> ();
		if (area.end.y > area.start.y) {
			pts.Add (new Vector2 (area.end.xl, area.end.y));
			pts.Add (new Vector2 (area.end.xr, area.end.y));
			pts.Add (new Vector2 (area.start.xr, area.start.y));
			pts.Add (new Vector2 (area.start.xl, area.start.y));
		} else {
			pts.Add (new Vector2 (area.start.xl, area.start.y));
			pts.Add (new Vector2 (area.start.xr, area.start.y));
			pts.Add (new Vector2 (area.end.xr, area.end.y));
			pts.Add (new Vector2 (area.end.xl, area.end.y));
		}
		float absv = Mathf.Abs (area.end.vy) * 0.1f;
		float lerpShift = (Mathf.Pow (2, -absv) * (Mathf.Pow (2, absv+1) - 1) - 1) * Mathf.Sign(area.end.vy);
		Color ncolor = Color.HSVToRGB (1 + lerpShift * 0.5f, 1, 1);
		ncolor.a = 0.5f;
		material.color = ncolor;
		go.GetComponent<PolyDrawer> ().RawPoints = pts;
		go.GetComponent<PolyDrawer> ().Mat = material;
		return go;
	}
}