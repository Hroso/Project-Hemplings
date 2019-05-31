using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GM
{
    public class Unit : MonoBehaviour
    { 
        public GameManager gameManager;    
        public SpriteRenderer rend;
        public Animator animator;
        public bool move;
        public bool onGround;
        public State curState;
        bool prevGround;
        bool movingLeft;
        bool initLerp;
        public bool isUmbrella;
        int fallPixelAmount;
        float lerpSpeed = 0.3f;
        float fallSpeed = 10.0f;
        float umbrellaSpeed = 0.3f;
        Vector3 targetPos;
        Vector3 startPos;
        float t;
        float baseSpeed;
        bool isInit;
        Node curNode;

        int targetX;
        int targetY;
        Node targetNode;
        List<Node> stoppedNodes = new List<Node>();
        public void Init(GameManager gm)
        {
            gameManager = gm;
            PlaceOnNode();
            isInit = true;
            curState = State.walk;
        }

        void PlaceOnNode()
        {
            curNode = GameManager.singleton.spawnNode;
            transform.position = GameManager.singleton.spawnPos;
        }
        //nasa Update funkcia, volame ju v UnitManageri pre kazdu jednotku osobitne kazdy frame 
        public void Tick(float delta)
        {
            if (!isInit)
                return;
            if (!move)
                return;
            rend.flipX = movingLeft;
            animator.SetBool("isUmbrella", isUmbrella);
            switch(curState)
            {
                case State.walk:
                case State.umbrella:
                case State.digForward:
                    Walk(delta);
                    break;
                case State.stop:
                    Stop();                
                    break;
                case State.digDown:
                    break;
                case State.dead:
                    break;
                default: break;
                
            }

        }

        void Walk(float delta) 
        {
            
            if (!initLerp)
            {
                startPos = transform.position;
                t = 0;
                initLerp = true;
                bool hasPath = Pathfinding();
                if (hasPath)
                {
                    Vector3 tp = gameManager.GetWorldPositionFromNode(targetNode);
                    targetPos = tp;
                }
                float d = Vector3.Distance(targetPos, startPos);
                if (onGround) 
                {
                    baseSpeed = lerpSpeed / d;
                }
                else 
                {
                    if (isUmbrella)
                    {
                        baseSpeed = umbrellaSpeed / d;                        
                    }
                    else
                    {
                        baseSpeed = fallSpeed / d;
                    }

                }
            }
            else
            {
                t += delta * baseSpeed;
                if (t > 1)
                {
                    t = 1;
                    initLerp = false;
                    curNode = targetNode;
                }
                Vector3 tp = Vector3.Lerp(startPos, targetPos, t);
                transform.position = tp;
            }
        }

        void Stop()
        {

        }

        public bool ChangeState(State s) 
        {
            isUmbrella = false;
            switch (s)
            {
                case State.walk:
                    curState = s;
                    animator.Play("walking");
                    break;
                case State.stop:
                    if (onGround)
                    {
                        CreateStoppedNodes();
                        curState = s;
                        animator.Play("stopping");
                        return true;
                    }
                    else return false;
                case State.umbrella:
                    isUmbrella = true;
                    break;
                case State.digDown:
                    break;
                case State.digForward:
                    break;
                case State.dead:
                    lerpSpeed = 0;
                    curState = s;
                    break;
                default: 
                    break;
            }
            return true;
        }
        //logika pohybu postaviciek
        bool Pathfinding()
        {
            if (curNode == null)
            {
                targetPos = transform.position;
                targetPos.y = -30;
                prevGround = onGround;
                return false;
            }
            targetX = curNode.x;
            targetY = curNode.y;
            bool airBelow = IsAir(targetX, targetY-1);
            bool airAhead = IsAir(targetX, targetY);
            if (airBelow)  //padaj
            {

                targetX = curNode.x;
                targetY -= 1;
                if (onGround) 
                {  
                    //fallPixelAmount osetruje to, aby sa nezapinala padacia animacia, ked robi jednotka iba "kroky" po "schodoch" smerom dole
                    fallPixelAmount++;
                    if (fallPixelAmount > 5) 
                    {  
                        animator.Play("falling");
                        onGround = false;
                    }
                }
            }
            else //uz si na zemi
            {
                onGround = true;
                if (onGround && !prevGround)  // dopadol si
                {
                    Debug.Log(fallPixelAmount);
                    if ((fallPixelAmount > 3) && (!isUmbrella)) //ak si padal moc dlho, umres   
                    {   
                        targetNode = curNode;
                        ChangeState(State.dead);
                        animator.Play("death_landing");
                        prevGround = onGround;
                        return true;
                    }
                    else
                    {
                        animator.Play("landing");
                        targetNode = curNode;
                        prevGround = onGround;
                        fallPixelAmount = 0;
                        return true;
                    }
                }
                fallPixelAmount = 0;
                bool stopped = IsStopped((movingLeft) ? targetX - 1 : targetX +1, targetY);
                if (!stopped)
                {                        
                    if (airAhead) // chod doprava/dolava
                    {
                        targetX -= (movingLeft) ? 1 : -1;
                        targetY = curNode.y;
                    }
                    else //zisti ci nevies vyliezt (ked je pred nami stena vysky <=3, tak ju proste prekroci)
                    {
                        bool isValid = false;
                        bool targetIsAir = false;
                        int howHigh = 0;
                        for (int height = 1; height < 4; height++) 
                        {
                            targetIsAir = IsAir(targetX, targetY + height);
                            if (targetIsAir) 
                            {
                                isValid = true;
                                howHigh = height;
                                break;
                            }
                        }
                        if (isValid) 
                        {
                            targetY += howHigh;
                        }
                        else 
                        {
                            movingLeft = !movingLeft;
                            targetX = (movingLeft) ? curNode.x -1 : curNode.x +1;
                        }
                    }
                }
                else 
                {
                    movingLeft = !movingLeft;
                    targetX = (movingLeft) ? curNode.x -1 : curNode.x +1;
                    targetY = curNode.y;
                }

            }
            targetNode = gameManager.GetNodeFromXY(targetX, targetY);
            prevGround = onGround;
            return true;

        }
        //funkcia, na zistovanie ci je dany Node vzduch (prazdny)
        bool IsAir(int x, int y) 
        {
            Node tmp = gameManager.GetNodeFromXY(x, y);
            if (tmp != null)
                return tmp.isEmpty;
            else return true;
        }
        //funkcia, na zistovanie, ci pred nami nieje STOP unit
        bool IsStopped(int x, int y) 
        {
            Node tmp = gameManager.GetNodeFromXY(x, y);
            if (tmp == null)
                return false;
            return tmp.isStopped;
        }
        
        void CreateStoppedNodes()   
        {
            for (int i = -2; i < 2; i++) 
            {
                for (int j = 0; j < 4; j++)
                {
                    Node tmp = gameManager.GetNodeFromXY(curNode.x + i, curNode.y + j);
                    if (tmp == null)
                        continue;
                    tmp.isStopped = true;
                    stoppedNodes.Add(tmp);
                    
                }
            }
        }
        void ClearStoppedNodes()
        {
            for (int i = 0; i < stoppedNodes.Count; i++)
            {
                stoppedNodes[i].isStopped = false;
            }
            stoppedNodes.Clear();
        }


    }
}
