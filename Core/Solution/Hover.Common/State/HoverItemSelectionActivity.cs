﻿using System;
using Hover.Common.Items;
using UnityEngine;
using Hover.Common.Custom;
using System.Collections.Generic;

namespace Hover.Common.State {

	/*================================================================================================*/
	[RequireComponent(typeof(HoverItemData))]
	[RequireComponent(typeof(HoverItemCursorActivity))]
	public class HoverItemSelectionActivity : MonoBehaviour {

		public bool IsNearestHighlight { get; set; }
		public bool IsHighlightPrevented { get; private set; }
		public bool IsSelectionPrevented { get; private set; }
		
		private readonly BaseInteractionSettings vSettings;
		private readonly Dictionary<string, bool> vPreventSelectionMap;
		
		private DateTime? vSelectionStart;
		private float vDistanceUponSelection;


		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public HoverItemSelectionActivity() {
			vSettings = new BaseInteractionSettings(); //TODO: access from somewhere
			vPreventSelectionMap = new Dictionary<string, bool>();
			IsNearestHighlight = true; //TODO: temporary
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public float MaxHighlightProgress {
			get {
				if ( IsHighlightPrevented ) {
					return 0;
				}
				
				BaseItem itemData = GetComponent<HoverItemData>().Data;
				ISelectableItem selData = (itemData as ISelectableItem);

				if ( selData != null && selData.IsStickySelected ) {
					return 1;
				}
				
				HoverItemCursorActivity.Highlight? nearestHigh = 
					GetComponent<HoverItemCursorActivity>().NearestHighlight;
				return (nearestHigh == null ? 0 : nearestHigh.Value.Progress);
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		public float SelectionProgress {
			get {
				if ( vSelectionStart == null ) {
					BaseItem itemData = GetComponent<HoverItemData>().Data;
					ISelectableItem selData = (itemData as ISelectableItem);

					if ( selData == null || !selData.IsStickySelected ) {
						return 0;
					}
					
					HoverItemCursorActivity.Highlight? nearestHigh = 
						GetComponent<HoverItemCursorActivity>().NearestHighlight;
					float minHighDist = (nearestHigh == null ? 
						float.MaxValue : nearestHigh.Value.Distance);

					return Mathf.InverseLerp(vSettings.StickyReleaseDistance/vSettings.ScaleMultiplier,
						vDistanceUponSelection, minHighDist);
				}
				
				float ms = (float)(DateTime.UtcNow-(DateTime)vSelectionStart).TotalMilliseconds;
				return Math.Min(1, ms/vSettings.SelectionMilliseconds);
			}
		}
		
		/*--------------------------------------------------------------------------------------------* /
		public void SetAsNearestItem(CursorType pCursorType, bool pIsNearest) {
			vIsNearestHighlightMap[pCursorType] = pIsNearest;
		}
		
		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void PreventSelectionViaDisplay(string pName, bool pPrevent) {
			bool hasKey = vPreventSelectionMap.ContainsKey(pName);
		
			if ( !pPrevent ) {
				if ( hasKey ) {
					vPreventSelectionMap.Remove(pName);
				}
				
				return;
			}
			
			if ( !hasKey ) {
				vPreventSelectionMap.Add(pName, false);
			}
			
			vPreventSelectionMap[pName] = true;
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public bool IsSelectionPreventedViaDisplay() {
			return (vPreventSelectionMap.Count > 0);
		}
		
		/*--------------------------------------------------------------------------------------------*/
		public bool IsSelectionPreventedViaDisplay(string pName) {
			return (vPreventSelectionMap.ContainsKey(pName) && vPreventSelectionMap[pName]);
		}

		
		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public void Update() {
			UpdateIsHighlightPrevented();
			UpdateSelectionProgress();
		}
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		private void UpdateIsHighlightPrevented() {
			BaseItem itemData = GetComponent<HoverItemData>().Data;
			ISelectableItem selData = (itemData as ISelectableItem);
			
			IsHighlightPrevented = (
				!itemData.IsEnabled ||
				!itemData.IsVisible ||
				!itemData.IsAncestryEnabled ||
				!itemData.IsAncestryVisible ||
				selData == null ||
				IsSelectionPreventedViaDisplay()
			);

			if ( IsHighlightPrevented ) {
				vSelectionStart = null;
				
				if ( selData != null ) {
					selData.DeselectStickySelections();
				}
			}
		}

		/*--------------------------------------------------------------------------------------------*/
		private bool UpdateSelectionProgress() {
			BaseItem itemData = GetComponent<HoverItemData>().Data;
			ISelectableItem selData = (itemData as ISelectableItem);

			if ( selData == null ) {
				return false;
			}

			////

			float selectProg = SelectionProgress;
			bool canSelect = (!IsHighlightPrevented && IsNearestHighlight && selData.AllowSelection);
			
			if ( selectProg <= 0 || !canSelect ) {
				selData.DeselectStickySelections();
			}

			if ( !canSelect ) {
				IsSelectionPrevented = false;
				vSelectionStart = null;
				return false;
			}

			////

			HoverItemCursorActivity.Highlight? nearestHigh = 
				GetComponent<HoverItemCursorActivity>().NearestHighlight;
			
			if ( nearestHigh == null || nearestHigh.Value.Progress < 1 ) {
				IsSelectionPrevented = false;
				vSelectionStart = null;
				return false;
			}

			////

			if ( IsSelectionPrevented ) {
				vSelectionStart = null;
				return false;
			}

			if ( vSelectionStart == null ) {
				vSelectionStart = DateTime.UtcNow;
				return false;
			}

			if ( selectProg < 1 ) {
				return false;
			}

			vSelectionStart = null;
			IsSelectionPrevented = true;
			vDistanceUponSelection = nearestHigh.Value.Distance;
			selData.Select();
			return true;
		}

	}

}
