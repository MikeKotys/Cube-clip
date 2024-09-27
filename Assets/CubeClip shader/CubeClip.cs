using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeClipShader
{
	[ExecuteInEditMode]
	public class CubeClip : MonoBehaviour
	{
		[SerializeField]
		List<ClipableMesh> ClipableMeshes = new List<ClipableMesh>();
		List<ClipableMesh> OldClipableMeshes = new List<ClipableMesh>();
		public float Opacity;

		private void OnEnable()
		{//#colreg(darkorange*.5);
			transform.hasChanged = true;
		}//#endcolreg

		public void CheckAndAddClipableMesh(ClipableMesh mesh)
		{//#colreg(darkred);
			if (!ClipableMeshes.Contains(mesh))
				ClipableMeshes.Add(mesh);
		}//#endcolreg

		private void OnValidate()
		{
			ClipableMeshes = ClipableMeshes.Distinct().ToList();
			int n = OldClipableMeshes.Count;
			while (--n > -1)
			{
				if (!ClipableMeshes.Contains(OldClipableMeshes[n]))
					OldClipableMeshes[n].RemoveClipper(this);
			}
			OldClipableMeshes.Clear();

			n = ClipableMeshes.Count;
			while (--n > -1)
			{
				if (ClipableMeshes[n] == null)
					ClipableMeshes.RemoveAt(n);
				else
				{
					ClipableMeshes[n].SynchronizeClippingCubeLists(this);
					OldClipableMeshes.Add(ClipableMeshes[n]);
				}
			}

			SetOpacity(Opacity);
		}

		private void OnDisable()
		{
			transform.hasChanged = true;
		}

		private void OnDestroy()
		{
			foreach (var clipableMesh in ClipableMeshes)
				clipableMesh.RemoveClipper(this);
		}

		public void SetOpacity(float opacity)
		{//#colreg(darkred);
			Opacity = opacity;
			foreach (var clipableMesh in ClipableMeshes)
				clipableMesh.SetOpacity(this, Opacity);
		}//#endcolreg
	}
}