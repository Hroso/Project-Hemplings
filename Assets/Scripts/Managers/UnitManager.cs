using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GM 
{
	public class UnitManager : MonoBehaviour 
	{
		GameManager gameManager;
		public float maxUnits = 10; 
		public float timeScale = 1; // pomocou timeScale vieme menit "rychlost" pohybu jednotiek
		float delta;
		public float interval = 1;
		float timer = 0;
		public GameObject unitPrefab;
		GameObject unitsParent;
		List<Unit> all_units = new List<Unit>();
		public static UnitManager singleton;
		void Awake ()
		{
			singleton = this;
		}

		void Start ()
		{
			unitsParent = new GameObject();
			unitsParent.name = "units parent";
			gameManager = GameManager.singleton;
		}

		void Update () 
		{
			delta = Time.deltaTime * timeScale;
			timer -= delta;
			//spawnovanie jednotiek
			if ((timer < 0) && (all_units.Count < maxUnits))
			{
				timer = interval;
				CreateUnit();
			}
			for (int i = 0; i < all_units.Count; i++) 
			{
				all_units[i].Tick(delta);
			}
		}
		//funkcia prebehne vsetky units a zisti, ktory je najblizsie ku kurzoru (alebo inemu vektoru, ktory zadame)
		public Unit MouseOnUnit(Vector3 xyz)
		{
			Unit closest = null;
			float minDist = 0.15f;
			for (int i = 0; i < all_units.Count; i++)
			{
				float curDist = Vector3.Distance(xyz, all_units[i].transform.position);
				if (curDist < minDist)
				{
					minDist = curDist;
					closest = all_units[i];
				}
			}
			return closest;
		}
		//funkcia na vytvorenie jednotky
		void CreateUnit()
		{
			GameObject g = Instantiate(unitPrefab);
			g.transform.parent = unitsParent.transform;
			Unit u = g.GetComponent<Unit>();
			u.Init(gameManager);
			all_units.Add(u);
			u.move=true;
		}
	}
}