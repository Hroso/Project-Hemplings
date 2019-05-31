using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace GM
{
	public class InterfaceManager : MonoBehaviour 
	{
		public Transform mouseTransform;
		public Image mouse;
		public Sprite cross1, cross2;
		public Sprite box;
		public bool overUnit;
		public bool switchToState;
		public State tState;
		public ButtonManager curButton;
		public Color selectTint;
		Color defColor;
		void Start () 
		{
			Cursor.visible = false;
		}	
		public void PressButton(ButtonManager button)
		{
			if (curButton)
			{
				curButton.buttonImage.color = defColor;
			}
			curButton = button;
			defColor = curButton.buttonImage.color;
			curButton.buttonImage.color = selectTint;
			tState = curButton.state;
		}
		public void Tick ()
		{
			mouseTransform.transform.position = Input.mousePosition;
			if (overUnit)
			{
				mouse.sprite = box;
			}
			else mouse.sprite = cross1;
		}
		public static InterfaceManager singleton;
		void Awake ()
		{
			singleton = this;
		}

	}	
}
