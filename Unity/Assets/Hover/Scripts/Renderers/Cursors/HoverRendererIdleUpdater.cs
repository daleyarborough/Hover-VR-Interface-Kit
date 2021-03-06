using Hover.Cursors;
using Hover.Renderers.Utils;
using Hover.Utils;
using UnityEngine;

namespace Hover.Renderers.Cursors {

	/*================================================================================================*/
	[ExecuteInEditMode]
	[RequireComponent(typeof(TreeUpdater))]
	[RequireComponent(typeof(HoverCursorFollower))]
	public class HoverRendererIdleUpdater : MonoBehaviour, ITreeUpdateable, ISettingsController {

		public GameObject IdleRendererPrefab;
		public HoverRendererIdle IdleRenderer;
		public bool ClickToRebuildRenderer = false;

		private GameObject vPrevIdlePrefab;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Awake() {
			vPrevIdlePrefab = IdleRendererPrefab;
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual void Start() {
			//do nothing...
		}

		/*--------------------------------------------------------------------------------------------*/
		public virtual void TreeUpdate() {
			DestroyRendererIfNecessary();
			IdleRenderer = (IdleRenderer ?? FindOrBuildIdle());

			ICursorData cursorData = GetComponent<HoverCursorFollower>().GetCursorData();

			UpdateRenderer(cursorData);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void DestroyRendererIfNecessary() {
			if ( ClickToRebuildRenderer || IdleRendererPrefab != vPrevIdlePrefab ) {
				vPrevIdlePrefab = IdleRendererPrefab;
				RendererUtil.DestroyRenderer(IdleRenderer);
				IdleRenderer = null;
			}

			ClickToRebuildRenderer = false;
		}

		/*--------------------------------------------------------------------------------------------*/
		private HoverRendererIdle FindOrBuildIdle() {
			return RendererUtil.FindOrBuildRenderer<HoverRendererIdle>(gameObject.transform, 
				IdleRendererPrefab, "Idle", typeof(HoverRendererIdle));
		}


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateRenderer(ICursorData pCursorData) {
			IdleRenderer.Controllers.Set(HoverRendererIdle.CenterPositionName, this);
			IdleRenderer.Controllers.Set(HoverRendererIdle.DistanceThresholdName, this);
			IdleRenderer.Controllers.Set(HoverRendererIdle.TimerProgressName, this);
			IdleRenderer.Controllers.Set(HoverRendererIdle.IsRaycastName, this);

			IdleRenderer.CenterOffset =
				transform.InverseTransformPoint(pCursorData.Idle.WorldPosition);
			IdleRenderer.DistanceThreshold = pCursorData.Idle.DistanceThreshold;
			IdleRenderer.TimerProgress = pCursorData.Idle.Progress;
			IdleRenderer.IsRaycast = pCursorData.IsRaycast;

			/*Transform itemPointHold = IdleRenderer.Fill.ItemPointer.transform.parent;
			Transform cursPointHold = IdleRenderer.Fill.CursorPointer.transform.parent;
			
			Vector3 itemCenter = GetComponent<HoverRendererUpdater>()
				.ActiveRenderer.GetCenterWorldPosition();
			Vector3 itemCenterLocalPos = IdleRenderer.transform
				.InverseTransformPoint(itemCenter);
			Vector3 cursorLocalPos = IdleRenderer.transform
				.InverseTransformPoint(pCursorData.WorldPosition);

			itemPointHold.localRotation = Quaternion.FromToRotation(Vector3.right, itemCenterLocalPos);
			cursPointHold.localRotation = Quaternion.FromToRotation(Vector3.right, cursorLocalPos);*/
		}

	}

}
