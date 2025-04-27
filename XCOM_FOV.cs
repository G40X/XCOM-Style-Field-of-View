using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XCOM_FOV : MonoBehaviour
{
    public float radius;
    [Range(0, 360)] public float angle;

    public List<GameObject> targetsInRange = new();

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;
    
    [SerializeField] private Transform los;
    private Collider losCollider;
    private Vector3[] boxEdges;
    private void Awake()
    {
        // Line of Sight (LoS) collider should be about half of the cell size of 1 tile, since the edges will be used.
        // LoS Collider should be seprate from the character's physical world interaction collider because it will be a trigger collider
        los = transform.GetChild(0);
        losCollider = los.GetComponent<BoxCollider>();
    }
    private void Start()
    {
        Bounds worldBounds = losCollider.bounds;
        boxEdges = new Vector3[8];
        float edgeY = los.transform.position.y;
        boxEdges[0] = new Vector3(losCollider.transform.position.x - worldBounds.extents.x, edgeY, losCollider.transform.position.z - worldBounds.extents.z);
        boxEdges[1] = new Vector3(losCollider.transform.position.x + worldBounds.extents.x, edgeY, losCollider.transform.position.z - worldBounds.extents.z);
        boxEdges[2] = new Vector3(losCollider.transform.position.x + worldBounds.extents.x, edgeY, losCollider.transform.position.z + worldBounds.extents.z);
        boxEdges[3] = new Vector3(losCollider.transform.position.x - worldBounds.extents.x, edgeY, losCollider.transform.position.z + worldBounds.extents.z);
    }

    private void Update()
    {
        FieldOfViewCheck();
    }

    public void FieldOfViewCheck()
    {
        targetsInRange.Clear();

        // Gets ALL Line of Sight colliders, including enemies and allied characters
        Collider[] rangeCheck = Physics.OverlapSphere(los.position, radius, targetMask);
        if (rangeCheck.Length > 0)
        {
            Vector3 directionToTarget = Vector3.zero;
            float distanceToTarget = 0f;
            foreach (Collider unitCollider in rangeCheck)
            {
                // Filter out this character and it's allies from check
                if (!los.CompareTag(unitCollider.tag))
                {
                    XCOM_FOV enemyFOV = unitCollider.transform.parent.gameObject.GetComponent<XCOM_FOV>();
                    if (enemyFOV != null)
                    {
                        // First, check if the center to target ray
                        directionToTarget = (unitCollider.transform.position - los.position).normalized;
                        if (Vector3.Angle(los.forward, directionToTarget) < angle / 2)
                        {
                            distanceToTarget = Vector3.Distance(los.position, unitCollider.transform.position);
                            if (!Physics.Raycast(los.position, directionToTarget, distanceToTarget, obstacleMask) && !targetsInRange.Contains(unitCollider.gameObject))
                            {
                                GameObject targetObject = unitCollider.transform.parent.gameObject;
                                targetsInRange.Add(targetObject);
                                continue; // Skip edge checks for this collider since we've already found it
                            }
                            else
                            {
                                // Next, if center ray was blocked or out of angle, try edge to edge checks
                                bool targetFound = false;
                                // loop through this character's line of sight locations
                                for (int i = 0; i < this.boxEdges.Length && !targetFound; i++)
                                {
                                    // for each line of sight location check all enemy line of sight locations.
                                    // In this case, 8 checks for each of this LoS location
                                    for (int e = 0; e < enemyFOV.boxEdges.Length && !targetFound; e++)
                                    {
                                        // Check if edge to edge is in line of sight angle
                                        directionToTarget = (enemyFOV.boxEdges[e] - this.boxEdges[i]).normalized;
                                        if (Vector3.Angle(los.forward, directionToTarget) < angle / 2)
                                        {
                                            distanceToTarget = Vector3.Distance(this.boxEdges[i], enemyFOV.boxEdges[e]);
                                            if (!Physics.Raycast(this.boxEdges[i], directionToTarget, distanceToTarget, obstacleMask) && !targetsInRange.Contains(unitCollider.gameObject))
                                            {
                                                GameObject targetObject = unitCollider.transform.parent.gameObject;
                                                targetsInRange.Add(targetObject);
                                                targetFound = true; // Exit both "boxEdge" loops once an enemy is found and move on to the next enemy
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // After All Check loops
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < boxEdges.Length; i++)
            {
                Gizmos.DrawCube(boxEdges[i], new Vector3(0.05f, 0.05f, 0.05f));
            }
        }
    }
}
