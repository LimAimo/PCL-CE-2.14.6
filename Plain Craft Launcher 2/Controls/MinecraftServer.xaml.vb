Imports PCL.Core.Minecraft
Imports PCL.Core.UI

Class MinecraftServer
    Inherits Grid
    
    Private Const FallbackImageUri As String = "pack://application:,,,/Plain Craft Launcher 2;component/Images/Icons/DefaultServer.png"

    Public Property Address As String
        Get
            Return GetValue(AddressProperty)
        End Get
        Set(value As String)
            SetValue(AddressProperty, value)
        End Set
    End Property
    Private Shared ReadOnly AddressProperty As DependencyProperty =
        DependencyProperty.Register(
            NameOf(Address),
            GetType(String),
            GetType(MinecraftServer),
            New PropertyMetadata(String.Empty, AddressOf OnAddressChanged)
        )

    Private Shared Sub OnAddressChanged(d As DependencyObject, e As DependencyPropertyChangedEventArgs)
        Dim server As MinecraftServer = d
        d.Dispatcher.BeginInvoke(Function() server.UpdateServerInfoAsync(e.NewValue?.ToString()))
    End Sub

    Public Async Function UpdateServerInfoAsync(address As String) As Task
        If address Is Nothing Then Return
        address = address.Replace("：", ":")
        ' 预先重置UI状态
        LabServerDesc.Foreground = Brushes.White
        LabServerDesc.Text = "查询中..."
        LabServerPlayer.Text = "-/-"
        LabServerPlayer.ToolTip = Nothing
        ImageLoaderHelper.SetFallbackImage(imgServerLogo, FallbackImageUri)

        Try
            ' 获取可达地址（DNS解析）
            Dim addr = Await ServerAddressResolver.GetReachableAddressAsync(address)

            ' 显示基本服务器信息（McPing 功能已移除）
            LabServerDesc.Text = $"Minecraft 服务器 ({addr.Ip}:{addr.Port})"
            LabServerPlayer.Text = "未知"
            ImageLoaderHelper.SetFallbackImage(imgServerLogo, FallbackImageUri)
        Catch ex As Exception
            Log(ex, "[MinecraftServer] 信息查询失败")
            LabServerDesc.Text = $"无法连接: {ex.Message}"
            LabServerDesc.Foreground = Brushes.Red
            ImageLoaderHelper.SetFallbackImage(ImgServerLogo, FallbackImageUri)
        End Try
    End Function
End Class