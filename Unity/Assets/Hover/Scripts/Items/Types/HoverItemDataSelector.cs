﻿using System;

namespace Hover.Items.Types {

	/*================================================================================================*/
	[Serializable]
	public class HoverItemDataSelector : HoverItemDataSelectable, IItemDataSelector {
		
		public SelectorActionType _Action = SelectorActionType.Default;
		

		////////////////////////////////////////////////////////////////////////////////////////////////
		/*--------------------------------------------------------------------------------------------*/
		public SelectorActionType Action {
			get { return _Action; }
			set { _Action = value; }
		}

	}

}
