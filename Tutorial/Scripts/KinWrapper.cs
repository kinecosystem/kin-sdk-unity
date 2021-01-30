/*
 * https://github.com/hitwill/kin-sdk-unity-tutorial/tree/master
 * Usage:
 * 1. Create an empty object in Unity and attach this script to it
 * 2. modify the variables in the editableVariables region
 * The wrapper will self initialize and you can simply call the public functions
 */

using UnityEngine;
using Kin;
using System;
using System.Text;
using System.Collections;
using UnityEngine.Networking;


public class KinWrapper : MonoBehaviour, IPaymentListener, IBalanceListener
{
    #region Editable Variables
    //edit these constants for your app
    //Kin address for your server TODO:/ enter server address here
    private  string serverKinAddress = ""; //public key of your server
    //URL to your server
    private string baseURL = "";// e.g."https://yourkinserver.com";

    //appId assigned by Kin foundation. Use "1acd" for testing
    private readonly string appId = "1acd";
    private readonly Kin.Environment environment = Kin.Environment.Test;
    
    //Your server URL  for client to request whitelisting
    private readonly string whitelistURL = "?whitelist=1";
    //Your server URL for client to request payments (from your server)
    private readonly string requestPaymentURL = "?request=1";
    //Your server URL for client to request payments (from your server)
    private readonly string fundURL = "?fund=1";
    //How long to wait before retrying
    private readonly float secondsBetweenRetry = 4f;
    //times to try initializing in case of network error
    private readonly int maxInitializes = 15;
    // upate caller on statuses of initialization 
    private readonly bool verbose = true;
    #endregion


    //No variables need editing from below
    #region Operating Variables
    private KinClient kinClient; //kin object
    private KinAccount kinAccount; //kin object
    private int initializationRetries = 0; //number of times we have retried in case of failure
    private bool fetchedUserBalance = false; //monitor the status of the user's balance
    private bool listenersActive = false; //monitor the status of the blockchain listeners
    private KinWrapper kinWrapper = null; //used to make sure only one instance of this class is running
    private readonly int fee = 100; //standard fee for Kin blockchain. This will be zero if your account is whitelisted
    private bool isInitialized = false; //monitor the status of the wrapper
    private Action<System.Object, string> listenerCallback; //a callback we can use to send events
    #endregion

    void Awake()
    {
        ///only allow one instance of this object across scenes in Unity
        MaintainOneInstance();
    }

    void Start(){
         //https://github.com/kinecosystem/kin-sdk-unity#get-started
        kinClient = new KinClient(environment, appId); //Declare a new client
        kinAccount = FetchAccount(); //Return the user's *local* account. If doesn't exist, create it first.
     }

    #region Initialization
    public void Initialize(Action <System.Object, string> callback = null, string url = null, string address = null)
    {
        /// Initialize the wrapper by:
        /// 1. Creating and Fetching the user's local address
        /// 2. Calling InitializeKin which handles functions that need internet connectivity
        //This code can only run on Android - so we check for this first
        if(callback!= null) listenerCallback = callback;

        if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
        {
            LogError("Kin can only run on Android or Android Emulator, not the Unity Editor or Unity Remote");
            return; //can only run on Android
        }
       serverKinAddress = address;
       baseURL = url;
        
        if (verbose) listenerCallback?.Invoke("Account Address:" + kinAccount.GetPublicAddress(), "log");
        if(address == "" || url == "")
        {
            if (verbose) listenerCallback?.Invoke("Cannot proceed without remote server", "log");
            return;
        }

        StartCoroutine(WaitForNet //Make sure user is online
            (InitializeKin) //Initilize the user's account online
        );
    }


