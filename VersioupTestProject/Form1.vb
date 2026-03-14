Imports System.Net.Http
Imports System.Text.Json
Imports AutoUpdaterDotNET

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Text = "今のバージョンは" & Application.ProductVersion & "です。"

        ' 1. イベントハンドラを登録（XMLの代わりにここが呼ばれる）
        AddHandler AutoUpdater.ParseUpdateInfoEvent, AddressOf GitHubApiParser

        ' 2. 開始（URLは何でも良いが、一応ダミーを入れておく）
        AutoUpdater.Start("https://api.github.com/repos/duron850mhz/VersioupTestProject/releases/latest")
    End Sub

    ''' <summary>
    ''' GitHub APIから最新リリース情報を取得して、AutoUpdater.NETのUpdateInfoEventArgsにセットする
    ''' </summary>
    ''' <param name="args"></param>
    Private Sub GitHubApiParser(args As ParseUpdateInfoEventArgs)
        Using client As New HttpClient()
            client.DefaultRequestHeaders.Add("User-Agent", "MyVbApp")

            Try
                ' URLを直接指定
                Dim url As String = "https://api.github.com/repos/duron850mhz/VersioupTestProject/releases/latest"
                Dim jsonString As String = client.GetStringAsync(url).GetAwaiter().GetResult()

                Dim json As JsonDocument = JsonDocument.Parse(jsonString)
                Dim latestRelease = json.RootElement

                ' 1番目のファイル(asset)を取得
                Dim firstAsset = latestRelease.GetProperty("assets")(0)

                ' --- 修正ポイント：VB.NETの正しい初期化子の書き方 ---
                Dim info As New UpdateInfoEventArgs()
                info.CurrentVersion = latestRelease.GetProperty("tag_name").GetString().Replace("v", "")
                info.ChangelogURL = latestRelease.GetProperty("html_url").GetString()
                info.DownloadURL = firstAsset.GetProperty("browser_download_url").GetString()

                ' 最後にargsにセット
                args.UpdateInfo = info
                ' --------------------------------------------------

            Catch ex As Exception
                ' 必要に応じてメッセージボックス等でエラーを表示
                MessageBox.Show(ex.Message)
            End Try
        End Using
    End Sub
End Class
