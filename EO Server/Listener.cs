using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EO_Server
{
    /*
    public class StateObject
    {
        public NetworkClient client;
        public Socket socket;
        public const int BufferSize = 512;
        public byte[] buffer;
        public int packetsRead;

        public StateObject()
        {
            packetsRead = 0;
        }
        //public PacketReader reader;
    }
    */

    public class Listener
    {
        private Socket listenerSocket;
        private int port;
        public bool listening;

        public Dictionary<ulong, NetworkClient> connectedClients;
        public Queue<NetworkClient> _remove_clients; //List of clients to remove on next Update
        private ulong availableClientId;

        public Listener(int port)
        {
            this.port = port;
            availableClientId = 0;
            connectedClients = new Dictionary<ulong, NetworkClient>();
            _remove_clients = new Queue<NetworkClient>();

            try
            {
                listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
        }


        public NetworkClient AddClient(Socket socket)
        {
            ulong clientId = availableClientId;
            NetworkClient client = new NetworkClient(clientId, socket);
            connectedClients.Add(clientId, client);
            availableClientId++;
            
            return client;
        }

        //TODO: Remove from connectedClients List
        private void RemoveClient(ulong clientId)
        {
            try
            {
                if(connectedClients.TryGetValue(clientId, out NetworkClient client))
                {
                    RemoveClient(client);
                }
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private void RemoveClient(NetworkClient client)
        {
            try
            {
                client.connected = false;
                //not thread safe
                client.socket.Shutdown(SocketShutdown.Both);
                client.socket.Close();

                if (client.character != null)
                    client.OnDisconnect();
                
                connectedClients.Remove(client.clientId);
                Console.WriteLine($"Disconnected client {client.clientId}");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public void StartListening()
        {
            try
            {
                listenerSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                listenerSocket.Listen(10);
                listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                listening = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               // throw e;
            }
        }

        public void Update()
        {
            //Disconnect any clients
            while(_remove_clients.Count > 0)
            {
                RemoveClient(_remove_clients.Dequeue());
            }

            foreach(var entry in connectedClients)
            {
                NetworkClient client = entry.Value;

                if(client.incomingPackets.Count > 0)
                    client.HandlePacket(client.incomingPackets.Dequeue());

                //InputManager
                if(client.character != null && client.character.map != null)
                {
                    if((Server.GetCurrentTime() - client.inputManager.lastRecInput) >= 500)
                    {
                        client.inputManager.ClearInput();
                    }
                }
                
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                Socket clientSocket = listenerSocket.EndAccept(ar);
                
                //clientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, 1);
                clientSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1000);

                
                NetworkClient client = AddClient(clientSocket);

                Receive(client);
                listenerSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

                Console.WriteLine($"Accepted client {client.clientId} at {clientSocket.RemoteEndPoint}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //throw e;
            }
        }

        private void Receive(NetworkClient client)
        {
            try
            {
                /*
                client.socket.BeginReceive(client.buffer, 0, client.buffer.Length, SocketFlags.None,
                        new AsyncCallback(ReceiveCallback), client);
                */
                client.socket.BeginReceive(client.receiveBuffer, client.receiveOffset, (client.reader.messageSize - client.receiveOffset), 
                    SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
            }
            catch(SocketException e)
            {
                Console.WriteLine("[Receive]" + e.ToString());
                Console.WriteLine("Client id: " + client.clientId.ToString());

                _remove_clients.Enqueue(client);
            }
            catch(Exception e)
            {
                Console.WriteLine("[Receive]" + e.ToString());
                Console.WriteLine("Client id: " + client.clientId.ToString());
                //throw e;
                _remove_clients.Enqueue(client);
            }
        }

        public void SendPacket(NetworkClient client, Packet packet)
        {
            PacketWriter builder = new PacketWriter();
            builder.WriteJSONPacket(packet);
            SendBytes(client, builder);
            
        }

        public void SendClients(IEnumerable<NetworkClient> clients, Packet packet)
        {
            
            PacketWriter builder = new PacketWriter();
            builder.WriteJSONPacket(packet);

            foreach (NetworkClient client in clients)
            {
                if (client.connected)
                {
                    SendBytes(client, builder);
                }
            }
            

        }

        public void SendBytes(NetworkClient client, PacketWriter builder)
        {
            try
            {
                client.socket.BeginSend(builder.buffer, 0, builder.offset, SocketFlags.None,
                    new AsyncCallback(SendCallback), client);
            }
            catch (SocketException e)
            {
                Console.WriteLine("[Send]" + e.ToString());
                Console.WriteLine("Client id: " + client.clientId.ToString());

                _remove_clients.Enqueue(client);
            }
            catch (Exception e)
            {
                Console.WriteLine("[Send]" + e.ToString());
                Console.WriteLine("Client id: " + client.clientId.ToString());
                // throw e;
                _remove_clients.Enqueue(client);
            }
        }

       

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                NetworkClient client = (NetworkClient)ar.AsyncState;
                int bytesSent = client.socket.EndSend(ar);
                //Console.WriteLine($"Sent {bytesSent} bytes to client {state.client.clientId}");
            }
            catch (Exception e)
            {
                Console.WriteLine("[SendCallback]" + e.ToString());
                //throw e;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                NetworkClient client = (NetworkClient) ar.AsyncState;
                //Console.WriteLine($"Client {client.clientId} socket connected: {client.socket.Connected}");
                PacketReader reader = client.reader;
                int bytesRead = client.socket.EndReceive(ar);

                if(bytesRead > 0)
                {
                    //Console.WriteLine($"Received data from client {state.client.clientId}");

                    if((reader.messageSize - (client.receiveOffset + bytesRead)) == 0)
                    {
                        if(reader.packetLength == -1)
                        {
                            reader.ReadPacketLength();
                            client.receiveOffset += bytesRead;

                            //Console.WriteLine($"Packet length of {reader.packetLength}");
                        }
                        else
                        {
                            //TODO: Unmark when needed
                            
                            if(reader.ReadJSONPacket())
                            {
                                //Console.WriteLine($"Constructed packet {reader.packet}");
                                client.packetsRead++;

                                if(reader.packetType == (int) PacketType.SET_NET_TIME)
                                {
                                    SetNetworkTime cp = (SetNetworkTime) reader.packet;
                                    cp.netTime = Server.GetCurrentTime();
                                    SendPacket(client, cp);

                                    Console.WriteLine($"Syncing network time with client {client.clientId}");
                                }
                                else
                                {
                                    client.incomingPackets.Enqueue(reader.packet);
                                }
                                //Console.WriteLine($"Read {client.packetsRead} packets from client");
                               
                            }
                            else
                            {
                                Console.WriteLine($"Failed to construct packet, error:{reader.error}");
                            }
                            
                            
                            //TODO: Finish packet reading
                            client.reader = new PacketReader(client.receiveBuffer);
                            client.receiveOffset = 0;
                        }
                       
                    }
                    else
                    {
                        client.receiveOffset += bytesRead;
                    }

                    Receive(client);
                }
                else
                {
                    //RemoveClient(client.clientId);
                    _remove_clients.Enqueue(client);
                }

                //state.reader = new PacketReader();

                
            }
            catch (Exception e)
            {
                Console.WriteLine("[ReceiveCallback]" + e.ToString());
                //throw e;
            }
        }

        public void Disconnect()
        {
            if(listenerSocket.Connected)
            {
                listenerSocket.Shutdown(SocketShutdown.Both);

            }
            listenerSocket.Close();
        }

        public void OnProgExit()
        {
            foreach(var client in connectedClients.Values)
            {
                client.OnProgExit();
            }
        }
        
    }
}
