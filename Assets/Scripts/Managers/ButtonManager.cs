using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GM
{
	public class ButtonManager : MonoBehaviour 
	{
		public State state;
		public Image buttonImage;
		
		public void Press()
		{
			InterfaceManager.singleton.PressButton(this);
		}
	}
}
