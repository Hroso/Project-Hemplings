using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

        Node currNode;
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
        private void Awake()
        {
            singleton = this;
        }

        private void Start()
        {
            CreateMap();
            unitManager = UnitManager.singleton;
            interfaceManager = InterfaceManager.singleton;
            spawnNode = GetNodeFromWorldPosition(spawnTransform.position);
            spawnPos = GetWorldPositionFromNode(spawnNode);
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
            levelRenderer.sprite = Sprite.Create(textureInstance, renderer, Vector2.zero);

        }   
        //ziskame poziciu mysky (na neskorsiu pracu s nou)
        void GetMousePos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            mouse = ray.GetPoint(5);
            currNode = GetNodeFromWorldPosition(mouse);
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
        //funkcia "guma" (na mazanie mapy v runtime)
        void HandleInput()
        {
            if (currNode == null)
                return;
            if (Input.GetMouseButton(0))
            {
                if (currNode != prevNode)
                {
                    prevNode = currNode;
                    Color asdf = Color.white;
                    asdf.a = 0;
                    Vector3 center = GetWorldPositionFromNode(currNode);
                    float radius = editRadius * posOffset;
                    for (int x = -6; x < 6; x++)
                    {
                        for (int y = -6; y < 6; y++)
                        {
                            int newx = x + currNode.x;
                            int newy = y + currNode.y;

                            float d = Vector3.Distance(center, GetWorldPositionFromNode(newx, newy));
                            if (d > radius) 
                                continue;

                            Node checkNode = GetNodeFromXY(newx, newy);
                            if (checkNode == null)
                                continue;
                            checkNode.isEmpty = true;
                            textureInstance.SetPixel(newx, newy, asdf);
                        }
                    }
                    textureInstance .Apply();
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
            //HandleInput();
        }
    }
    

}
