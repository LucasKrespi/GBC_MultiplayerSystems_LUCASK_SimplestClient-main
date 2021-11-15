using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class GameSystemManager : MonoBehaviour
{
    GameObject inputFieldUsername, inputFieldPassaword, buttomSubmit, toggleCreate, toggleLogin, networkClient, chatInput, findGameSessionButton, sendButton, chatDisplay, replayButton, obserButton;

    public GameObject textPrefab, chatView;

    private int chatMaxMessages = 30;

    public const string m_sPlayerOneMove = "X", m_sPlayerTwoMove = "O";

    public player me, enemy;

    public GameStates currentGameState;

    public LinkedList<chatMessage> m_lkMessagesForChat;

    public LinkedList<Spot> m_lkSpotsForReplay;

    private bool hasXwon = false, hasOwon = false;
    public enum ClientToServerSignifiers
    {
        LOGIN = 0,
        CREATE_USER = 1,
        ADD_TO_GAME_SESSION = 2,
        PLAY_WAS_MADE = 3,
        CHAT_MSG = 4,
        JOIN_AS_OBSERVER = 5
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
        OBSERVER = 8
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
                default:
                    break;
            }

        }

        buttomSubmit.GetComponent<Button>().onClick.AddListener(OnClickButtonSubmit);
        toggleCreate.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleChangeCreate);
        toggleLogin.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleChangeLogin);

        findGameSessionButton.GetComponent<Button>().onClick.AddListener(FindGameSessionButtonPressed);

        sendButton.GetComponent<Button>().onClick.AddListener(sendTextFromInputField);
        replayButton.GetComponent<Button>().onClick.AddListener(replayButtonPressed);

        obserButton.GetComponent<Button>().onClick.AddListener(JoinAsAnObserver);


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
        if (CheckGameOver() && currentGameState != GameStates.GAME_OVER)
        {
            if (hasXwon)
            {
                chatSendMessage("X WINN!!11!!", Color.red);
                hasXwon = false;
            }
            if (hasOwon)
            {
                chatSendMessage("O WINN!!11!!", Color.red);
                hasOwon = false;
            }


            ChangeGameState(GameStates.GAME_OVER);
        }
    }

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
                sp.ChangeButonText(me.move);
                networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.PLAY_WAS_MADE).ToString() + "," + sp.m_iterator.ToString());

                m_lkSpotsForReplay.AddLast(sp);
            }

            networkClient.GetComponent<NetworkedClient>().isInputEnable = false;

        }
    }

    private void JoinAsAnObserver()
    {
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.JOIN_AS_OBSERVER).ToString());
        ChangeGameState(GameStates.WAITING_FOR_MATCH);
    }

    public void ChangeGameState(GameStates gameState)
    {
        currentGameState = gameState;

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
        return checkCombination(0, 1, 2) || checkCombination(3, 4, 5) || checkCombination(6, 7, 8) ||
               checkCombination(0, 3, 6) || checkCombination(1, 4, 7) || checkCombination(2, 5, 8) ||
               checkCombination(0, 4, 8) || checkCombination(2, 4, 6);
    }

    private bool checkCombination(int i, int j, int k)
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
            hasXwon = true;
            return true;
        }
        else if (iString == "O" && jString == "O" && kString == "O")
        {
            hasOwon = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void chatSendMessage(string text, Color color)
    {
        if(m_lkMessagesForChat.Count >= chatMaxMessages)
        {
            Destroy(m_lkMessagesForChat.First.Value.textObject.gameObject);

            m_lkMessagesForChat.RemoveFirst();
        }
        chatMessage chatMessage = new chatMessage();

       
        chatMessage.text = text;

        GameObject newText = Instantiate(textPrefab, chatView.transform);

        chatMessage.textObject = newText.GetComponent<Text>();

        chatMessage.textObject.text = chatMessage.text;

        chatMessage.textObject.color = color;

        m_lkMessagesForChat.AddLast(chatMessage);
    }

    public void sendTextFromInputField()
    {
        string text = chatInput.GetComponent<InputField>().text;
        chatSendMessage(text, Color.green);
        networkClient.GetComponent<NetworkedClient>().SendMessageToHost(((int)ClientToServerSignifiers.CHAT_MSG).ToString() + "," + text);
        chatInput.GetComponent<InputField>().Select();
        chatInput.GetComponent<InputField>().text = "";
    }

    public void replayButtonPressed()
    {

        foreach (Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
        {
            sp.m_Button.GetComponentInChildren<Text>().text = "";
        }

        StartCoroutine(replay());
    }


    public IEnumerator replay()
    {
        if(m_lkSpotsForReplay.Count > 0)
        {
            foreach(Spot sp in FindObjectOfType<CreateBoard>().m_SpotList)
            {
                
                if (sp.m_iterator == m_lkSpotsForReplay.First.Value.m_iterator)
                {
                    sp.ChangeButonText(m_lkSpotsForReplay.First.Value.m_ButtonText);
                    Debug.Log("Should replay: " + m_lkSpotsForReplay.First.Value.m_ButtonText);
                }
            }

            m_lkSpotsForReplay.RemoveFirst();

        }
        else
        {
            yield return null;
        }


        yield return new WaitForSeconds(1);

        StartCoroutine(replay());

    }


    public void SetMeAsFirstPLayer()
    {
        me.move = m_sPlayerOneMove;
        enemy.move = m_sPlayerTwoMove;
    }

    public void SetMeAsSecondPLayer()
    {
        enemy.move = m_sPlayerOneMove;
        me.move = m_sPlayerTwoMove;
    }
}

[System.Serializable]
public class chatMessage
{
    public string text;

    public Text textObject;
}

[System.Serializable]
public class player
{
    public string move;
}