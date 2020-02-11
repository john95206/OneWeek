using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SpaceGame
{
    [RequireComponent(typeof(LineRenderer))]
    public class GestureRenderer : MonoBehaviour
    {
        [SerializeField]
        private Color beginColor = Color.cyan;

        [SerializeField]
        private Color endColor = Color.magenta;

        [SerializeField]
        private float lineWidth = 0.2f;

        private List<Vector3> verts = new List<Vector3>();

        private LineRenderer lr;

		EnemyDestroyer destroyer;

        void Start()
        {
            lr = GetComponent<LineRenderer>();
            lr.SetColors(beginColor, endColor);
            lr.SetWidth(lineWidth, lineWidth);
            lr.SetVertexCount(0);
            lr.sortingLayerName = "ForeGround";
            lr.sortingOrder = 2000;

			destroyer = GetComponent<EnemyDestroyer>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                verts.Clear();
            }

            if (Input.GetMouseButton(0))
            {
                // add vertices
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);
                if (verts.Count == 0 || pos != verts[verts.Count - 1])
                {
                    verts.Add(pos);
                }

            }

			if (Input.GetMouseButtonUp(0))
			{
				UpdatePolygonCollider();
			}

            // set LineRenderer
            if (verts.Count > 1)
            {
                lr.SetVertexCount(verts.Count);
                lr.SetPositions(verts.ToArray());
            }
        }

		void UpdatePolygonCollider()
		{
			var collider = GetComponent<PolygonCollider2D>();
			if (collider == null)
			{
				collider = this.gameObject.AddComponent<PolygonCollider2D>();
			}
			collider.isTrigger = true;
			var points = verts.ConvertAll(Vector3ToVector2).ToArray();
			collider.points = points;

			destroyer.BeginDetectionByCollider();
		}

		Vector2 Vector3ToVector2(Vector3 vector3)
		{
			return new Vector2(vector3.x, vector3.y);
		}
	}
}