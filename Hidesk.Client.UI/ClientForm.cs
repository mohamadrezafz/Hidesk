﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hidesk.Client.UI
{
    public partial class ClientForm : Form
    {
        private Hidesk.Client.Client client;
        private const string CommandLeftMouseUP = "LFU";
        private const string CommandLeftMouseDOWN = "LFD";
        private const string CommandRightMouseUP = "RFU";
        private const string CommandRightMouseDOWN = "RFD";
        private const string CommandMiddleMouseUP = "MFU";
        private const string CommandMiddleMouseDOWN = "MFD";
        public ClientForm()
        {
            client = new Client();

            InitializeComponent();
            RegisterEvents();



        }

        private void RegisterEvents()
        {
            //this
            btnConnect.Click += btnConnect_Click;
            btnLogin.Click += btnLogin_Click;
            btnStartSession.Click += btnStartSession_Click;
            btnRemoteDesktop.Click += btnRemoteDesktop_Click;
            btnSendMessage.Click += btnSendMessage_Click;
            btnUploadFile.Click += btnUploadFile_Click;
            btnDisconnect.Click += btnDisconnect_Click;
            btnCalc.Click += btnCalc_Click;
            btnEndSession.Click += btnEndSession_Click;
            this.FormClosing += ClientForm_FormClosing;

            //client
            client.SessionRequest += client_SessionRequest;
            client.TextMessageReceived += client_TextMessageReceived;
            client.FileUploadRequest += client_FileUploadRequest;
            client.FileUploadProgress += client_FileUploadProgress;
            client.ClientDisconnected += client_ClientDisconnected;
            client.SessionClientDisconnected += client_SessionClientDisconnected;
            client.GenericRequestReceived += client_GenericRequestReceived;
            client.SessionEndedByTheRemoteClient += client_SessionEndedByTheRemoteClient;
        }

        void btnEndSession_Click(object sender, EventArgs e)
        {
            client.EndCurrentSession((senderClient, response) =>
            {

                InvokeUI(() =>
                {
                    if (!response.HasError)
                    {
                        btnEndSession.Enabled = false;
                        btnStartSession.Enabled = true;
                        btnCalc.Enabled = false;
                        btnRemoteDesktop.Enabled = false;
                        btnSendMessage.Enabled = false;
                        btnUploadFile.Enabled = false;
                    }
                    else
                    {
                        Status(response.Exception.ToString());
                    }
                });
            });
        }

        void client_SessionEndedByTheRemoteClient(Client client)
        {
            InvokeUI(() =>
            {
                MessageBox.Show(this, "Sesion ended by the remote client.", this.Text);
                btnEndSession.Enabled = false;
                btnStartSession.Enabled = true;
                btnCalc.Enabled = false;
                btnRemoteDesktop.Enabled = false;
                btnSendMessage.Enabled = false;
                btnUploadFile.Enabled = false;
            });
        }

        void client_GenericRequestReceived(Client client, Shared.Messages.GenericRequest msg)
        {
            if (msg.GetType() == typeof(MessagesExtensions.CalcMessageRequest))
            {
                MessagesExtensions.CalcMessageRequest request = msg as MessagesExtensions.CalcMessageRequest;

                MessagesExtensions.CalcMessageResponse response = new MessagesExtensions.CalcMessageResponse(request);
                response.Result = request.A + request.B;
                client.SendGenericResponse(response);
            }
        }

        private void btnCalc_Click(object sender, EventArgs e)
        {
            MessagesExtensions.CalcMessageRequest request = new MessagesExtensions.CalcMessageRequest();
            request.A = 10;
            request.B = 5;

            client.SendGenericRequest<MessagesExtensions.CalcMessageResponseDelegate>(request, (clientSender, response) =>
            {

                InvokeUI(() =>
                {

                    MessageBox.Show(this, response.Result.ToString(), this.Text);

                });

            });
        }

        void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.Status == Shared.Enums.StatusEnum.Connected)
            {
                client.Disconnect();
            }
        }

        void client_SessionClientDisconnected(Client obj)
        {
            InvokeUI(() =>
            {
                btnSendMessage.Enabled = false;
                btnEndSession.Enabled = false;
                btnRemoteDesktop.Enabled = false;
                btnCalc.Enabled = false;
                btnUploadFile.Enabled = false;
            });

            Status("The remote session client was disconnected!");
        }

        void client_ClientDisconnected(Client obj)
        {
            InvokeUI(() =>
            {
                btnDisconnect.Enabled = false;
                btnLogin.Enabled = false;
                btnSendMessage.Enabled = false;
                btnRemoteDesktop.Enabled = false;
                btnCalc.Enabled = false;
                btnStartSession.Enabled = false;
                btnUploadFile.Enabled = false;
                btnConnect.Enabled = true;
                btnEndSession.Enabled = false;
            });

            Status("The client was disconnected!");
        }

        void btnDisconnect_Click(object sender, EventArgs e)
        {
            client.Disconnect();
        }

        void client_FileUploadProgress(Client client, EventArguments.FileUploadProgressEventArguments args)
        {
            Status("Downloading " + args.FileName + " To " + args.DestinationPath + ", " + ((args.CurrentPosition * 100) / args.TotalBytes) + "%...");

            if (args.CurrentPosition >= args.TotalBytes)
            {
                Status("Downloading " + args.FileName + " Completed!");
            }
        }

        void client_FileUploadRequest(Client client, EventArguments.FileUploadRequestEventArguments args)
        {
            InvokeUI(() =>
            {
                if (MessageBox.Show(this, "File upload request, " + args.Request.FileName + ", " + args.Request.TotalBytes.ToString() + ". Confirm?", this.Text, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.Title = this.Text + " Save files as";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        args.Confirm(dlg.FileName);
                    }
                    else
                    {
                        args.Refuse();
                    }
                }
                else
                {
                    args.Refuse();
                }
            });
        }

        void btnUploadFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = this.Text + " select file to upload";
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            client.UploadFile(dlg.FileName, (clientSender, response) =>
            {
                Status("Uploading " + response.FileName + ", " + ((response.CurrentPosition * 100) / response.TotalBytes) + "%...");

                if (response.CurrentPosition >= response.TotalBytes)
                {
                    Status("Uploading " + response.FileName + " Completed!");
                }
            });
        }

        void client_TextMessageReceived(Client sender, string message)
        {
            //if (message == CommandLeftMouseDOWN)
            //{
            //    VirtualMouse.LeftClick();
            //}
            //if (message == CommandRightMouseDOWN)
            //{
            //    VirtualMouse.RightClick();

            //}

            if (message == CommandLeftMouseDOWN || message == CommandRightMouseDOWN || message == CommandLeftMouseUP || message == CommandRightMouseUP)
            {
                InputMouseClicked(message);
            }
            //if (message.Contains(":"))
            //{
            //    var coordinate = message.Split(':');

            //    VirtualMouse.Move(Convert.ToInt32(coordinate[0]), Convert.ToInt32(coordinate[1]));
            //}


            Status("Message received: " + message);
        }

        void btnSendMessage_Click(object sender, EventArgs e)
        {
            DialogInput dlg = new DialogInput("Enter text message:");
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            client.SendTextMessage(dlg.Result);
        }
        // bool remoteDesktop = false;
        void btnRemoteDesktop_Click(object sender, EventArgs e)
        {
            try
            {
                client.RequestDesktop((clientSender, response) =>
                {
                    panelPreview.BackgroundImage = new Bitmap(response.FrameBytes);
                    response.FrameBytes.Dispose();
                });
                //remoteDesktop = true;
                this.panelPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelPreview_MouseDown);

                this.panelPreview.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelPreview_MouseUp);
              //  this.panelPreview.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelPreview_MouseMove);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        void client_SessionRequest(Client client, EventArguments.SessionRequestEventArguments args)
        {
            this.InvokeUI(() =>
            {

                if (MessageBox.Show(this, "Session request from " + args.Request.Email + ". Confirm request?", this.Text, MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    args.Confirm();
                    Status("Session started with " + args.Request.Email);

                    InvokeUI(() =>
                    {
                        btnSendMessage.Enabled = true;
                        btnRemoteDesktop.Enabled = true;
                        btnCalc.Enabled = true;
                        btnUploadFile.Enabled = true;
                        btnEndSession.Enabled = true;
                    });
                }
                else
                {
                    args.Refuse();
                }

            });
        }

        void btnStartSession_Click(object sender, EventArgs e)
        {
            DialogInput dlg = new DialogInput("Please enter target user name:");
            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            client.RequestSession(dlg.Result, (senderClient, args) =>
            {

                if (args.IsConfirmed)
                {
                    Status("Session started with " + dlg.Result);

                    InvokeUI(() =>
                    {
                        btnSendMessage.Enabled = true;
                        btnRemoteDesktop.Enabled = true;
                        btnCalc.Enabled = true;
                        btnUploadFile.Enabled = true;
                        btnEndSession.Enabled = true;
                    });
                }
                else
                {
                    Status(args.Exception.ToString());
                }

            });

        }

        void btnLogin_Click(object sender, EventArgs e)
        {
            //DialogInput dlg = new DialogInput("Please enter your user name:");
            //if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            Random random = new Random();
            var rnd = random.Next(1000, 10000);

            client.Login(/*dlg.Result*/ rnd.ToString(), (senderClient, args) =>
            {

                if (args.IsValid)
                {
                    Status("User Validated!");
                    this.InvokeUI(() =>
                    {
                        this.Text = "Client - " + rnd /*dlg.Result*/;
                        btnStartSession.Enabled = true;
                        btnLogin.Enabled = false;
                    });
                }

                if (args.HasError)
                {
                    Status(args.Exception.ToString());
                }

            });
        }

        void btnConnect_Click(object sender, EventArgs e)
        {
            client.Connect("45.9.255.116", 8888);
            btnLogin.Enabled = true;
            btnDisconnect.Enabled = true;
            btnConnect.Enabled = false;
        }

        private void Status(String str)
        {
            InvokeUI(() => { lbStatus.Text = str; });
        }

        private void InvokeUI(Action action)
        {
            this.Invoke(action);
        }

        //private void panelPreview_MouseClick(object sender, MouseEventArgs e)
        //{
        //    switch (e.Button)
        //    {

        //        case MouseButtons.Left:
        //            client.SendTextMessage(CommandLeftMouseDOWN);
        //            break;

        //        case MouseButtons.Right:
        //            client.SendTextMessage(CommandRightMouseDOWN);
        //            break;

        //    }

        //}

        private void ClientForm_Load(object sender, EventArgs e)
        {

            btnConnect.PerformClick();

            btnLogin.PerformClick();

        }
        //private void panelPreview_MouseMove(object sender, MouseEventArgs e)
        //{

        //    var aaa = e.Location.X + ":" + e.Location.Y;

        //    client.SendTextMessage(aaa);

        //}

        public static void InputMouseClicked(string mouse)
        {
            switch (mouse)
            {
                case CommandLeftMouseDOWN:
                    VirtualMouse.LeftDown();
                    break;

                case CommandRightMouseDOWN:
                    VirtualMouse.RightDown();
                    break;
                //case CommandMiddleMouseDOWN:
                //    VirtualMouse.do
                //    break;
                case CommandLeftMouseUP:
                    VirtualMouse.LeftUp();
                    break;
                case CommandRightMouseUP:
                    VirtualMouse.RightUp();
                    break;
                //  case CommandMiddleMouseUP:
                //  Mouse.mouse_event(Mouse.MOUSEEVENTF_MIDDLEUP, Cursor.Position.X, Cursor.Position.Y, 0, 0);
                //   break;

                default:
                    break;

            }
        }


        private void panelPreview_MouseUp(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                client.SendTextMessage(CommandLeftMouseUP);
            }
            else if (e.Button == MouseButtons.Right)
            {
                client.SendTextMessage(CommandRightMouseUP);
            }
            else
            {
                client.SendTextMessage(CommandMiddleMouseUP);
            }

        }

        private void panelPreview_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                client.SendTextMessage(CommandLeftMouseDOWN);
            }
            else if (e.Button == MouseButtons.Right)
            {
                client.SendTextMessage(CommandRightMouseDOWN);
            }
            else
            {
                client.SendTextMessage(CommandMiddleMouseDOWN);
            }
        }
        //Point startingPoint = new Point(0, 0);
        //Point endingPoint = new Point(0, 0);
        //Point deltaPoint = new Point(0, 0);
        //private void panelPreview_MouseMove(object sender, MouseEventArgs e)
        //{


        //    try
        //    {

        //        deltaPoint.X = endingPoint.X - startingPoint.X;                                         // how to handle delta properly. AKA how to structure
        //        deltaPoint.Y = endingPoint.Y - startingPoint.Y;

        //        startingPoint.X = Cursor.Position.X;
        //        startingPoint.Y = Cursor.Position.Y;


        //        client.SendTextMessage(Cursor.Position.X + ":" + Cursor.Position.Y);


        //        endingPoint.X = Cursor.Position.X;
        //        endingPoint.Y = Cursor.Position.Y;
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        //public static void SendTransmission()
        //{
        //    Point startingPoint = new Point(0, 0);
        //    Point endingPoint = new Point(0, 0);
        //    Point deltaPoint = new Point(0, 0);

        //    try
        //    {

        //        deltaPoint.X = endingPoint.X - startingPoint.X;                                         // how to handle delta properly. AKA how to structure
        //        deltaPoint.Y = endingPoint.Y - startingPoint.Y;

        //        startingPoint.X = Cursor.Position.X;
        //        startingPoint.Y = Cursor.Position.Y;

        //        binaryWriter.Write(CommandCursor);
        //        binaryWriter.Write(deltaPoint.X);
        //        binaryWriter.Write(deltaPoint.Y);
        //        binaryWriter.Flush();

        //        Thread.Sleep(30);
        //        endingPoint.X = Cursor.Position.X;
        //        endingPoint.Y = Cursor.Position.Y;
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}
    }
}
//5.135.16.88
//45.9.255.116
