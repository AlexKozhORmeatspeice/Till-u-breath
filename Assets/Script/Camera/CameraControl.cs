using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private float zoom = 1f; 
    private Transform swivel;

    [SerializeField] private float minOrtSize;
    [SerializeField] private float maxOrtSize;

    [SerializeField] private float rotateSpeed = 30f;
    private float rotAngle;
    [SerializeField] private float moveSpeedMinZoom = 400f;
    [SerializeField] private float moveSpeedMaxZoom = 100f;
    
    [SerializeField] private Camera camera;
    
    [SerializeField] private HexGrid grid;
    [SerializeField] private Vector2 mapBorders;
    
    private void Awake()
    {
        rotAngle = 0f;
        zoom = 1f;
        swivel = transform.GetChild(0);
    }

    private void Update()
    {
        //zoom
        float zoomDelta = -Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
            AdjustZoom(zoomDelta);

        //rotate
        float rotDelta = Input.GetAxis("Rotation");
        if(rotDelta != 0f)
            AdjustRotation(rotDelta);
        
        //move
        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);
        
        float ortSize = Mathf.Lerp(minOrtSize, maxOrtSize, zoom);
        camera.orthographicSize = ortSize;
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 dir = swivel.localRotation * new Vector3(xDelta, 0, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float dist = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * Time.deltaTime;

        Vector3 pos = transform.localPosition;
        pos += dir * (damping * dist);
        transform.localPosition = ClampPosition(pos);
    }

    private Vector3 ClampPosition(Vector3 pos)
    {
        float xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - mapBorders.x)* (2f * HexMetrics.innerRadius) / 2f;
        pos.x = Mathf.Clamp(pos.x, -xMax, xMax);
        
        float zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - mapBorders.y) * (1.5f * HexMetrics.outerRadius) / 2f;
        pos.z = Mathf.Clamp(pos.z, -zMax, zMax);

        return pos;
    }

    private void AdjustRotation(float rotDelta)
    {
        rotAngle += rotDelta * rotateSpeed * Time.deltaTime;
        
        if (rotAngle < 0f)
        {
            rotAngle += 360f;
        }
        else if (rotAngle >= 360f)
        {
            rotAngle -= 360f;
        }
        
        swivel.rotation = Quaternion.Euler(0.0f,rotAngle, 0.0f);
    }
}
