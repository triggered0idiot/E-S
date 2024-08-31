using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DoomahLevelLoader
{
    public class ShowTriggers : ICheat
    {
        public string LongName => "Show Triggers";
        public string Identifier => "envyandspite.showtriggeres";
        public string ButtonEnabledOverride => "Enabled";
        public string ButtonDisabledOverride => "Disabled";
        public string Icon => "debug";
        public bool IsActive { get; private set; } = false;
        public bool DefaultState => false;
        public StatePersistenceMode PersistenceMode => (StatePersistenceMode)1;

        public ShowTriggers()
        {
        }

        public void Disable()
        {
            TriggerRenderer[] trs = GameObject.FindObjectsOfType<TriggerRenderer>();
            foreach (TriggerRenderer tr in trs)
            {
                GameObject.Destroy(tr);
            }
        }

        public void Enable()
        {
            BoxCollider[] colliders = GameObject.FindObjectsOfType<BoxCollider>();
            foreach (BoxCollider collider in colliders) {
                collider.gameObject.AddComponent<TriggerRenderer>();
            }
        }

        public void Update() { }
    }

    public class TriggerRenderer : MonoBehaviour // this totally isnt from my trigger viewer wdym
    {
        [Header("DO NOT TOUCH THIS!")]
        public BoxCollider target;
        public Material trig;
        public Material clip;
        public Material addressable;
        Mesh cube;
        Vector2[] defaultUVS;
        public void Cleanup()
        {
            DestroyImmediate(gameObject);
            DestroyImmediate(cube);
        }
        public void Awake() //if this gets called somehow
        {
            Cleanup();
        }

        bool isaddressable = false;
        public void Start()
        {
            foreach (Component c in target.GetComponents(typeof(Component)))
            {
                isaddressable |= c.GetComponent("AddressableReplacer") != null;
            }
            isaddressable |= this.GetComponent("AddressableReplacer") != null;
        }

        private void OnPostRender()
        {
            if (target && trig && clip && addressable) DrawTexturedCube(target, isaddressable ? addressable : (target.isTrigger ? trig : clip));

            if (!target) Cleanup();
        }

        private void DrawTexturedCube(BoxCollider collider, Material mat)
        {
            // Apply the material
            if (!mat.SetPass(0)) return;

            // Get the collider's transform matrix
            Vector3 newScale = collider.transform.localScale;
            newScale.Scale(target.size);
            Matrix4x4 matrix = Matrix4x4.TRS(collider.transform.TransformPoint(collider.center), collider.transform.rotation, newScale);

            if (!cube) { cube = MeshCube(); defaultUVS = cube.uv; }
            cube.uv = AdjustUV(cube, newScale);

            Graphics.DrawMeshNow(cube, matrix);
        }

        static Mesh CopyMesh(Mesh mesh) // thanks unity (stfu)
        {
            Mesh newmesh = new Mesh();
            newmesh.vertices = mesh.vertices;
            newmesh.triangles = mesh.triangles;
            newmesh.uv = mesh.uv;
            newmesh.normals = mesh.normals;
            newmesh.colors = mesh.colors;
            newmesh.tangents = mesh.tangents;
            return newmesh;
        }

        private Mesh MeshCube()
        {
            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh m = new Mesh();
            try { m = CopyMesh(tmp.GetComponent<MeshFilter>().sharedMesh); }
            catch { }
            DestroyImmediate(tmp);
            return m;
        }

        #region UV


        public enum MappingAxis
        {
            X_Axis,
            Y_Axis,
            Z_Axis
        }

        public static MappingAxis GetDominantAxis(Vector3 normal)
        {
            // Take the absolute value of the normal vector components
            float absX = Mathf.Abs(normal.x);
            float absY = Mathf.Abs(normal.y);
            float absZ = Mathf.Abs(normal.z);

            // Determine the dominant axis
            if (absX > absY && absX > absZ)
            {
                return MappingAxis.X_Axis;
            }
            else if (absY > absX && absY > absZ)
            {
                return MappingAxis.Y_Axis;
            }
            else
            {
                return MappingAxis.Z_Axis;
            }
        }

        Vector2[] AdjustUV(Mesh mesh, Vector3 scale)
        {
            scale *= 0.25f;
            Vector2[] uvs = mesh.uv;

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(vertices[i]);
                MappingAxis dominantAxis = GetDominantAxis(normals[i]);

                switch (dominantAxis)
                {
                    case MappingAxis.X_Axis:
                        uvs[i] = new Vector2(worldPos.y, worldPos.z) * scale;
                        break;
                    case MappingAxis.Y_Axis:
                        uvs[i] = new Vector2(worldPos.x, worldPos.z) * scale;
                        break;
                    case MappingAxis.Z_Axis:
                        uvs[i] = new Vector2(worldPos.x, worldPos.y) * scale;
                        break;
                }
            }

            return uvs;
        }

        #endregion
    }
}
