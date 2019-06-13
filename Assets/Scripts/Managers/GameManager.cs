using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace GM 
{

    public class GameManager : MonoBehaviour
    {
        public Texture2D levelTexture;
        public SpriteRenderer levelRenderer;
        Texture2D textureInstance;

        public float posOffset = 0.01f;   
        int maxWidth;
        int maxHeight;
        Node[,] table;

        Node curNode;
        Node prevNode;
        Vector3 mouse;

        public static GameManager singleton;
        public Vector3 spawnPos;
        public Transform spawnTransform;
        public Node spawnNode;
        Unit curUnit;
        public float editRadius = 6;
        public bool overUIElement;
        UnitManager unitManager;
        InterfaceManager interfaceManager;
        List<Node> clearNodes = new List<Node>(); 

        bool applyTexture;
        
        public int stopAbilityUsesLeft = 5;
        public int umbrellaAbilityUsesLeft = 5;
        public int digDownAbilityUsesLeft = 5;
        public int digForwardAbilityUsesLeft = 5;
        public int explodeAbilityUsesLeft = 5;
        public Text stopText;
        public Text umbrellaText;
        public Text digDownText;
        public Text digForwardText;
        public Text explodeText;
        private void Awake()
        {
            singleton = this;
        }

        private void Start()
        {
            //inicializacia mapy a UIcka
            CreateMap();
            unitManager = UnitManager.singleton;
            interfaceManager = InterfaceManager.singleton;
            spawnNode = GetNodeFromWorldPosition(spawnTransform.position);
            spawnPos = GetWorldPositionFromNode(spawnNode);
            //inicializacia textu v buttonoch
            stopText.text = stopAbilityUsesLeft.ToString();
            umbrellaText.text = umbrellaAbilityUsesLeft.ToString();
            digDownText.text = digDownAbilityUsesLeft.ToString();
            digForwardText.text = digForwardAbilityUsesLeft.ToString();
            explodeText.text = explodeAbilityUsesLeft.ToString();
        }
        //vytvorenie mapy po pixeloch
        void CreateMap()
        {
            maxWidth = levelTexture.width;
            maxHeight = levelTexture.height;
            table = new Node[maxWidth, maxHeight];
            textureInstance = new Texture2D(maxWidth, maxHeight);
            textureInstance.filterMode = FilterMode.Point;
            for (int i = 0; i < maxWidth; i++)
            {
                for (int j = 0; j < maxHeight; j++)
                {
                    Node temp = new Node();
                    temp.x = i;
                    temp.y = j;
                    Color asdf = levelTexture.GetPixel(i, j);
                    textureInstance.SetPixel(i, j, asdf);
                    if (asdf.a == 0)
                        temp.isEmpty = true;
                    else temp.isEmpty = false;
                    table[i, j] = temp;
                }
            }
            textureInstance.Apply();
            Rect renderer = new Rect(0, 0, maxWidth, maxHeight);
            levelRenderer.sprite = Sprite.Create(textureInstance, renderer, Vector2.zero, 100, 0, SpriteMeshType.FullRect);

        }   
        //ziskame poziciu mysky (na neskorsiu pracu s nou)
        void GetMousePos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            mouse = ray.GetPoint(5);
            curNode = GetNodeFromWorldPosition(mouse);
        }
        //nasledovne funkcie su na pracu s mapou, vieme si z Nodu vytiahnut X,Y suradnice a naopak
        public Node GetNodeFromXY(int x, int y)
        {
            if (x < 0 || y < 0 || x > maxWidth - 1 || y > maxHeight - 1)
            {
                return null;
            }
            else return table[x, y];
        }
        public Node GetNodeFromWorldPosition(Vector3 asdf)
        {
            int newx = Mathf.RoundToInt(asdf.x / posOffset);
            int newy = Mathf.RoundToInt(asdf.y / posOffset);
            return GetNodeFromXY(newx, newy);
        }
        public Vector3 GetWorldPositionFromNode(int x, int y)
        {
            Vector3 tmp = Vector3.zero;
            tmp.x = x * posOffset;
            tmp.y = y * posOffset;
            return tmp;
        }
        public Vector3 GetWorldPositionFromNode(Node n)
        {
            if (n == null)
                return -Vector3.one;
            Vector3 tmp = Vector3.zero;
            tmp.x = n.x * posOffset;
            tmp.y = n.y * posOffset;
            return tmp;

        }
        //vymaze dane pixely z mapy, vyuzite napriklad pri kopani
        public void ClearNodes() 
        {
            if (clearNodes.Count == 0)
                return;
            Color tmp = Color.white;
            tmp.a = 0;
            for (int i = 0; i < clearNodes.Count; i++) 
            {
                clearNodes[i].isEmpty = true;
                textureInstance.SetPixel(clearNodes[i].x, clearNodes[i].y, tmp);
            }
            clearNodes.Clear();
            applyTexture = true;
        }
        public void AddNodesToBeCleared(List<Node> toClear)
        {
            clearNodes.AddRange(toClear);
        }
    

        //funkcia "guma" (na mazanie mapy v runtime)
        void HandleInput()
        {
            if (curNode == null)
                return;
            if (Input.GetMouseButton(0))
            {
                if (curNode != prevNode)
                {
                    prevNode = curNode;
                    Color asdf = Color.white;
                    asdf.a = 0;
                    Vector3 center = GetWorldPositionFromNode(curNode);
                    float radius = editRadius * posOffset;
                    for (int x = -6; x < 6; x++)
                    {
                        for (int y = -6; y < 6; y++)
                        {
                            int newx = x + curNode.x;
                            int newy = y + curNode.y;

                            float d = Vector3.Distance(center, GetWorldPositionFromNode(newx, newy));
                            if (d > radius) 
                                continue;

                            Node checkNode = GetNodeFromXY(newx, newy);
                            if (checkNode == null)
                                continue;
                            //checkNode.isEmpty = true;
                            textureInstance.SetPixel(newx, newy, asdf);
                        }
                    }
                    applyTexture = true;
                }
            }
        }

        void HandleUnit()
        {  
            if (curUnit == null)
                return;
            if (Input.GetMouseButtonUp(0))
            {
                if (interfaceManager.tState == State.walk)
                    return;
                if (curUnit.curState == State.walk)
                    curUnit.ChangeState(interfaceManager.tState);
                if (curUnit.curState == State.stop)
                {
                    if (interfaceManager.tState == State.explode)
                    {
                        curUnit.ChangeState(State.walk);
                        curUnit.ChangeState(State.explode);
                    }
                }
            }
        }
        void CheckForUnit()
        {
            mouse.z = 0;
            curUnit = unitManager.MouseOnUnit(mouse);
            if (curUnit == null)
            {
                interfaceManager.overUnit = false;
                return;
            }
            interfaceManager.overUnit = true;

        }
        private void Update()
        {
            overUIElement = EventSystem.current.IsPointerOverGameObject();
            GetMousePos();
            CheckForUnit();
            interfaceManager.Tick();
            HandleUnit();
            ClearNodes();    
            if (applyTexture)
            {
                textureInstance.Apply();
                applyTexture = false;
            }
            //HandleInput();
        }
    }
    

}
