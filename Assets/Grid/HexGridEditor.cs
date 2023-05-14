using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
/// <summary>
/// https://answers.unity.com/questions/463207/how-do-you-make-a-custom-handle-respond-to-the-mou.html
/// user: higekun
/// </summary>

[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    Vector3[] positions = new Vector3[4];
    Vector3 OriginPoint;
    private int selectedPointIndex = -1;
    private double lastClickTime;
    private Vector3 lastPosition;

    private GUIStyle style;

    public void OnSceneGUI()
    {
        if (style == null || style.fontSize < 32)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 32;
            style.padding = new RectOffset(0, 0, 0, 0);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
        }
        var t = target as HexGrid;
        var tr = t.transform;
        var pos = tr.position;

        OriginPoint = t.OriginPoint;


        var colorC = new Color(.4f, 0f, 0.8f, .4f);
        Handles.color = colorC;

        Handles.DrawAAConvexPolygon(t.Points.ToArray());
        Handles.color = Color.white;

        EditorGUI.BeginChangeCheck();
        Vector3 newPoint = Handles.PositionHandle(OriginPoint, Quaternion.identity);
        if (style != null)
            Handles.Label(newPoint, "Origin", style);
        else
            Handles.Label(newPoint, "Origin");
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Move Point");
            t.OriginPoint = newPoint;
        }
        positions = t.Points.ToArray();
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 point = positions[i];
            float handleSize = HandleUtility.GetHandleSize(point) * 0.06f;
            double timeSinceLastClick = EditorApplication.timeSinceStartup - lastClickTime;
            if (Handles.Button(point, Quaternion.identity, handleSize, handleSize, Handles.DotHandleCap))
            {

                if (selectedPointIndex == i && timeSinceLastClick < 0.5f)
                {
                    EditorGUI.BeginChangeCheck();
                    Undo.RecordObject(t, "Add Point");
                    List<Vector3> pPoints = positions.Take(i + 1).ToList();
                    Vector3[] nPoints = positions.Skip(i + 1).Take(positions.Length - i - 1).ToArray();
                    Vector3[] newPoints = new Vector3[positions.Length + 1];
                    pPoints.Add(point);
                    pPoints.AddRange(nPoints);
                    newPoints = pPoints.ToArray();
                    positions = newPoints;
                    t.Points.Clear();
                    t.Points.AddRange(positions);
                    selectedPointIndex = i+1;
                    Event.current.Use();
                    break;
                }
                selectedPointIndex = i;
                lastClickTime = EditorApplication.timeSinceStartup;
            }
            if (selectedPointIndex == i && timeSinceLastClick > .5f)
            {
                EditorGUI.BeginChangeCheck();
                newPoint = Handles.PositionHandle(point, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move PolyPoint");
                    t.Points[i] = newPoint ;
                }
            }

        }



    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var t = target as HexGrid;
        var tr = t.transform;
        if (GUILayout.Button("Generate Hex Grid"))
        {
            t.Reset();
            t.Initialize();
            t.AssociateTiles();
        }

    }

    ///HexGrid Generation Area Definition

    public static bool TryPosition(Vector3 position, Vector3[] area)
    {
        var points = new List<Vector3>() { position };
        points.AddRange(area);
        points = SortByDistance(points);
        var triangle = new Vector3[] {points[0], points[1], points[2]};

        int x = 0;
        foreach (Vector3 point in triangle)
        {
            int y = 0;
            var d = Mathf.Infinity;
            var hNearest = point;
            var hFarthest = point;
            foreach (Vector3 copoint in triangle)
            {
                if (copoint == point) { continue; }
                if ((copoint - point).sqrMagnitude < d)
                {
                    hNearest = (copoint - point).normalized;
                }
                else
                {
                    hFarthest = (copoint - point).normalized;
                }
            }
            var angle = Vector3.Angle(hNearest, hFarthest);
            if (Vector3.Angle((point - position).normalized, hNearest) > angle) { y++; }
            if (Vector3.Angle((point - position).normalized, hFarthest) > angle) { y++; }
            if (y >= 1)
            {
                x++;
            }
        }

        return x == 0;
    }
    public static List<Vector3> SortByDistance(List<Vector3> lst)
    {
        List<Vector3> output = new List<Vector3>();
        output.Add(lst[NearestVector3(new Vector3(0, 0), lst)]);
        lst.Remove(output[0]);
        int x = 0;
        for (int i = 0; i < lst.Count + x; i++)
        {
            output.Add(lst[NearestVector3(output[output.Count - 1], lst)]);
            lst.Remove(output[output.Count - 1]);
            x++;
        }
        return output;
    }

    public static int NearestVector3(Vector3 srcPt, List<Vector3> lookIn)
    {
        KeyValuePair<double, int> smallestDistance = new KeyValuePair<double, int>();
        for (int i = 0; i < lookIn.Count; i++)
        {
            double distance = Mathf.Sqrt(Mathf.Pow(srcPt.x - lookIn[i].x, 2) + Mathf.Pow(srcPt.y - lookIn[i].y, 2));
            if (i == 0)
            {
                smallestDistance = new KeyValuePair<double, int>(distance, i);
            }
            else
            {
                if (distance < smallestDistance.Key)
                {
                    smallestDistance = new KeyValuePair<double, int>(distance, i);
                }
            }
        }
        return smallestDistance.Value;
    }
    public static bool IsVector3InPolygon(Vector3 p, Vector3[] polygon)
    {
        float minx = polygon[0].x;
        float maxx = polygon[0].x;
        float minz = polygon[0].z;
        float maxz = polygon[0].z;
        for (int i = 1; i < polygon.Length; i++)
        {
            Vector3 q = polygon[i];
            minx = Mathf.Min(q.x, minx);
            maxx = Mathf.Max(q.x, maxx);
            minz = Mathf.Min(q.z, minz);
            maxz = Mathf.Max(q.z, maxz);
        }

        if (p.x < minx || p.x > maxx || p.z < minz || p.z > maxz)
        {
            return false;
        }

        // https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html
        bool inside = false;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            if ((polygon[i].z > p.z) != (polygon[j].z > p.z) &&
                 p.x < (polygon[j].x - polygon[i].x) * (p.z - polygon[i].z) / (polygon[j].z - polygon[i].z) + polygon[i].x)
            {
                inside = !inside;
            }
        }

        return inside;
    }
    public static Dictionary<Vector3, MapTile> SortTiles(MapTile[] Tiles)
    {
        var MapTiles = new Dictionary<Vector3, MapTile>();
        foreach (MapTile tile in Tiles)
        {
            var v = tile.position;
            MapTiles.Add(v, tile);
        }
        return MapTiles;
    }

}

