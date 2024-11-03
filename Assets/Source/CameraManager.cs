﻿using DG.Tweening;
using Quinn.PlayerSystem;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace Quinn
{
	public class CameraManager : MonoBehaviour
	{
		public static CameraManager Instance { get; private set; }

		[SerializeField]
		private float FadeToBlackDuration = 0.3f;
		[SerializeField]
		private float FadeFromBlackDuration = 0.2f;
		[SerializeField]
		private Ease FadeToBlackEase = Ease.InCubic;
		[SerializeField]
		private Ease FadeFromBlackEase = Ease.OutCubic;

		[Space, SerializeField]
		private float PlayerToCursorBias = 0.2f;
		[SerializeField]
		private float CameraTargetSmoothTime = 0.2f;

		public Transform CameraTarget => _handle.CameraTarget;

		private CameraHandle _handle;
		private CinemachineCamera _followCam;
		private Image _blackout;

		private Vector2 _camTargetVel;

		private void Awake()
		{
			Debug.Assert(Instance == null);
			Instance = this;
		}

		private async void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				float time = Time.time;
				await TransitionAsync(() => Time.time > time + 1f);
			}

			Vector2 playerPos = PlayerManager.Instance.Player.transform.position;
			Vector2 cursorPos = InputManager.Instance.CursorWorldPos;

			Vector2 cameraTarget = Vector2.Lerp(playerPos, cursorPos, PlayerToCursorBias);
			CameraTarget.position = Vector2.SmoothDamp(CameraTarget.position, cameraTarget, ref _camTargetVel, CameraTargetSmoothTime);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		public void SetCameraHandle(CameraHandle handle)
		{
			_handle = handle;

			_followCam = _handle.VirtualCamera;
			_blackout = _handle.Blackout;
		}

		public void EnableFollowCamera()
		{
			Debug.Assert(_followCam != null, "Cannot enable follow camera because it is null");
			_followCam.enabled = true;
		}

		public void DisableFollowCamera()
		{
			Debug.Assert(_followCam != null, "Cannot disable follow camera because it is null");
			_followCam.enabled = false;
		}

		public async Awaitable TransitionAsync(System.Func<bool> canFadeIn)
		{
			_blackout.enabled = true;
			var color = _blackout.color;
			color.a = 0f;
			_blackout.color = color;

			await _blackout.DOFade(1f, FadeToBlackDuration)
				.SetEase(FadeToBlackEase)
				.AsyncWaitForCompletion();

			while(!canFadeIn())
			{
				await System.Threading.Tasks.Task.Yield();
			}

			_blackout.DOFade(0f, FadeFromBlackDuration)
				.SetEase(FadeFromBlackEase)
				.onComplete += () =>
				{
					_blackout.enabled = false;
				};
		}
		public async Awaitable TransitionAsync()
		{
			await TransitionAsync(() => true);
		}
	}
}
