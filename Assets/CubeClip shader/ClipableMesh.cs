using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeClipShader
{
	[ExecuteInEditMode]
	public class ClipableMesh : MonoBehaviour
	{
		[SerializeField]
		List<CubeClip> ClippingCubes = new List<CubeClip>();
		Material Material;

		const string MatrixName = "_Fadable_matrix_";
		const string PositionName = "_Fadable_pivot_";
		const string OpacityName = "_Fadable_opacity_";

		const string KeywordName = "_FADABLE_STATUS__";

		const int MaxClippersCount = 8;

		private void OnEnable()
		{//#colreg(darkorange*.5);
			var renderer = GetComponent<Renderer>();
#if UNITY_EDITOR
			if (renderer == null)
				Debug.LogError("GameObject with a '" + nameof(ClipableMesh) + "' component does not have a Renderer on it!", this.gameObject);
#endif
			Material = renderer.sharedMaterial;

			for (int i = 0; i < MaxClippersCount; ++i)
				SetOpacity(i, 1);

			SynchronizeClippingCubeLists(null);
		}//#endcolreg
		
		public void RemoveClipper(CubeClip cubeToRemove)
		{//#colreg(darkred);
			if (ClippingCubes.Contains(cubeToRemove))
			{
				ClippingCubes.Remove(cubeToRemove);
				SynchronizeClippingCubeLists(null);

				if (ClippingCubes.Count == 0)
				{
					for (int i = 0; i < MaxClippersCount; ++i)
						SetOpacity(i, 1);
				}
			}
		}//#endcolreg

		public void SynchronizeClippingCubeLists(CubeClip newCube)
		{//#colreg(darkred);
			if (newCube == null || (newCube != null && !ClippingCubes.Contains(newCube)))
			{
				if (newCube != null)
					ClippingCubes.Add(newCube);

				ClippingCubes = ClippingCubes.Distinct().ToList();

				int n = ClippingCubes.Count;
				while (--n > -1)
				{
					if (ClippingCubes[n] == null)
						ClippingCubes.RemoveAt(n);
					else
					{
						ClippingCubes[n].CheckAndAddClipableMesh(this);
						ClippingCubes[n].transform.hasChanged = true;
					}
				}
			}
		}//#endcolreg

		private void OnValidate()
		{
			SynchronizeClippingCubeLists(null);

			if (ClippingCubes.Count == 0)
			{
				for (int i = 0; i < MaxClippersCount; ++i)
					SetOpacity(i, 1);
			}
		}

		void SetOpacity(int i, float opacity)
		{
			Material.SetFloat(OpacityName + i, opacity);
		}

		public void SetOpacity(CubeClip cube, float opacity)
		{//#colreg(darkred);
			if (Material != null)
			{
				int cubeNumber = 0;
				foreach (var clipper in ClippingCubes)
				{
					if (!clipper.enabled)
						opacity = 1;

					SetOpacity(cubeNumber, opacity);
					cubeNumber++;
				}
			}
		}//#endcolreg


#if UNITY_EDITOR
		Vector3 LastSetPosition = -Vector3.one * float.MaxValue;
#endif
		void SetShaderParameters(Transform cubeTransform, int i, float opacity)
		{
			Quaternion oldRotation = cubeTransform.rotation;
			cubeTransform.rotation = Quaternion.Inverse(cubeTransform.rotation);
			var matrix = cubeTransform.localToWorldMatrix.inverse;
			cubeTransform.rotation = oldRotation;

			Material.SetMatrix(MatrixName + i, matrix);
			Material.SetVector(PositionName + i, cubeTransform.position);
			SetOpacity(i, opacity);
		}

		void Update()
		{//#colreg(darkblue);
			bool anyClipperChanged = false;

			int n = ClippingCubes.Count;
			while (--n > -1)
			{
				if (ClippingCubes[n] == null)
				{
					ClippingCubes.RemoveAt(n);
					anyClipperChanged = true;
				}
			}

			n = ClippingCubes.Count;
			while (--n > -1)
			{
				var clipper = ClippingCubes[n];
				if (clipper.transform.hasChanged)
				{
					anyClipperChanged = true;

					float opacity = clipper.Opacity;
					if (!clipper.enabled)
						opacity = 1;
					SetShaderParameters(clipper.transform, n, opacity);
					clipper.transform.hasChanged = false;
				}
			}

			if (anyClipperChanged)
			{
				for (int i = 0; i < MaxClippersCount; i++)
					Material.DisableKeyword(KeywordName + i);

				if (ClippingCubes.Count > 0)
					Material.EnableKeyword(KeywordName + (ClippingCubes.Count - 1));

				for (int i = ClippingCubes.Count; i < MaxClippersCount; ++i)
					SetOpacity(i, 1);
			}

#if UNITY_EDITOR
			if (ClippingCubes.Count > MaxClippersCount)
				Debug.LogError("A renderer with cube clipping has too many clippers (max = " + MaxClippersCount + ")!", this.gameObject);
#endif
		}//#endcolreg
	}
}