    void InitializeKin()
    {
        /// Perform online functions by:
        /// 1. Checking if the user is onboarded (registered on the blockchain) and registering them
        /// 2. Fetching the user's current balance on the blockchain
        /// 3. Listening for any changes in the user's balance
        if (initializationRetries >= maxInitializes) return;
        //Check if the user has been onboarded (registered on the blockchain)
        //This will only be done once, then we persist it locally so we don't have to keep checking
        if (PlayerPrefs.GetInt("UserAccountOnboarded", 0) == 0)
        {
            //https://github.com/kinecosystem/kin-sdk-unity#query-account-status
            if (verbose) listenerCallback?.Invoke("Initializing", "log");
            if (verbose) listenerCallback?.Invoke("Getting account status", "log");
            kinAccount.GetStatus(GetStatusCallback); //check if onboarded and onboard if necessary
        }
        else if (fetchedUserBalance == false)
        {
            //https://github.com/kinecosystem/kin-sdk-unity#retrieving-balance
            if (verbose) listenerCallback?.Invoke("Updating account balance", "log");
            kinAccount.GetBalance(GetBalanceCallback); //Get the user's balance and persist it locally
        }
        else if (listenersActive == false)
        {
            //Listen for balance and payment changes and update the local value
            //This way, the local balance is always uptodate and immediately retrievable
            if (verbose) listenerCallback?.Invoke("Finished initializing", "log");
            AddListeners();
            isInitialized = true;
            if (verbose) listenerCallback?.Invoke("Listening to blockchain", "log");
        }
    }
    #endregion

    #region public functions
    /// <summary>
    /// Return user's public address
    /// </summary>
    /// <returns>User's public address</returns>
    public string PublicAddress()
    {
        string publicAddress = kinAccount.GetPublicAddress();// set so we can use it later
        return (publicAddress);
    }

    /// <summary>
    /// Return the cached user's balance - kept up to date by listenrs
    /// </summary>
    /// <returns>User's Kin balance</returns>
    public decimal Balance()
    {
        decimal balance = (decimal)PlayerPrefs.GetFloat("KinBalanceUser", 0f);
        return (balance);
    }


    /// <summary>
    /// Return if the wrapper has finished initializing
    /// </summary>
    /// <returns>True/False initialized</returns>
    public bool IsInitialized()
    {
        return (isInitialized);
    }

    /// <summary>
    /// Get notified when a paymenet happens on the user's account, or when their balance changes
    /// </summary>
    /// <param name="callback"></param>
    public void RegisterCallback(Action<System.Object, String> callback)
    {
        listenerCallback = callback;
    }

   

