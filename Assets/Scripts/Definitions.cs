using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GM
{
		//rozne stavy/abilities, ktore budeme aplikovat na jednotky (default=walk)
	public enum State
	{
		walk, stop, umbrella, digDown, digForward, climb, explode, dead
	};
	public class Node
    {
        public int x;
        public int y;
        public bool isEmpty;
		public bool isStopped;
    }
}


