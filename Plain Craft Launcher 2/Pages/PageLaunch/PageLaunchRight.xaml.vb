Imports PCL.Core.Utils.Exts
Imports PCL.Core.App
Public Class PageLaunchRight

    Private Sub Init() Handles Me.Loaded
        PanBack.ScrollToHome()
        PanScroll = PanBack '不知道为啥不能在 XAML 设置
        PanLog.Visibility = If(ModeDebug, Visibility.Visible, Visibility.Collapsed)
        '社区版提示
        PanHint.Visibility = If(Setup.Get("UiLauncherCEHint"), Visibility.Visible, Visibility.Collapsed)
        LabHint1.Text = $"你正在使用 PCL 社区版的二次更改版本！此版本为独立开发和维护，与官方版本维护路线不同，体验有所出入。{vbCrLf}{vbCrLf}如果你是意外下载到了二次修改版本，我们十分建议您下载 PCL 官方版长期使用，此发行版本对新手用户体验可能不友好。{vbCrLf}此外，这个版本由个人偏好进行了魔改，如果遇到问题，不要向官方仓库反馈社区版的问题哦！{vbCrLf}"
        
    End Sub

    '关闭社区版提示
    Private Sub BtnHintClose_Click(sender As Object, e As EventArgs) Handles BtnHintClose.Click
        AniDispose(PanHint, True)
        States.Hint.CEMessage = False
End Sub

    ''' <summary>
    ''' 获取随机提示文本。
    ''' </summary>
    Public Shared Function GetRandomHint(Optional enableLengthLimit As Boolean = False, Optional raw As Boolean = False) As String
        Dim lines As String() = Nothing

        '外部文件
        Dim externalPath = ExePath & "PCL\hints.txt"
        If File.Exists(externalPath) Then
            Try
                lines = File.ReadAllLines(externalPath).Where(Function(l) Not String.IsNullOrWhiteSpace(l)).Select(Function(l) l.Trim()).ToArray()
            Catch
                Log($"[Page] 读取外部文件失败：{externalPath}", LogLevel.Hint)
            End Try
        End If

        '嵌入式资源
        If lines Is Nothing OrElse lines.Length = 0 Then
            Using reader As New StreamReader(Application.GetResourceStream(New Uri("pack://application:,,,/Plain Craft Launcher 2;component/Resources/hints.txt", UriKind.Absolute)).Stream)
                lines = reader.ReadToEnd().Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries).Where(Function(l) Not String.IsNullOrWhiteSpace(l)).Select(Function(l) l.Trim()).ToArray()
            End Using
        End If

        '长度限制
        If enableLengthLimit Then
            Dim shortLines = lines.Where(Function(l) l.Length < 50).ToArray()
            If shortLines.Length > 0 Then lines = shortLines
        End If

        '随机返回
        Dim hint = lines(Random.Shared.Next(lines.Length))
        Return If(raw, hint, hint.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;"))
    End Function


End Class