    /// <summary>
    /// Request the server (your server) to send a payment to the user
    /// </summary>
    /// <param name="amount">Amount of Kin to request</param>
    /// <param name="memo">A memo the server will use, for your records</param>
    /// <param name="onComplete">A function to call after the request is completed</param>
    IEnumerator RequestPayment(decimal amount, string memo, Action<bool> onComplete = null, bool fund = false)
    {
        if (verbose) listenerCallback?.Invoke("Requesting " + amount + " Kin from server", "log");
        //request payment from server
        string idHash = DeviceID();
        string reqUrl;
        WWWForm form = new WWWForm();
        form.AddField("address", PublicAddress());
        form.AddField("id", idHash);
        form.AddField("memo", memo);
        form.AddField("amount", amount.ToString());
        if (fund)
        {
            reqUrl = baseURL + fundURL; //this is the first time we are funding the account
        }
        else
        {
            reqUrl = baseURL + requestPaymentURL; //this is just a regular payment
        }

        var req = UnityWebRequest.Post(reqUrl, form);
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            LogError(req.downloadHandler.text);
            onComplete(false);
        }
        else
        {
            if (verbose) listenerCallback?.Invoke("Requesting complete", "log");

            onComplete(true);
        }
    }


    /// <summary>
    /// Send a payment from the user to the server (your server)
    /// </summary>
    /// <param name="amount">Amount of Kin to send</param>
    /// <param name="memo">A memo for your records</param>
    /// <param name="address">Address to send it to - leave blank to send to server</param>
    public void SendKin(decimal amount, string memo = "", string address = "")
    {
        if (verbose) listenerCallback?.Invoke("Sending " + amount + " Kin", "log");
        var amountInKin = amount;
        //https://github.com/kinecosystem/kin-sdk-unity#transactions
        if (address == "") address = serverKinAddress;
        //We first build the transaction
        kinAccount.BuildTransaction(address, amountInKin, fee, memo, BuildTransactionCallBack);
    }

    /// <summary>
    /// Request the server (your server) to send a payment to the user
    /// </summary>
    /// <param name="amount">Amount of Kin to request</param>
    /// <param name="memo">A memo the server will use, for your records</param>
     public void EarnKin(decimal amount, string memo){
        StartCoroutine(RequestPayment(amount,memo));
     }


    /// <summary>
    /// Delete the user's account
    /// </summary>
    public bool DeleteAccount()
    {
        //https://github.com/kinecosystem/kin-sdk-unity#creating-and-retrieving-a-kin-account
        kinClient.DeleteAccount();
        //Also delete our local caches
        PlayerPrefs.SetInt("UserAccountOnboarded", 0);
        PlayerPrefs.SetFloat("KinBalanceUser", 0f);
        isInitialized = false;
        return (true);
    }
    #endregion


    #region transactions
    void BuildTransactionCallBack(KinException ex, Transaction transaction)
    {
        //After building, we can store the transaction id to monitor the status in case of downtime (not implemented in this wrapper)
        if (ex == null)
        {
            if (verbose) listenerCallback?.Invoke("Sending transaction", "log");
            //We now send a transaction and fees are charged
            kinAccount.SendTransaction(transaction, SendTransactionCallback);

            //OPTIONAL: Your server account can be authorized by the Kin Foundation to whitelist transactions for zero fees
            //Once authorized, disable the above, and enable below to send whitelisted transactions
            //https://github.com/kinecosystem/kin-sdk-unity#transferring-kin-to-another-account-using-whitelist-service
            //StartCoroutine(WhitelistTransaction(transaction, WhitelistTransactionCallback));
        }
        else
        {
            LogError("Build Transaction Failed. " + ex);
        }
    }

    IEnumerator WhitelistTransaction(Transaction transaction, Action<string, string> onComplete)
    {
        var postDataObj = new WhitelistPostData(transaction);
       

        string reqUrl = baseURL + whitelistURL;

        WWWForm form = new WWWForm();
        form.AddField("envelope", postDataObj.envelope);
        form.AddField("networkId", postDataObj.network_id);
        form.AddField("network_id", postDataObj.network_id); //for forward compatibility


        var req = UnityWebRequest.Post(reqUrl, form);
        yield return req.SendWebRequest();

        if (req.isNetworkError || req.isHttpError)
        {
            LogError(req.error);
            onComplete?.Invoke(null, null);
        }
        else
        {
            onComplete(req.downloadHandler.text, transaction.Id);
        }
    }

    void WhitelistTransactionCallback(string whitelistTransaction, string transactionId)
    {
        //After whitelisting, we can now send the transaction
        if (whitelistTransaction != null)
        {
            //https://github.com/kinecosystem/kin-sdk-unity#transferring-kin-to-another-account-using-whitelist-service
            if (verbose) listenerCallback?.Invoke("Transaction whitelisted - sending payment", "log");
            if (verbose) listenerCallback?.Invoke(whitelistTransaction, "log");
            if (verbose) listenerCallback?.Invoke(transactionId, "log");
            kinAccount.SendWhitelistTransaction(transactionId, whitelistTransaction, SendTransactionCallback);
        }
        else
        {
            LogError("Whitelisting Transaction Failed. ");
        }
    }

    void SendTransactionCallback(KinException ex, String transactionId)
    {
        if (ex == null)
        {
            //Success
        }
        else
        {
            LogError("Send Transaction Failed. " + ex);

        }
    }
    #endregion

    #region Accounts
    KinAccount FetchAccount()
    {
        //https://github.com/kinecosystem/kin-sdk-unity#creating-and-retrieving-a-kin-account
        KinAccount ka = null;
        try
        {
            if (!kinClient.HasAccount()) //Check to see if user has a local account created
            {
                //If not, create one locally and return its reference
                ka = kinClient.AddAccount();
            }
            else
            {
                //If already created, just return its reference
                ka = kinClient.GetAccount(0);
            }
        }

        catch (KinException e)
        {
            LogError("Error fetching account: " + e);
        }
        return (ka);
    }

    void GetStatusCallback(KinException ex, AccountStatus status)
    {
        //Called back returning the status of the account
        if (ex == null)
        {
            //If account is already onboarded (registered on the blockchain) it comes back as created
            if (status == AccountStatus.Created)
            {
                if (verbose) listenerCallback?.Invoke("Account created", "log");
                PlayerPrefs.SetInt("UserAccountOnboarded", 1); //save this so we don't have to check next time
                InitializeKin();// continue with initialization of the wrapper (next step)
            }
            else
            {
                //The account was not yet onboarded, so we onboard it
                //Actually, to register it on the blockchain, we just need to fund it with some minimal Kin
                if (verbose) listenerCallback?.Invoke("Account doesn't exist", "log");
                if (verbose) listenerCallback?.Invoke("Creating account (funding)", "log");
                StartCoroutine(RequestPayment(10.00M, "Inital funding", FundAccountCallback, true));
            }
        }
        else
        {
            LogError("Get Account Status Failed. " + ex);
        }
    }

    void FundAccountCallback(bool success)
    {
        if (success)
        {
            if (verbose) listenerCallback?.Invoke("Account funded", "log");
            PlayerPrefs.SetInt("UserAccountOnboarded", 1);// mark as onboarded so we don't have to check again
            InitializeKin();// continue with initialization of the wrapper (next step)
        }
        else
        {
            LogError("Could not register user account");
            StartCoroutine(WaitAndInitialize());
        }
    }



    void GetBalanceCallback(KinException ex, decimal balance)
    {
        if (ex == null)
        {
            if (verbose) listenerCallback?.Invoke("Account balance fetched", "log");
            PlayerPrefs.SetFloat("KinBalanceUser", (float)(balance)); //save this so we can access it instantaneously
            fetchedUserBalance = true;
            InitializeKin();// continue with initialization of the wrapper (next step)
        }
        else
        {
            LogError("Get Balance Failed. " + ex);

            StartCoroutine(WaitAndInitialize());
        }
    }

    #endregion

    #region Blockchain Listeners
    public void AddListeners()
    {
        ////https://github.com/kinecosystem/kin-sdk-unity#listening-to-balance-changes
        kinAccount.AddPaymentListener(this);
        kinAccount.AddBalanceListener(this);
        //NOTE: you can also listen for account creation: https://github.com/kinecosystem/kin-sdk-unity#listening-to-account-creation
        listenersActive = true;
    }

    public void OnEvent(PaymentInfo data)
    {
        if (verbose) listenerCallback?.Invoke("Payment event detected", "log");
        listenerCallback?.Invoke(data, "payment");
    }

    public void OnEvent(decimal balance)
    {
        if (verbose) listenerCallback?.Invoke("Balance event detected", "log");
        PlayerPrefs.SetFloat("KinBalanceUser", (float)(balance)); //save this so we can access it instantaneously
        listenerCallback?.Invoke(balance, "balance");
    }
    #endregion



    #region helperFunctions
    //functions below are just helpers, and you can modify them to suite your use (such as retries or LogError)
    IEnumerator WaitAndInitialize()
    {
        if (verbose) listenerCallback?.Invoke("Retrying", "log");
        yield return new WaitForSeconds(secondsBetweenRetry);
        initializationRetries++;
        InitializeKin(); // continue with initialization of the wrapper (next step)
    }

    void MaintainOneInstance()
    {
        if (kinWrapper == null)
        {
            kinWrapper = this;
            DontDestroyOnLoad(this);//prevent object destruction across scenes so we don't have to re-initialize and make the user wait
        }
        else
        {
            Destroy(gameObject); //only allow one instance of this object
        }
    }


    IEnumerator WaitForNet(Action OnConnected)
    {
        yield return new WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);
        OnConnected.Invoke();
    }

    class WhitelistPostData
    {
        public string envelope;
        public string network_id;  //for forward compatibility
        public string networkId;

        public WhitelistPostData(Transaction transaction)
        {
            envelope = transaction.WhitelistableTransactionPayLoad;
            network_id = transaction.WhitelistableTransactionNetworkPassphrase;
            networkId = transaction.WhitelistableTransactionNetworkPassphrase;
        }
    }

    void LogError(string ex)
    {
        listenerCallback?.Invoke(ex, "error");
    }

    string DeviceID()
    {
        string id = SystemInfo.deviceUniqueIdentifier;
        string idHash = String.Format("{0:X}", id.GetHashCode()); //shorten to fit in stellar memo
        return (idHash);
    }

    #endregion
}