using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GM
{
    public class Unit : MonoBehaviour
    { 
        [Header("States")]
        public bool move;
        public bool onGround;
        public State curState;
        bool prevGround;
        bool movingLeft;
        bool initLerp;
        public bool isUmbrella;
        public bool isDiggingForward;
        
        [Header("Attributes")]
        int fallPixelAmount;
        int digForwardFrames = 20;
        int dfCounter;
        int digDownFrames = 35;
        int ddCounter;
        int airDeathFrames = 80;
        float explodeTimer = 1.02f;
        float explodeTimerCounter;
        float explodeRadius = 6f;

        float lerpSpeed = 0.3f;
        float fallSpeed = 10.0f;    
        float digSpeed = 0.05f;
        float umbrellaSpeed = 0.3f;

        [Header("References")]
        public GameManager gameManager;    
        public SpriteRenderer rend;
        public Animator animator;
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
                    Walk(delta);
                    break;
                case State.stop:
                    Stop(delta);
                    break;
                case State.digDown:
                    if (!isUmbrella)
                        DiggingDown(delta);
                    break;
                case State.digForward:
                    if (!isUmbrella)
                        DiggingForward(delta);
                    break;
                case State.dead:
                    break;
                case State.explode:
                    Explode(delta);
                    break;
                default: break;
                
            }

        }
        public bool ChangeState(State s) 
        {
            switch (s)
            {
                case State.walk:
                    isUmbrella = false;
                    curState = s;
                    animator.Play("walking");
                    break;
                case State.stop:
                    if (prevGround && (gameManager.stopAbilityUsesLeft > 0))
                    {
                        gameManager.stopAbilityUsesLeft--;
                        gameManager.stopText.text = gameManager.stopAbilityUsesLeft.ToString();
                        isUmbrella = false;
                        CreateStoppedNodes();
                        curState = s;
                        animator.Play("stopping");
                        return true;
                    }
                    else return false;
                case State.umbrella:
                    if (gameManager.umbrellaAbilityUsesLeft <= 0)
                        return false;
                    gameManager.umbrellaAbilityUsesLeft--;
                    gameManager.umbrellaText.text = gameManager.umbrellaAbilityUsesLeft.ToString();
                    isUmbrella = true;
                    break;
                case State.digDown:
                    if (prevGround && (gameManager.digDownAbilityUsesLeft > 0))
                    {
                        gameManager.digDownAbilityUsesLeft--;
                        gameManager.digDownText.text = gameManager.digDownAbilityUsesLeft.ToString();
                        isUmbrella = false;
                        ddCounter = 0;
                        curState = s;
                        animator.Play("diggingDown");
                        return true;
                    }
                    else return false;
                case State.digForward:
                    if (prevGround && (gameManager.digForwardAbilityUsesLeft > 0))
                    {
                        gameManager.digForwardAbilityUsesLeft--;
                        gameManager.digForwardText.text = gameManager.digForwardAbilityUsesLeft.ToString();
                        isUmbrella = false;
                        dfCounter = 0;
                        isDiggingForward = true;
                        return true;
                    }
                    else return false;                
                case State.dead:
                    lerpSpeed = 0;
                    curState = s;
                    animator.Play("death_landing");
                    break;
                case State.explode:
                    if (gameManager.explodeAbilityUsesLeft <= 0)
                        return false;
                    gameManager.explodeAbilityUsesLeft--;
                    gameManager.explodeText.text = gameManager.explodeAbilityUsesLeft.ToString();
                    curState = s;
                    animator.Play("dying");
                    explodeTimerCounter = 0;
                    break;
                default: 
                    break;
            }
            return true;
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
                MoveToPos(delta);
            }
        }

        void Stop(float delta)
        {
            Node belowLeft = gameManager.GetNodeFromXY(curNode.x - 1, curNode.y - 1);
            Node below = gameManager.GetNodeFromXY(curNode.x, curNode.y - 1);
            Node belowRight = gameManager.GetNodeFromXY(curNode.x + 1, curNode.y - 1);
            if ((below.isEmpty || below == null) && (belowRight.isEmpty || belowRight == null) && (belowLeft.isEmpty || belowLeft == null))
            {
                ChangeState(State.walk);
                return;
            }
        }

        void Explode(float delta)
        {
            explodeTimerCounter += delta;
            if (explodeTimerCounter > explodeTimer)
            {
                targetNode = curNode;
                prevGround = onGround;
                double radius = explodeRadius * 0.01;
                int steps = Mathf.RoundToInt(explodeRadius);
                Vector3 center = transform.position;
                List<Node> explodable = new List<Node>();
                for (int x = -steps; x < steps; x++)
                {
                    for (int y = -steps; y < steps; y++)
                    {
                        int newx = x + curNode.x;
                        int newy = y + curNode.y;
                        float d = Vector3.Distance(center, gameManager.GetWorldPositionFromNode(newx, newy));
                        if (d > radius) 
                            continue;
                        Node checkNode = gameManager.GetNodeFromXY(newx, newy);
                        if (checkNode == null)
                            continue;
                        explodable.Add(checkNode);

                    }
                }
                gameManager.AddNodesToBeCleared(explodable);
            }
        }        

        void MoveToPos(float delta)
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

        void DiggingDown(float delta)
        {
            if (!initLerp)
            {
                startPos = transform.position;
                t = 0;
                initLerp = true;
                int newX = (movingLeft) ? curNode.x + 1 : curNode.x - 1;
                Node aroundNode = gameManager.GetNodeFromXY(newX, curNode.y + 1);
                List<Node> diggable = FindNodes(aroundNode, 4);
                if (diggable.Count == 0 || ddCounter > digDownFrames) 
                {
                    ChangeState(State.walk);
                    return;
                }
                ddCounter++;
                gameManager.AddNodesToBeCleared(diggable);
                Node tmp = gameManager.GetNodeFromXY(curNode.x, curNode.y - 1);
                if (tmp == null)
                {
                    ChangeState(State.walk);
                    return;
                }
                targetNode = tmp;
                targetPos = gameManager.GetWorldPositionFromNode(targetNode);
                float d = Vector3.Distance(targetPos, startPos);
                baseSpeed = digSpeed / d;
            }
            else
            {
                MoveToPos(delta);
            }
        }

        void DiggingForward(float delta)
        {
            if (!initLerp)
            {
                startPos = transform.position;
                t = 0;
                initLerp = true;
                int newX = (movingLeft) ? curNode.x - 2 : curNode.x + 2;
                Node aroundNode = gameManager.GetNodeFromXY(newX, curNode.y + 4);
                List<Node> diggable = FindNodes(aroundNode, 5);
                if (diggable.Count == 0 || dfCounter > digForwardFrames) 
                {
                    ChangeState(State.walk);
                    isDiggingForward = false;
                    return;
                }
                dfCounter++;
                gameManager.AddNodesToBeCleared(diggable);
                Node tmp = gameManager.GetNodeFromXY(newX, curNode.y);
                if (tmp == null)
                {
                    ChangeState(State.walk);
                    return;
                }
                targetNode = tmp;
                targetPos = gameManager.GetWorldPositionFromNode(targetNode);
                float d = Vector3.Distance(targetPos, startPos);
                baseSpeed = digSpeed / d;
            }
            else
            {
                MoveToPos(delta);
            }

        }
        //najde nodes okolo danej node, pouzivam pri kopani dopredu/dole, nech viem, ci kopat, alebo zrusit kopanie
        List<Node> FindNodes(Node around, float rad)
        {
            List<Node> list = new List<Node>();
            Vector3 center = gameManager.GetWorldPositionFromNode(around);
            float radius = rad * 0.01f;
            for (int x = -6; x < 6; x++)
            {
                for (int y = -6; y < 6; y++)
                {
                    int newx = x + curNode.x;
                    int newy = y + curNode.y;
                    float d = Vector3.Distance(center, gameManager.GetWorldPositionFromNode(newx, newy));
                    if (d > radius) 
                        continue;
                    Node checkNode = gameManager.GetNodeFromXY(newx, newy);
                    if (checkNode == null)
                        continue;
                    if (!checkNode.isEmpty)
                        list.Add(checkNode);
                }
            }
            return list;
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
                fallPixelAmount++;
                if (onGround) 
                {  
                    //fallPixelAmount osetruje to, aby sa nezapinala padacia animacia, ked robi jednotka iba "kroky" po "schodoch" smerom dole
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
                    if ((fallPixelAmount > airDeathFrames) && (!isUmbrella)) //ak si padal moc dlho, umres   
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
                        bool startDigging = false;
                        int howHigh = 0;
                        for (int height = 0; height < 4; height++) 
                        {
                            targetIsAir = IsAir(targetX, targetY + height);
                            if (isDiggingForward && (height > 2) && (!targetIsAir))
                            {
                                startDigging = true;
                                break;
                            }
                            if (targetIsAir) 
                            {
                                isValid = true;
                                howHigh = height;
                                break;
                            }
                        }
                        if (isValid && !startDigging) 
                        {
                            targetY += howHigh;
                        }
                        else 
                        {
                            if (startDigging)
                            {
                                curState = State.digForward;
                                animator.Play("diggingForward");
                                return false;
                            }
                            else 
                            {
                                movingLeft = !movingLeft;
                                targetX = (movingLeft) ? curNode.x -1 : curNode.x +1;
                            }
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
        //vytvori "blok" okolo jednotky, ktora ma STOP abilitu, nech realne STOP funguje
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
        //vycisti ^
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
