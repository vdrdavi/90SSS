using System.Collections.Generic;
using UnityEngine;

public enum GuardState { Patrolling, Chasing }

public class GuardPatrol : MonoBehaviour
{
    public float speed = 3.5f;
    public float chaseSpeed = 5.0f;
    public float rotationSpeed = 300f;
    public float viewDistance = 5f;
    public float viewAngle = 45f;
    public LayerMask obstacleLayer;

    public MeshFilter viewMeshFilter;
    public float fovResolution = 1f;

    private BSPMapGenerator mapGenerator;
    private List<Vector3> macroRoute = new List<Vector3>();
    private int currentMacroIndex = 0;
    private Queue<Vector3> microRoute = new Queue<Vector3>();
    private Vector3 currentTarget;
    private bool isRouteReady = false;

    private Transform playerTransform;
    private GuardState currentState = GuardState.Patrolling;
    private Mesh viewMesh;

    private void Start()
    {
        mapGenerator = FindObjectOfType<BSPMapGenerator>();
        if (mapGenerator != null)
        {
            Invoke("InitializePatrol", 0.2f);
        }

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        if (viewMeshFilter != null)
        {
            viewMeshFilter.mesh = viewMesh;

            MeshRenderer meshRenderer = viewMeshFilter.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 50;
            }
        }
    }

    private void InitializePatrol()
    {
        List<Vector3> roomCenters = new List<Vector3>();

        foreach (RoomNode room in mapGenerator.allRooms)
        {
            if (mapGenerator.entranceRoom != null && room == mapGenerator.entranceRoom)
            {
                continue;
            }

            Vector3Int cellPos = new Vector3Int((int)room.Bounds.center.x, (int)room.Bounds.center.y, 0);
            roomCenters.Add(mapGenerator.floorTilemap.GetCellCenterWorld(cellPos));
        }

        if (roomCenters.Count > 0)
        {
            CalculateTSP(roomCenters);
            currentTarget = transform.position;
            isRouteReady = true;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void CalculateTSP(List<Vector3> points)
    {
        macroRoute.Clear();
        Vector3 currentPoint = transform.position;

        while (points.Count > 0)
        {
            int nearestIndex = 0;
            float minDistance = Vector3.Distance(currentPoint, points[0]);

            for (int i = 1; i < points.Count; i++)
            {
                float dist = Vector3.Distance(currentPoint, points[i]);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestIndex = i;
                }
            }

            currentPoint = points[nearestIndex];
            macroRoute.Add(currentPoint);
            points.RemoveAt(nearestIndex);
        }
    }

    private List<Vector3> CalculatePath(Vector3 startWorld, Vector3 targetWorld)
    {
        Vector3Int startPos = mapGenerator.floorTilemap.WorldToCell(startWorld);
        Vector3Int targetPos = mapGenerator.floorTilemap.WorldToCell(targetWorld);

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(startPos);
        cameFrom[startPos] = startPos;

        Vector3Int[] directions = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        bool found = false;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (current == targetPos)
            {
                found = true;
                break;
            }

            foreach (Vector3Int dir in directions)
            {
                Vector3Int next = current + dir;
                if (!cameFrom.ContainsKey(next) && mapGenerator.floorTilemap.HasTile(next))
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        List<Vector3> path = new List<Vector3>();
        if (found)
        {
            Vector3Int current = targetPos;
            while (current != startPos)
            {
                path.Add(mapGenerator.floorTilemap.GetCellCenterWorld(current));
                current = cameFrom[current];
            }
            path.Reverse();
        }
        return path;
    }

    private void Update()
    {
        if (!isRouteReady || macroRoute.Count == 0) return;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        bool canSeePlayer = CheckVision();

        if (canSeePlayer)
        {
            if (currentState == GuardState.Patrolling)
            {
                currentState = GuardState.Chasing;
                microRoute.Clear();
            }
        }
        else
        {
            if (currentState == GuardState.Chasing)
            {
                currentState = GuardState.Patrolling;
                ResumePatrolAtClosestPoint();
            }
        }

        if (currentState == GuardState.Chasing)
        {
            ExecuteChase();
        }
        else
        {
            ExecutePatrol();
        }
    }

    private void LateUpdate()
    {
        if (viewMeshFilter != null)
        {
            DrawFieldOfView();
        }
    }

    private bool CheckVision()
    {
        if (playerTransform == null) return false;

        Vector3 dirToPlayer = playerTransform.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.up, dirToPlayer.normalized);
        if (angle > viewAngle) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distance, obstacleLayer);
        if (hit.collider != null) return false;

        return true;
    }

    private void ExecuteChase()
    {
        Vector3 targetPosition = playerTransform.position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);
    }

    private void ExecutePatrol()
    {
        if (Vector3.Distance(transform.position, currentTarget) < 0.05f)
        {
            if (microRoute.Count > 0)
            {
                currentTarget = microRoute.Dequeue();
            }
            else
            {
                currentMacroIndex = (currentMacroIndex + 1) % macroRoute.Count;
                List<Vector3> newPath = CalculatePath(transform.position, macroRoute[currentMacroIndex]);

                foreach (Vector3 p in newPath)
                {
                    microRoute.Enqueue(p);
                }

                if (microRoute.Count > 0)
                {
                    currentTarget = microRoute.Dequeue();
                }
            }
        }

        Vector3 direction = (currentTarget - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
    }

    private void ResumePatrolAtClosestPoint()
    {
        int closestIndex = 0;
        float minDistance = Vector3.Distance(transform.position, macroRoute[0]);

        for (int i = 1; i < macroRoute.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, macroRoute[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }

        currentMacroIndex = closestIndex;
        microRoute.Clear();

        List<Vector3> newPath = CalculatePath(transform.position, macroRoute[currentMacroIndex]);
        foreach (Vector3 p in newPath)
        {
            microRoute.Enqueue(p);
        }

        if (microRoute.Count > 0)
        {
            currentTarget = microRoute.Dequeue();
        }
        else
        {
            currentTarget = transform.position;
        }
    }

    private void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * 2 * fovResolution);
        float stepAngleSize = (viewAngle * 2) / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - viewAngle + (stepAngleSize * i);
            Vector3 dir = new Vector3(-Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad), 0);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewDistance, obstacleLayer);

            if (hit.collider != null)
            {
                viewPoints.Add(hit.point);
            }
            else
            {
                viewPoints.Add(transform.position + dir * viewDistance);
            }
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
        viewMesh.RecalculateBounds();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.TriggerGameOver();
            }
        }
    }
}