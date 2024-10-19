using Mono.CSharp;
using QFSW.QC;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;



public class TestRelay : MonoBehaviour
{
    private string playerName;

    [SerializeField] TextMeshProUGUI playerNameUI;
    [SerializeField] TextMeshProUGUI gameModeUI;
    [SerializeField] TextMeshProUGUI playersInLobby;
    [SerializeField] TextMeshProUGUI HostInLobby;
    private async void Start()
    {
        await UnityServices.InitializeAsync(); //for connecting the APi to net

        AuthenticationService.Instance.SignedIn += Instance_SignedIn;

        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //for sign in without any google or firebase login ,, 
        playerName = "Bindo" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
        playerNameUI.text = "Player - " + playerName.ToString();
    }

    #region Relay

    [Command] //for code to excute in console
    private async void CreateServer()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); //creating the server
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode + "JoinCode");  //fetching relay code

            RelayServerData serverData = new RelayServerData(allocation, "dtls"); //new techque

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);  //newTechque

            NetworkManager.Singleton.StartServer();
            /*  NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(   //connecting networkmanager to relay  //oldTechque
                  allocation.RelayServer.IpV4 ,  //ipaddress of relay
                 (ushort)allocation.RelayServer.Port,  //port id form relay
                  allocation.AllocationIdBytes,   //Id in bytes 
                  allocation.Key,
                  allocation.ConnectionData);*/
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
        }
    }


    [Command]
    private async void JoinServer(string relayId)
    {
        try
        {
            Debug.Log("Relay joing" + relayId);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayId);  //joing relay with code 

            RelayServerData serverData = new RelayServerData(joinAllocation, "dtls"); //new techque

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);  //newTechque
            NetworkManager.Singleton.StartClient();

            /* NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(  //for connecting to relay  //old techque
                 joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.Key,
                joinAllocation.AllocationIdBytes,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                 );*/

        }
        catch (RelayServiceException ex)
        {
            Debug.Log(ex + "exception");
        }
    }
    #endregion
    private void Update()
    {
        HandleLobbyHeartBeat();
       HandleLobbyPollForUpdate();

        if (playerHasJoinLobby)
        {
            PrintPlayerInLobby(joinLobby);
        }
        else
        {
            return;
        }
    }

  

    Lobby hostLobby;
    [SerializeField] float lobbyHeartBeatTimer;
    [SerializeField] float lobbyupdateTimer;

    private async void HandleLobbyHeartBeat()
    {
        if (hostLobby != null)
        {
            lobbyHeartBeatTimer -= Time.deltaTime;
            if (lobbyHeartBeatTimer < 0f)
            {
                float resetTimer = 15;
                lobbyHeartBeatTimer = resetTimer;

               
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);

            }
        }
        else
        {
            // Debug.Log("lobbyNotFound");
        }
    }
    private async void HandleLobbyPollForUpdate()
    {
        if (joinLobby != null)
        {
            lobbyupdateTimer -= Time.deltaTime;
            if (lobbyupdateTimer < 0f)
            {
                float resetTimer = 1.1f;
                lobbyupdateTimer = resetTimer;

               Lobby lobby=  await LobbyService.Instance.GetLobbyAsync(joinLobby.Id);

                joinLobby = lobby;
            }
        }
        else
        {
            // Debug.Log("lobbyNotFound");
        }
    }
    [Command]
    private async void UpdatePlayerdata(string newPlayerName)
    {
        try
        {
           
            playerName = newPlayerName;
           // await LobbyService.Instance.UpdatePlayerAsync( newPlayerName);
            await LobbyService.Instance.UpdatePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>  //set player data by this
                {
                    { "PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName) }
                }
            });

         
        }catch(LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }



    /* [Command]
     private async void createLobby()
     {
         try
         {

             string lobbyname = "FirstLobby";
             int maxPlayer = 6;

             CreateLobbyOptions options = new CreateLobbyOptions
             {
                 IsPrivate = false,
                 Player = GetPlayer(),
                 Data = new Dictionary<string, DataObject>
                 {
                     {"GameMode" ,new DataObject(DataObject.VisibilityOptions.Public , "CaptureTheFlag" , DataObject.IndexOptions.S1)},  //creating the data object for lobby
                   *//*  {"GameMode" ,new DataObject(DataObject.VisibilityOptions.Public , "DeathMatch" , DataObject.IndexOptions.S2)},  //creating the data object for lobby*//*
                     {"Map" , new DataObject(DataObject.VisibilityOptions.Public , "Mars") }
                 }
             };
             Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxPlayer, options);

             hostLobby = lobby;
             joinLobby = hostLobby;

             gameModeUI.text = "GameMode - "  + lobby.Data["GameMode"].Value;

             Debug.Log("createdlobby" + "lobby name - " + lobby.Name + "lobbyplayer -" + lobby.MaxPlayers + "LobbyCode - " + lobby.LobbyCode);
             PrintPlayerData(hostLobby);
             LobbyHostPlayerName(hostLobby);
             // PrintPlayerInLobby(hostLobby);
         }
         catch (LobbyServiceException ex)
         {
             Debug.LogException(ex);
         }
     }*/

    [Command]
    private async void createLobby()
    {
        try
        {
            string lobbyName = "FirstLobby";
            int maxPlayer = 6;

            // Create Relay allocation for the lobby
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayer); //creating the server
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode + " JoinCode");  //fetching relay code

            // Create the lobby with Relay join code in the lobby data
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
            {
                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag", DataObject.IndexOptions.S1)},  //creating the data object for lobby
                {"Map", new DataObject(DataObject.VisibilityOptions.Public, "Mars") },
                {"RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode) } // Storing the Relay join code in lobby data
            }
            };

            // Create the lobby
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, options);
            hostLobby = lobby;
            joinLobby = hostLobby;

            // Set up Relay server data for the NetworkManager
            RelayServerData serverData = new RelayServerData(allocation, "dtls"); //new technique
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);  //new technique
            NetworkManager.Singleton.StartHost();

            // Update the UI
            gameModeUI.text = "GameMode - " + lobby.Data["GameMode"].Value;
            Debug.Log("created lobby" + "lobby name - " + lobby.Name + " max players -" + lobby.MaxPlayers + " LobbyCode - " + lobby.LobbyCode);
            PrintPlayerData(hostLobby);
            LobbyHostPlayerName(hostLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogException(ex);
        }
    }

    Lobby joinLobby;
    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {

                Count = 25,

                Filters = new List<QueryFilter> { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),

                    new QueryFilter(QueryFilter.FieldOptions.S1 , "CaptureTheFlag" , QueryFilter.OpOptions.EQ),
                  /*  new QueryFilter(QueryFilter.FieldOptions.S2 , "DeathMatch" , QueryFilter.OpOptions.EQ)*/
                    
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false , QueryOrder.FieldOptions.Created)

                }
            };

            QueryResponse query = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log("lobbies found" + query.Results.Count);

            foreach (Lobby lobby in query.Results)
            {
                Debug.Log(lobby.Name + lobby.MaxPlayers + lobby.Data["GameMode"].Value);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [Command]
    private async void JoinLobby()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();


            await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException e)
        {

            Debug.LogException(e);
        }
    }
    [Command]
    private async void KickPlayer()
    {
       try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, joinLobby.Players[1].Id);

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }

    }
    [Command]
    private void LobbyPlayerList()
    {
       
        PrintPlayerInLobby(joinLobby);
      
    }

    private void PrintPlayerInLobby(Lobby lobby)
    {
       // Debug.Log(" Players in  lobby " + lobby.Name + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
        {
          //  Debug.Log(player.Id + " " + player.Data["PlayerName"].Value + " Map " + lobby.Data["Map"].Value);
            playersInLobby.text = player.Data["PlayerName"].Value;
        }
    } 
    private void LobbyHostPlayerName(Lobby lobby)
    {
       // Debug.Log(" Players in  lobby " + lobby.Name + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
        {
          //  Debug.Log(player.Id + " " + player.Data["PlayerName"].Value + " Map " + lobby.Data["Map"].Value);
            HostInLobby.text = player.Data["PlayerName"].Value;
        }
    }

    //host id = host.player.id
    [Command]
    private async void MigratePlayer()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinLobby.Players[1].Id
            });

            joinLobby = hostLobby;  //changing the join lobby id 
            PrintPlayerData(hostLobby);
           

        }catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }

    }
    bool playerHasJoinLobby;
    /* [Command]
     private async void joinLobbyByCode(string joinCode)
     {
         try
         {
             QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

             JoinLobbyByCodeOptions join = new JoinLobbyByCodeOptions
             {
                 Player = GetPlayer(),

             };

             Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(joinCode, join);
             joinLobby = lobby;
             gameModeUI.text = "GameMode - " + lobby.Data["GameMode"].Value;
             Debug.Log($"JoinLobby by code {joinCode}");
             //for getting list of player in the lobby 
             PrintPlayerInLobby(joinLobby);
             LobbyHostPlayerName(hostLobby);
             NetworkManager.Singleton.StartClient();
             playerHasJoinLobby = true;

         }
         catch (LobbyServiceException e)
         {
             Debug.LogException(e);
         }
     }*/

    [Command]
    private async void joinLobbyByCode(string joinCode)
    {
        try
        {
            JoinLobbyByCodeOptions join = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(),
            };

            // Join the lobby using the provided join code
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(joinCode, join);
            joinLobby = lobby;

            // Update the game mode UI
            gameModeUI.text = "GameMode - " + lobby.Data["GameMode"].Value;
            Debug.Log($"Joined lobby by code {joinCode}");

            // Get the Relay join code from the lobby data
            string relayJoinCode = lobby.Data["RelayJoinCode"].Value;

            // Connect to Relay using the retrieved join code
            await JoinRelayServer(relayJoinCode);

            // Start the NetworkManager client
            NetworkManager.Singleton.StartClient();
            playerHasJoinLobby = true;

            // Update lobby player list UI
            PrintPlayerInLobby(joinLobby);
            LobbyHostPlayerName(joinLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async Task JoinRelayServer(string relayJoinCode)
    {
        try
        {
            // Join the Relay server using the provided join code
            Debug.Log("Joining Relay with code: " + relayJoinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            // Configure the NetworkManager with Relay server data
            RelayServerData serverData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(serverData);

            Debug.Log("Successfully connected to Relay");
        }
        catch (RelayServiceException ex)
        {
            Debug.LogException(ex);
        }
    }


    [Command]
    private async void LeaveLobby()
    {
        try
        {
            
            await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id , AuthenticationService.Instance.PlayerId);

        }catch (LobbyServiceException e)
        {
                Debug.LogException(e);
        }
    }

    [Command]
    private async void QuikJoin()
    {
        try
        {
            await Lobbies.Instance.QuickJoinLobbyAsync();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject> {
                 {
                     "PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)
                 }}
        };
    }



    /*private async void ListOfPlayerJoined()
    {
        QueryResponse queryResponse = await Lobbies.Instance.
    }*/

   
   
    private void PrintPlayerData(Lobby lobby)
    {
        Debug.Log(" Players in  lobby " + lobby.Name + lobby.Data["GameMode"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value + " Map " + lobby.Data["Map"].Value);
          
        }
    }
    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {

            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {"GameMode" , new DataObject(DataObject.VisibilityOptions.Public , gameMode ) }
            }
            });
            joinLobby = hostLobby;
            PrintPlayerData(hostLobby);

        }
        catch (LobbyServiceException e)
        {

            Debug.LogException(e);

        }
    }

    [Command]
    private void PrintPlayer()
    {
        PrintPlayerData(joinLobby);
    }

    [Command]
    private async void DeleteLobby()   
    {
        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(joinLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private void Instance_SignedIn()
    {
        Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
    }


}
