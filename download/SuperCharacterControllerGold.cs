// SuperCharacterControllerGold.cs
// Author: Erik Roystan Ross
// Source: http://roystanross.wordpress.com/
// Thanks to: fholm, for some of the libraries/original algorithms

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class SuperCharacterControllerGold : MonoBehaviour
{
    public Vector3 debugMove = Vector3.zero;
    public bool debugPushbackMesssages;

    [Serializable]
    public class CollisionSphere
    {
        public float Offset;
        public bool IsFeet;
        public bool IsHead;

        public CollisionSphere()
        {

        }

        public CollisionSphere(float offset, bool isFeet, bool isHead)
        {
            Offset = offset;
            IsFeet = isFeet;
        }
    }

    [SerializeField]
    public class BasicGround
    {
        public RaycastHit Hit;
        public RaycastHit BackHit;
        public RaycastHit FrontHit;

        public BasicGround(RaycastHit hit, RaycastHit backHit, RaycastHit frontHit)
        {
            Hit = hit;
            BackHit = backHit;
            FrontHit = frontHit;
        }
    }

    [SerializeField]
    CollisionSphere[] spheres =
        new CollisionSphere[3] {
            new CollisionSphere(0.5f, true, false),
            new CollisionSphere(1.0f, false, false),
            new CollisionSphere(1.5f, false, true),
        };

    [SerializeField]
    LayerMask walkable;

    [SerializeField]
    protected float radius = 0.5f;

    [SerializeField]
    float slopeLimit = 60.0f;

    public BasicGround currentBasicGround { get; private set; }
    public CollisionSphere feet { get; private set; }
    public CollisionSphere head { get; private set; }

    private Vector3 initialPosition;
    private bool clamping = true;
    private bool slopeLimiting = true;

    private const float Tolerance = 0.05f;
    private const float TinyTolerance = 0.02f;
    private const string TemporaryLayer = "TempCast";
    private int TemporaryLayerIndex;

    public void Start()
    {
        TemporaryLayerIndex = LayerMask.NameToLayer(TemporaryLayer);

        foreach (var sphere in spheres)
        {
            if (sphere.IsFeet)
                feet = sphere;

            if (sphere.IsHead)
                head = sphere;
        }

        if (feet == null)
            Debug.LogError("[SuperCharacterController] Feet not found on controller");

        if (head == null)
            Debug.LogError("[SuperCharacterController] Head not found on controller");
    }

    void Update()
    {
        // Move Phase

        initialPosition = transform.position;

        ProbeGround();

        transform.position += debugMove * Time.deltaTime;

        // Pushback Phase

        Pushback();

        // Resolution Phase

        ProbeGround();

        if (slopeLimiting)
            SlopeLimit();

        ProbeGround();

        if (clamping)
            ClampToGround();
    }

    bool SlopeLimit()
    {
        Vector3 n = currentBasicGround.Hit.normal;
        float a = Vector3.Angle(n, Vector3.up);

        if (a > slopeLimit)
        {
            Vector3 absoluteMoveDirection = Math3D.ProjectVectorOnPlane(n, transform.position - initialPosition);

            // Retrieve a vector pointing down the slope
            Vector3 r = Vector3.Cross(n, Vector3.down);
            Vector3 v = Vector3.Cross(r, n);

            float angle = Vector3.Angle(absoluteMoveDirection, v);

            if (angle <= 90.0f)
                return false;

            // Calculate where to place the controller on the slope, or at the bottom, based on the desired movement distance
            Vector3 resolvedPosition = Math3D.ProjectPointOnLine(initialPosition, r, transform.position);
            Vector3 direction = Math3D.ProjectVectorOnPlane(n, resolvedPosition - transform.position);

            RaycastHit hit;

            // Check if our path to our resolved position is blocked by any colliders
            if (Physics.CapsuleCast(OffsetPosition(feet.Offset), OffsetPosition(head.Offset), radius, direction.normalized, out hit, direction.magnitude, walkable))
            {
                transform.position += v.normalized * hit.distance;
            }
            else
            {
                transform.position += direction;
            }

            return true;
        }

        return false;
    }

    void ClampToGround()
    {
        float d = currentBasicGround.Hit.distance;
        transform.position -= transform.up * d;
    }

    protected void EnableClamping()
    {
        clamping = true;
    }

    protected void DisableClamping()
    {
        clamping = false;
    }

    protected void EnableSlopeLimit()
    {
        slopeLimiting = true;
    }

    protected void DisabledSlopeLimit()
    {
        slopeLimiting = false;
    }

    void ProbeGround()
    {
        Vector3 o = OffsetPosition(feet.Offset) + (transform.up * Tolerance);

        RaycastHit hit;

        if (Physics.SphereCast(o, radius, -transform.up, out hit, walkable))
        {
            // Remove the tolerance from the distance travelled
            hit.distance -= Tolerance;

            // Cast downwards slightly behind and slightly in front of our hit.point
            // This avoids the problem of SphereCasts interpolating hit.normals when it collides
            // With the edge of a surface

            Vector3 backDirection = Math3D.ProjectVectorOnPlane(hit.normal, hit.point - transform.position).normalized * Tolerance;
            backDirection += transform.up * Tolerance;
            Vector3 backPoint = hit.point + backDirection;

            Vector3 frontDirection = Math3D.ProjectVectorOnPlane(transform.up, hit.point - transform.position).normalized * Tolerance;
            Vector3 frontPoint = hit.point - frontDirection;

            RaycastHit backHit;
            RaycastHit frontHit;

            Physics.Raycast(backPoint, -transform.up, out backHit, walkable);
            Physics.Raycast(frontPoint, -transform.up, out frontHit, walkable);

            currentBasicGround = new BasicGround(hit, backHit, frontHit);
        }
        else
        {
            // Error: There should always be some sort of ground below us
        }
    }

    void Pushback()
    {
        foreach (var sphere in spheres)
        {
            foreach (Collider col in Physics.OverlapSphere(OffsetPosition(sphere.Offset), radius, walkable))
            {
                Vector3 position = OffsetPosition(sphere.Offset);
                Vector3 contactPoint = Vector3.zero;

                if (col is BoxCollider)
                {
                    contactPoint = SuperCollisions.ClosestPointOnSurface((BoxCollider)col, position);
                }
                else if (col is SphereCollider)
                {
                    contactPoint = SuperCollisions.ClosestPointOnSurface((SphereCollider)col, position);
                }
                else if (col is MeshCollider)
                {
                    RPGMesh rpgMesh = col.GetComponent<RPGMesh>();

                    if (rpgMesh != null)
                    {
                        contactPoint = rpgMesh.ClosestPointOn(position, radius, false, false);
                    }
                }

                if (contactPoint != Vector3.zero)
                {
                    if (debugPushbackMesssages)
                        DebugDraw.DrawMarker(contactPoint, 2.0f, Color.cyan, 0.0f, false);

                    Vector3 v = contactPoint - position;

                    if (v != Vector3.zero)
                    {
                        // Cache the collider's layer so that we can cast against it
                        int layer = col.gameObject.layer;

                        col.gameObject.layer = TemporaryLayerIndex;

                        // Check which side of the normal we are on
                        bool facingNormal = Physics.SphereCast(new Ray(position, v.normalized), TinyTolerance, v.magnitude + TinyTolerance, 1 << TemporaryLayerIndex);

                        col.gameObject.layer = layer;

                        // Orient and scale our vector based on which side of the normal we are situated
                        if (facingNormal)
                        {
                            if (Vector3.Distance(position, contactPoint) < radius)
                            {
                                v = v.normalized * (radius - v.magnitude) * -1;
                            }
                            else
                            {
                                // A previously resolved collision has had a side effect that moved us outside this collider
                                continue;
                            }
                        }
                        else
                        {
                            v = v.normalized * (radius + v.magnitude);
                        }

                        transform.position += v;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (spheres != null)
        {
            foreach (var sphere in spheres)
            {
                Gizmos.color = sphere.IsFeet ? Color.green : (sphere.IsHead ? Color.yellow : Color.cyan);
                Gizmos.DrawWireSphere(OffsetPosition(sphere.Offset), radius);
            }
        }
    }

    Vector3 OffsetPosition(float y)
    {
        Vector3 p;

        p = transform.position;

        p.y += y;

        return p;
    }
}
