using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class GameSystemManager : MonoBehaviour
{
    GameObject inputFieldUsername, inputFieldPassaword, buttomSubmit, toggleCreate, toggleLogin, networkClient, chatInput, findGameSessionButton, sendButton, chatDisplay, replayButton, obserButton, backButtom;

    public GameObject textPrefab, chatView;

    private int m_iChatMaxMessages = 30;

    public const string m_sPlayerOneMove = "X", m_sPlayerTwoMove = "O";

    public player m_pMe, m_pEnemy;

    public GameStates m_gsCurrentGameState;

    public LinkedList<chatMessage> m_lkMessagesForChat;

    public LinkedList<Spot> m_lkSpotsForReplay;
    private LinkedList<Spot> m_lkReplayToConsume;

    private int m_iReplayIterator;

    private bool m_bHasXwon = false, m_bHasOwon = false;

    public enum ClientToServerSignifiers
    {
        LOGIN = 0,
        CREATE_USER = 1,
        ADD_TO_GAME_SESSION = 2,
        PLAY_WAS_MADE = 3,
        CHAT_MSG = 4,
        JOIN_AS_OBSERVER = 5,
        LEAVE_GAME_SESSION = 6
    }

    public enum ServerToClientSignifiers
    {
        LOGIN_FALIED = -1,
        LOGIN_SUCCESS = 0,
        CREATE_USER_SUCCESS = 1,
        CREATE_USER_FALIED = 2,
        GAME_SESSION_STARTED = 3,
        OPPONENT_PLAY = 4,
        FIRST_PLAYER = 5,
        SECOND_PLAYER = 6,
        CHAT_MSG = 7,
        OBSERVER = 8,
    
    }

    public enum GameStates
    {
        LOGIN = 0,
        MAIN_MENU = 1,
        WAITING_FOR_MATCH = 2,
        PLAYING_TIC_TAC_TOE = 3,
        GAME_OVER = 4,
        OBSERVING_GAME = 5 
    }
    void Start()
    {
        GameObject[] allObjecs = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach(GameObject go in allObjecs)
        {
            switch (go.name)
            {
                case "InputFieldUsername":
                    inputFieldUsername = go;
                    break;
                case "InputFieldPassaword":
                   inputFieldPassaword = go;
                    break;
                case "SubmitButton":
                    buttomSubmit = go;
                    break;
                case "ToggleCreate":
                    toggleCreate = go;
                    break;
                case "ToggleLogin":
                    toggleLogin = go;
                    break;
                case "NetworkClient":
                    networkClient = go;
                    break;
                case "Find Game":
                    findGameSessionButton = go;
                    break;
                case "ChatInput":
                    chatInput = go;
                    break;
                case "ChatView":
                    chatDisplay = go;
                    break;
                case "SendButton":
                    sendButton = go;
                    break;
                case "ReplayButton":
                    replayButton = go;
                    break;
                case "JoinAsObserver":
                    obserButton = go;
                    break;
                case "BackButton":
                    backButtom = go; 
                    break;
                default:
                    break;
            }

        }

        buttomSubmit.GetComponent<Button>().onClick.AddListener(OnClickButtonSubmit);

        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleChangeCreate);

        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleChangeLogin);

        findGameSessionButton.GetComponent<Button>().onClick.AddListener(FindGameSessionButtonPressed);

        obserButton.GetComponent<Button>().onClick.AddListener(JoinAsAnObserverButtonPressed);

        sendButton.GetComponent<Button>().onClick.AddListener(SendTextFromInputFieldButtonPressed);

        replayButton.GetComponent<Button>().onClick.AddListener(ReplayButtonPressed);
        
        backButtom.GetComponent<Button>().onClick.AddListener(BackButtomPressed);



        //for the game view

        m_lkMessagesForChat = new LinkedList<chatMessage>();
        m_lkSpotsForReplay = new LinkedList<Spot>();
        foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
        {
            sp.GetComponent<Button>().onClick.AddListener(MakeAPlayButtonPressed);
        }

        ChangeGameState(GameStates.LOGIN);
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckGameOver() && m_gsCurrentGameState == GameStates.PLAYING_TIC_TAC_TOE)
        {
            if (m_bHasXwon)
            {
                ChatSendMessage("X WINN!!11!!", Color.red);
                m_bHasXwon = false;
            }
            if (m_bHasOwon)
            {
                ChatSendMessage("O WINN!!11!!", Color.red);
                m_bHasOwon = false;
            }


            ChangeGameState(GameStates.GAME_OVER);
        }
    }

    // ====== Buttons and Input ===========
    public void OnClickButtonSubmit()
    {

        string n = inputFieldUsername.GetComponent<InputField>().text;
        string p = inputFieldPassaword.GetComponent<InputField>().text;

        if (toggleLogin.GetComponent<Toggle>().isOn)
        {
            networkClient.GetComponent<NetworkedClient>().SendMessageToHost((int)ClientToServerSignifiers.LOGIN + "," + n + "," + p);
        }
        else
        {
           networkClient.GetComponent<NetworkedClient>().SendMessageToHost((int)ClientToServerSignifiers.CREATE_USER + "," + n + "," + p);
        }
        
      
    }

    public void OnToggleChangeCreate(bool newValue)
    {
        toggleLogin.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }
    public void OnToggleChangeLogin(bool newValue)
    {
        toggleCreate.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    private void FindGameSessionButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.ADD_TO_GAME_SESSION).ToString());
        ChangeGameState(GameStates.WAITING_FOR_MATCH);
    }

    private void MakeAPlayButtonPressed()
    {
        if (networkClient.GetComponent<NetworkedClient>().isInputEnable)
        {

            Spot sp = EventSystem.current.currentSelectedGameObject.GetComponent<Spot>();

            if (!sp.isOccupied)
            {
                sp.isOccupied = true;
                sp.ChangeButonText(m_pMe.m_sMove);
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.PLAY_WAS_MADE).ToString() + "," + sp.m_iterator.ToString());

                m_lkSpotsForReplay.AddLast(sp);
            }

            networkClient.GetComponent<NetworkedClient>().isInputEnable = false;

        }
    }

    private void JoinAsAnObserverButtonPressed()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.JOIN_AS_OBSERVER).ToString());
        ChangeGameState(GameStates.WAITING_FOR_MATCH);
    }

    public void SendTextFromInputFieldButtonPressed()
    {
        string text = chatInput.GetComponent<InputField>().text;
        ChatSendMessage(text, Color.green);
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.CHAT_MSG).ToString() + "," + text);
        chatInput.GetComponent<InputField>().Select();
        chatInput.GetComponent<InputField>().text = "";
    }

    public void ReplayButtonPressed()
    {
        //Clean the Board
        foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
        {
            sp.m_Button.GetComponentInChildren<Text>().text = "";
        }

        //initiate replay iterator
        m_iReplayIterator = 0;

        //kill relpplay button until the end of the replay
        replayButton.SetActive(false);

        //kill mouse input
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //start replay coroutine;
        StartCoroutine(ReplayRoutine());
    }

    public void BackButtomPressed()
    {
        m_lkSpotsForReplay.Clear();

        //clean board
        foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
        {
            sp.gameObject.GetComponentInChildren<Text>().text = "";
            sp.isOccupied = false;
        }


        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.LEAVE_GAME_SESSION).ToString());

        ChangeGameState(GameStates.MAIN_MENU);
    }
    //========================================

    //======== Game States and Game logic ===========
    public void ChangeGameState(GameStates gameState)
    {
        m_gsCurrentGameState = gameState;

        inputFieldUsername.SetActive(false);
        inputFieldPassaword.SetActive(false);
        buttomSubmit.SetActive(false);
        toggleCreate.SetActive(false);
        toggleLogin.SetActive(false);
        
        findGameSessionButton.SetActive(false);

        chatInput.SetActive(false);
        sendButton.SetActive(false);
        chatDisplay.SetActive(false);
        replayButton.SetActive(false);
        backButtom.SetActive(false);
        obserButton.SetActive(false);



        foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
        {
            sp.gameObject.SetActive(false);
        }

        if(gameState == GameStates.LOGIN)
        {
            inputFieldUsername.SetActive(true);
            inputFieldPassaword.SetActive(true);
            buttomSubmit.SetActive(true);
            toggleCreate.SetActive(true);
            toggleLogin.SetActive(true);
        }

        if (gameState == GameStates.MAIN_MENU)
        {
            
            findGameSessionButton.SetActive(true);
            obserButton.SetActive(true);
        }

        if (gameState == GameStates.WAITING_FOR_MATCH)
        {

        }

        if (gameState == GameStates.PLAYING_TIC_TAC_TOE)
        {
            foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
            {
                sp.gameObject.SetActive(true);
            }

            chatInput.SetActive(true);
            sendButton.SetActive(true);
            chatDisplay.SetActive(true);

        }

        if (gameState == GameStates.GAME_OVER)
        {
            foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
            {
                sp.gameObject.SetActive(true);
                sp.isOccupied = true;
            }

            chatInput.SetActive(true);
            sendButton.SetActive(true);
            chatDisplay.SetActive(true);
            
            backButtom.SetActive(true);
            replayButton.SetActive(true);
        }

        if (gameState == GameStates.OBSERVING_GAME)
        {
            foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
            {
                sp.gameObject.SetActive(true);
            }

            chatInput.SetActive(true);
            sendButton.SetActive(true);
            chatDisplay.SetActive(true);
        }
    }

    private bool CheckGameOver()
    {
        return CheckCombination(0, 1, 2) || CheckCombination(3, 4, 5) || CheckCombination(6, 7, 8) ||
               CheckCombination(0, 3, 6) || CheckCombination(1, 4, 7) || CheckCombination(2, 5, 8) ||
               CheckCombination(0, 4, 8) || CheckCombination(2, 4, 6);
    }

    private bool CheckCombination(int i, int j, int k)
    {
        string iString = null;
        string jString = null;
        string kString = null;

        foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
        {
            if(sp.m_iterator == i && sp.m_Button != null)
            {
                iString = sp.m_Button.GetComponentInChildren<Text>().text;
            }
            else if(sp.m_iterator == j && sp.m_Button != null)
            {
                jString = sp.m_Button.GetComponentInChildren<Text>().text;
            }
            else if (sp.m_iterator == k && sp.m_Button != null)
            {
                kString = sp.m_Button.GetComponentInChildren<Text>().text;
            }
        }

        if (iString == "X" && jString == "X" && kString == "X")
        {
            m_bHasXwon = true;
            return true;
        }
        else if (iString == "O" && jString == "O" && kString == "O")
        {
            m_bHasOwon = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetMeAsFirstPLayer()
    {
        m_pMe.m_sMove = m_sPlayerOneMove;
        m_pEnemy.m_sMove = m_sPlayerTwoMove;
    }

    public void SetMeAsSecondPLayer()
    {
        m_pEnemy.m_sMove = m_sPlayerOneMove;
        m_pMe.m_sMove = m_sPlayerTwoMove;
    }

    //========================================


    //============== IEnumerators and chat message ========
    public void ChatSendMessage(string text, Color color)
    {
        if(m_lkMessagesForChat.Count >= m_iChatMaxMessages)
        {
            Destroy(m_lkMessagesForChat.First.Value.m_tTextObject.gameObject);

            m_lkMessagesForChat.RemoveFirst();
        }
        chatMessage chatMessage = new chatMessage();

       
        chatMessage.m_sText = text;

        GameObject newText = Instantiate(textPrefab, chatView.transform);

        chatMessage.m_tTextObject = newText.GetComponent<Text>();

        chatMessage.m_tTextObject.text = chatMessage.m_sText;

        chatMessage.m_tTextObject.color = color;

        m_lkMessagesForChat.AddLast(chatMessage);
    }

    public IEnumerator ReplayRoutine()
    {
        if (m_iReplayIterator == 0)
        {
            m_lkReplayToConsume = new LinkedList<Spot>(m_lkSpotsForReplay);
        }

        if (m_lkReplayToConsume.Count > 0)
        {

            foreach(Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
            {
                
                if (sp.m_iterator == m_lkReplayToConsume.First.Value.m_iterator)
                {
                    sp.ChangeButonText(m_lkReplayToConsume.First.Value.m_ButtonText);
                }
            }

            Destroy(m_lkReplayToConsume.First.Value);
            m_lkReplayToConsume.RemoveFirst();
            m_iReplayIterator++;

        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            m_iReplayIterator = 0;
            replayButton.SetActive(true);
            yield return null;
        }


        yield return new WaitForSeconds(1);

        StartCoroutine(ReplayRoutine());

    }

    //========================================
}


//==== Classes =====
[System.Serializable]
public class chatMessage
{
    public string m_sText;

    public Text m_tTextObject;
}

[System.Serializable]
public class player
{
    public string m_sMove;
}