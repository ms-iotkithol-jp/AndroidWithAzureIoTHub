# The sample of Android connected to Azure IoT Hub via [Azure IoT SDK](https://github.com/azure/azure-iot-sdk-csharp/) 
1. Open this solution by Visual Studio. 
2. Set Azure IoT Hub Name and Device Id and Device Key of MainActivity.cs which is created by Azure IoT Hub. 
``` cs
        Microsoft.Azure.Devices.Client.DeviceClient iotHubClient;
        string connectionString = "HostName=[IoT Hub Name].azure-devices.net;DeviceId=[DeviceId];SharedAccessKey=[DeviceKey]";
        bool sendingAvailable = false;
```
3. Connect Android Device(ex. DragonBoard 410c) to Windows PC via USB cable. 
4. Select DragonBord 410c and Run app! 

---
## チュートリアル
このアプリを作成する手順を説明。ここでは、DragonBoard 410Cを使った手順を紹介します。 
必要機材は、
- [DragonBard 410c](http://sp.chip1stop.com/dragonboard410c/?cid=c1stop_bn_dragonboard410c) - この[手順](https://github.com/96boards/documentation/wiki/Dragonboard-410c-Installation-Guide-for-Linux-and-Android)で、Androidをインストール
- Visual Studio 2015以上が動く、Windows PC 
- 他に、DragonBoardとWindows PCをつなぐ、マイクロUSBケーブル、DragonBoard用電源が必要です。
- DragonBoardをインターネットに接続するために、WiFiを用意してください。

### 1. プロジェクト作成
Visual Studioを起動して、新しいプロジェクトを作成します。使うテンプレートは、Visual C#→Android→Blank App(Android) 
### 2. [Azure IoT SDK](http://github.com/Azure/azure-iot-sdk-csharp)を組み込む 
ソリューションエクスプローラーで、”参照”を右クリックし、NeGetパッケージの管理を選択します。 
参照タブを選択し、"Azure Devices Client”と検索窓に入力し、”Microsoft.Azure.Devices.Client.PCL”を選択して、”インストール”をクリックします。インストール等のダイアログを”OK”していけばインストール可能です。 
JSONを扱うことが多いので、Newtonsoft.Jsonもインストールしてください。 
### 3. Azure IoT Hubへの接続 
予め、[ドキュメント](https://docs.microsoft.com/ja-jp/azure/iot-hub/iot-hub-csharp-csharp-getstarted)に記載された方法で、Azure IoT Hubを接続し、DragonBoardの識別子、Device Idを任意で決めて、Azure IoT Hubへの登録と、Device Keyを取得しておいてください。 
Visual Studioで作成したプロジェクトの、MainActivity.csを開きます。 
MainActivityクラスのメソッド、変数として以下を追加します。 
``` cs
        Microsoft.Azure.Devices.Client.DeviceClient iotHubClient;
        string connectionString = "HostName=[IoT Hub Name].azure-devices.net;DeviceId=[DeviceId];SharedAccessKey=[DeviceKey]";
        bool sendingAvailable = false;
        private async void InitializeIoTHub()
        {
            try
            {
                iotHubClient = Microsoft.Azure.Devices.Client.DeviceClient.CreateFromConnectionString(connectionString, Microsoft.Azure.Devices.Client.TransportType.Http1);
                await iotHubClient.OpenAsync();

                string content = "{\"message\":\"Hello from Dragon Board!\",\"Devicetime\":\"" + DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss") + "\"}";
                await iotHubClient.SendEventAsync(new Microsoft.Azure.Devices.Client.Message(System.Text.Encoding.UTF8.GetBytes(content)));
                sendingAvailable = true;
                ReceiveMessages();
            }
            catch(Exception ex)
            {
                Log.Debug(TAG, "IoT Hub Error +" + ex.Message);
            }
        }

        public async Task ReceiveMessages()
        {
            while (true)
            {
                var msg = await iotHubClient.ReceiveAsync();
                if (msg != null)
                {
                    string content = System.Text.Encoding.UTF8.GetString(msg.GetBytes());
                    Log.Debug(TAG, "Message Recievice - " + content);
                    await iotHubClient.CompleteAsync(msg);
                }
            }
        }
```
後は、[IoT Hub Name]、[DeviceId]、[DeviceKey]を、クラウド上のAzure IoT Hubの設定に合わせて変えて、OnCreateメソッド内で、InitializeIoTHubメソッドをコールする一文を加えます。 
これで基本は完成です。 クラウド側からAzure IoT Hubを通じて、コマンド列を送ることも可能です。 
後は、ボタンなりなんなり加えて、位置情報とか、IoTっぽい情報を送ってあげれば、完成です。


