using NetworkMessages;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

namespace NetworkMessages
{
    public enum Commands{
        PLAYER_UPDATE,
        SERVER_UPDATE,
        HANDSHAKE,
        PLAYER_INPUT
    }

    [System.Serializable]
    public class NetworkHeader{
        public Commands cmd;
    }

    [System.Serializable]
    public class HandshakeMsg:NetworkHeader{
        public NetworkObjects.NetworkPlayer player;
        public HandshakeMsg(){      // Constructor
            cmd = Commands.HANDSHAKE;
            player = new NetworkObjects.NetworkPlayer();
        }
    }
    
    [System.Serializable]
    public class PlayerUpdateMsg:NetworkHeader{
        public NetworkObjects.NetworkPlayer player;
        public PlayerUpdateMsg(){      // Constructor
            cmd = Commands.PLAYER_UPDATE;
            player = new NetworkObjects.NetworkPlayer();
            Debug.Log(player.cubPos);   //Should update the players position based off the cube
        }
    };

    public class PlayerInputMsg:NetworkHeader{
        public Input myInput;
        public PlayerInputMsg(){
            cmd = Commands.PLAYER_INPUT;
            myInput = new Input();
        }
    }
    [System.Serializable]
    public class  ServerUpdateMsg:NetworkHeader{
        public List<NetworkObjects.NetworkPlayer> players;
        public ServerUpdateMsg(){      // Constructor
            cmd = Commands.SERVER_UPDATE;
            players = new List<NetworkObjects.NetworkPlayer>();
        }

        //public static void PlayerPosition(NetworkObjects.NetworkPlayer player)
        //{
          
        //}
    }
}



namespace NetworkObjects
{
    [System.Serializable]
    public class NetworkObject{
        public string id;
    }
    [System.Serializable]
    public class NetworkPlayer : NetworkObject{
        public Color cubeColor;
        public Vector3 cubPos;
        public Quaternion rotation;

        float moveSpeed = 5f;
        bool[] inputs;
        

        public NetworkPlayer(){
            cubeColor = new Color();
            cubPos = new Vector3();
            rotation = new Quaternion();
        }

        public void Update()
        {
            Vector2 _inputDirection = Vector2.zero;

            if(inputs[0])
            {
                _inputDirection.y += 1;
            }
            if (inputs[1])
            {
                _inputDirection.y += 1;
            }
            if (inputs[2])
            {
                _inputDirection.x += 1;
            }
            if (inputs[3])
            {
                _inputDirection.x += 1;
            }

            Move(_inputDirection);
        }

        private void Move(Vector2 _inputDirection)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            Vector3 moveDirection = right * _inputDirection.x + forward * _inputDirection.y;
            cubPos += moveDirection * moveSpeed;

           // ServerUpdateMsg.PlayerPosition(this);
           // ServerUpdateMsg.PlayerRotation(this);
           
        }

        public void SetInput(bool[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}
