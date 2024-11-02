﻿using FMODUnity;
using Sirenix.OdinInspector;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Quinn.DungeonGeneration
{
	public class Room : MonoBehaviour
	{
		[SerializeField]
		private EventReference OpenSound, LockSound;
		[SerializeField, Required]
		private Trigger RoomTrigger;
		[SerializeField, Required]
		private CinemachineCamera RoomCamera;

		public bool IsLocked { get; private set; }
		public bool IsConquered { get; private set; } = true; // TODO: Implement.

		public Vector2Int RoomGridIndex { get; set; }

		private Door[] _doors;

		private void Awake()
		{
			_doors = GetComponentsInChildren<Door>();

			RoomTrigger.OnTriggerEnter += OnPlayerTriggerEnter;
			RoomTrigger.OnTriggerExit += OnPlayerTriggerExit;
		}

		public void Open()
		{
			if (!IsLocked)
				return;

			IsLocked = false;
			Audio.Play(OpenSound, transform.position);

			foreach (var door in _doors)
			{
				door.Open();
			}
		}

		public void Lock()
		{
			if (IsLocked)
				return;

			IsLocked = true;
			Audio.Play(LockSound, transform.position);

			foreach (var door in _doors)
			{
				door.Close();
			}
		}

		private void OnPlayerTriggerEnter(Collider2D collider)
		{
			if (collider.CompareTag("Player"))
			{
				RoomCamera.enabled = true;
				RoomCamera.Target.TrackingTarget = collider.transform;

				if (!IsConquered)
				{
					Lock();
				}
			}
		}

		private void OnPlayerTriggerExit(Collider2D collider)
		{
			if (collider.CompareTag("Player"))
			{
				RoomCamera.enabled = false;
				GeberateRoomAtPlayer(collider);
			}
		}

		private void GeberateRoomAtPlayer(Collider2D collider)
		{
			Vector2 playerExitDir = transform.position.DirectionTo(collider.transform.position);
			if (playerExitDir.IsVertical())
			{
				if (playerExitDir.y > 0f)
					DungeonGenerator.Instance.GenerateRoomAt(RoomGridIndex.x, RoomGridIndex.y + 1);
				else
					DungeonGenerator.Instance.GenerateRoomAt(RoomGridIndex.x, RoomGridIndex.y - 1);
			}
			else
			{
				if (playerExitDir.x > 0f)
					DungeonGenerator.Instance.GenerateRoomAt(RoomGridIndex.x + 1, RoomGridIndex.y);
				else
					DungeonGenerator.Instance.GenerateRoomAt(RoomGridIndex.x - 1, RoomGridIndex.y);
			}
		}
	}
}
