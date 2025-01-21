using ENet;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public uint playerId;
    private Vector3 velocity;
    private const float moveSpeed = 5f;

    private void Update()
    {
        if (playerId == Connection.Packet.mylocaluid)
        {
            HandleInput();
        }
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.W)) MovePlayer(Vector2.up);
        if (Input.GetKey(KeyCode.S)) MovePlayer(Vector2.down);
        if (Input.GetKey(KeyCode.A)) MovePlayer(Vector2.left);
        if (Input.GetKey(KeyCode.D)) MovePlayer(Vector2.right);
    }

    private void MovePlayer(Vector2 direction)
    {
        velocity = (Vector3)direction * moveSpeed;
        transform.position += velocity * Time.deltaTime;

        Connection.Packet.SendPlayerMovement(transform.position);
    }
    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
    }
}
