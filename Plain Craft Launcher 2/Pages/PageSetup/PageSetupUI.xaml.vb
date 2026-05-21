Imports PCL.Core.App
Imports PCL.Core.UI
Imports PCL.Core.Utils
Imports PCL.Core.Utils.OS

Public Class PageSetupUI

    Public Shadows IsLoaded As Boolean = False

    Public ReadOnly Property ThemeColors As String() =
        If(Basics.IsAprilFool, {"天空蓝", "龙猫蓝", "死机蓝", "HMCL"}, {"天空蓝", "龙猫蓝", "死机蓝"})

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub PageSetupUI_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        '重复加载部分
        PanBack.ScrollToHome()
        ThemeCheckAll(True)

        If ThemeDontClick <> 0 Then
            Dim NewText As String
            Select Case ThemeDontClick
                Case 1
                    NewText = "眼瞎白"
                Case 2
                    NewText = "真·滑稽彩"
                Case Else
                    NewText = "？？？"
            End Select
            'PanLauncherTheme 已移除
        End If

#If DEBUG Then
        'If EnableCustomTheme Then
        '    LabLauncherDelta.Visibility = Visibility.Visible
        '    SliderLauncherDelta.Visibility = Visibility.Visible
        '    LabLauncherLight.Visibility = Visibility.Visible
        '    SliderLauncherLight.Visibility = Visibility.Visible
        'End If
#End If

        AniControlEnabled += 1
        Reload() '#4826，在每次进入页面时都刷新一下
        AniControlEnabled -= 1

        '非重复加载部分
        If IsLoaded Then Return
        IsLoaded = True

        SliderLoad()

        'PanLauncherHide 已移除

        '设置解锁

        '主题相关控件已移除
        '极客蓝的处理在 ThemeCheck 中

    End Sub
    Public Sub Reload()
        Try
            '启动器
            'If Setup.Get("UiLauncherTheme") <= 14 Then CType(FindName("RadioLauncherTheme" & Setup.Get("UiLauncherTheme")), MyRadioBox).Checked = True
            ComboDarkMode.SelectedIndex = Setup.Get("UiDarkMode")
            If Not Basics.IsAprilFool Then
                'fix ui state error
                If Setup.Get("UiDarkColor") = ColorTheme.HmclBlue Then Setup.Set("UiDarkColor", ColorTheme.CatBlue)
                If Setup.Get("UiLightColor") = ColorTheme.HmclBlue Then Setup.Set("UiLightColor", ColorTheme.CatBlue)
            End If
            ComboDarkColor.SelectedIndex = Setup.Get("UiDarkColor")
            ComboLightColor.SelectedIndex = Setup.Get("UiLightColor")

            '字体设置
            ComboUiFont.SelectedFontTag = Setup.Get("UiFont")
            ComboUiMotdFont.SelectedFontTag = Setup.Get("UiMotdFont")
            
            ComboBackdropType.SelectedIndex = Setup.Get("UiBackdropType")
            HintBackdropUnsupported.Visibility = If(WindowInterop.IsMicaSupported(), Visibility.Collapsed, Visibility.Visible)

            '标题栏
            CType(FindName("RadioLogoType" & Setup.Get("UiLogoType")), MyRadioBox).Checked = True
            CheckLogoLeft.Visibility = If(RadioLogoType0.Checked, Visibility.Visible, Visibility.Collapsed)
            PanLogoText.Visibility = If(RadioLogoType2.Checked, Visibility.Visible, Visibility.Collapsed)
            PanLogoChange.Visibility = If(RadioLogoType3.Checked, Visibility.Visible, Visibility.Collapsed)
            TextLogoText.Text = Setup.Get("UiLogoText")
            CheckLogoLeft.Checked = Setup.Get("UiLogoLeft")

            '主页
            Try
                ComboCustomPreset.SelectedIndex = Setup.Get("UiCustomPreset")
            Catch
                Setup.Reset("UiCustomPreset")
            End Try
            CType(FindName("RadioCustomType" & Setup.Load("UiCustomType", ForceReload:=True)), MyRadioBox).Checked = True
            TextCustomNet.Text = Setup.Get("UiCustomNet")

            '功能隐藏
            ' 获取配置组引用
            Dim uiHidden = Config.Preference.Hide

            ' 主页面
            CheckHiddenPageDownload.Checked = uiHidden.PageDownload
            CheckHiddenPageSetup.Checked = uiHidden.PageSetup
            CheckHiddenPageTools.Checked = uiHidden.PageTools

            ' 子页面 设置
            CheckHiddenSetupLaunch.Checked = uiHidden.SetupLaunch
            CheckHiddenSetupUI.Checked = uiHidden.SetupUi
            CheckHiddenSetupGameManage.Checked = uiHidden.SetupGameManage
            CheckHiddenSetupJava.Checked = uiHidden.SetupJava
            CheckHiddenLauncherMisc.Checked = uiHidden.SetupLauncherMisc
            CheckHiddenSetupGameLink.Checked = uiHidden.SetupGameLink
            CheckHiddenSetupAbout.Checked = uiHidden.SetupAbout
            CheckHiddenSetupLog.Checked = uiHidden.SetupLog

            ' 子页面 工具
            CheckHiddenToolsGameLink.Checked = uiHidden.ToolsGameLink
            CheckHiddenToolsHelp.Checked = uiHidden.ToolsHelp
            CheckHiddenToolsTest.Checked = uiHidden.ToolsTest

            ' 子页面 实例设置
            CheckHiddenVersionEdit.Checked = uiHidden.InstanceEdit
            CheckHiddenVersionExport.Checked = uiHidden.InstanceExport
            CheckHiddenVersionSave.Checked = uiHidden.InstanceSave
            CheckHiddenVersionScreenshot.Checked = uiHidden.InstanceScreenshot
            CheckHiddenVersionMod.Checked = uiHidden.InstanceMod
            CheckHiddenVersionResourcePack.Checked = uiHidden.InstanceResourcePack
            CheckHiddenVersionShader.Checked = uiHidden.InstanceShader
            CheckHiddenVersionSchematic.Checked = uiHidden.InstanceSchematic
            CheckHiddenVersionServer.Checked = uiHidden.InstanceServer

            ' 特定功能
            CheckHiddenFunctionSelect.Checked = uiHidden.FunctionSelect
            CheckHiddenFunctionModUpdate.Checked = uiHidden.FunctionModUpdate
            CheckHiddenFunctionHidden.Checked = uiHidden.FunctionHidden
        Catch ex As NullReferenceException
            Log(ex, "个性化设置项存在异常，已被自动重置", LogLevel.Msgbox)
            Reset()
        Catch ex As Exception
            Log(ex, "重载个性化设置时出错", LogLevel.Feedback)
        End Try
    End Sub

    '初始化
    Public Sub Reset()
        Try
            Config.Preference.Reset()
            Log("[Setup] 已初始化个性化设置！")
            Hint("已初始化个性化设置", HintType.Finish, False)
        Catch ex As Exception
            Log(ex, "初始化个性化设置失败", LogLevel.Msgbox)
        End Try

        Reload()
    End Sub

    '将控件改变路由到设置改变
    Private Shared Sub SliderChange(sender As MySlider, e As Object)
        If AniControlEnabled = 0 Then Setup.Set(sender.Tag, sender.Value)
    End Sub
    Private Shared Sub ComboChange(sender As MyComboBox, e As Object) Handles ComboDarkMode.SelectionChanged, ComboCustomPreset.SelectionChanged, ComboBackdropType.SelectionChanged
        If AniControlEnabled = 0 Then
            Setup.Set(sender.Tag, sender.SelectedIndex)
            ' 切换窗口材质时立即应用
            If sender.Tag?.ToString() = "UiBackdropType" Then
                FormMain.ApplySystemBackdrop()
                ThemeRefreshMain()
            End If
        End If
    End Sub
    Private Shared Sub CheckBoxChange(sender As MyCheckBox, e As Object) Handles _
    CheckLogoLeft.Change,
    CheckHiddenFunctionHidden.Change, CheckHiddenFunctionSelect.Change, CheckHiddenFunctionModUpdate.Change,
    CheckHiddenPageDownload.Change, CheckHiddenPageSetup.Change, CheckHiddenPageTools.Change,
    CheckHiddenSetupLaunch.Change, CheckHiddenSetupUI.Change, CheckHiddenLauncherMisc.Change, CheckHiddenSetupGameLink.Change,
    CheckHiddenSetupAbout.Change, CheckHiddenSetupLog.Change, CheckHiddenSetupGameManage.Change,
    CheckHiddenToolsGameLink.Change, CheckHiddenToolsHelp.Change, CheckHiddenToolsTest.Change, CheckHiddenSetupJava.Change,
    CheckHiddenVersionEdit.Change, CheckHiddenVersionExport.Change, CheckHiddenVersionSave.Change,
    CheckHiddenVersionScreenshot.Change, CheckHiddenVersionMod.Change, CheckHiddenVersionResourcePack.Change,
    CheckHiddenVersionShader.Change, CheckHiddenVersionSchematic.Change, CheckHiddenVersionServer.Change

        ' 仅在动画未运行或初始化完成时保存设置，防止初始化时的触发导致重复写入
        If AniControlEnabled = 0 Then
            Setup.Set(sender.Tag, sender.Checked)
        End If

    End Sub
    Private Shared Sub TextBoxChange(sender As MyTextBox, e As Object) Handles TextLogoText.ValidatedTextChanged, TextCustomNet.ValidatedTextChanged
        If AniControlEnabled = 0 Then Setup.Set(sender.Tag, sender.Text)
    End Sub
    Private Shared Sub RadioBoxChange(sender As MyRadioBox, e As Object) Handles RadioLogoType0.Check, RadioLogoType1.Check, RadioLogoType2.Check, RadioLogoType3.Check, RadioCustomType0.Check, RadioCustomType1.Check, RadioCustomType2.Check, RadioCustomType3.Check
        Dim gotCfg = sender.Tag.ToString.Split("/")
        If AniControlEnabled = 0 Then Setup.Set(gotCfg(0), Integer.Parse(gotCfg(1)))
    End Sub

    Private Sub ComboFontChange(sender As Object, e As SelectionChangedEventArgs) Handles ComboUiFont.SelectionChanged
        If AniControlEnabled = 0 Then
            Setup.Set("UiFont", ComboUiFont.SelectedFontTag)
        End If
    End Sub
    
    Private Sub ComboMotdFontChange(sender As Object, e As SelectionChangedEventArgs) Handles ComboUiMotdFont.SelectionChanged
        If AniControlEnabled = 0 Then
            Setup.Set("UiMotdFont", ComboUiMotdFont.SelectedFontTag)
        End If
    End Sub

    '顶部栏
    Private Sub BtnLogoChange_Click(sender As Object, e As EventArgs) Handles BtnLogoChange.Click
        Dim FileName As String = SystemDialogs.SelectFile("常用图片文件(*.png;*.jpg;*.gif;*.webp)|*.png;*.jpg;*.gif;*.webp", "选择图片")
        If FileName = "" Then Return
        Try
            '拷贝文件
            File.Delete(ExePath & "PCL\Logo.png")
            CopyFile(FileName, ExePath & "PCL\Logo.png")
            '设置当前显示
            FrmMain.ImageTitleLogo.Source = Nothing '防止因为 Source 属性前后的值相同而不更新 (#5628)
            FrmMain.ImageTitleLogo.Source = ExePath & "PCL\Logo.png"
        Catch ex As Exception
            If ex.Message.Contains("参数无效") Then
                Log("改变标题栏图片失败，该图片文件可能并非标准格式。" & vbCrLf &
                    "你可以尝试使用画图打开该文件并重新保存，这会让图片变为标准格式。", LogLevel.Msgbox)
            Else
                Log(ex, "设置标题栏图片失败", LogLevel.Msgbox)
            End If
            FrmMain.ImageTitleLogo.Source = Nothing
        End Try
    End Sub
    Private Sub RadioLogoType3_Check(sender As Object, e As RouteEventArgs) Handles RadioLogoType3.PreviewCheck
        If Not (AniControlEnabled = 0 AndAlso e.RaiseByMouse) Then Return
Refresh:
        '已有图片则不再选择
        If File.Exists(ExePath & "PCL\Logo.png") Then
            Try
                FrmMain.ImageTitleLogo.Source = Nothing '防止因为 Source 属性前后的值相同而不更新 (#5628)
                FrmMain.ImageTitleLogo.Source = ExePath & "PCL\Logo.png"
            Catch ex As Exception
                If ex.Message.Contains("参数无效") Then
                    Log("调整标题栏图片失败，该图片文件可能并非标准格式。" & vbCrLf &
                    "你可以尝试使用画图打开该文件并重新保存，这会让图片变为标准格式。", LogLevel.Msgbox)
                Else
                    Log(ex, "调整标题栏图片失败", LogLevel.Msgbox)
                End If
                FrmMain.ImageTitleLogo.Source = Nothing
                e.Handled = True
                Try
                    File.Delete(ExePath & "PCL\Logo.png")
                Catch exx As Exception
                    Log(exx, "清理错误的标题栏图片失败", LogLevel.Msgbox)
                End Try
            End Try
            Return
        End If
        '没有图片则要求选择
        Dim FileName As String = SystemDialogs.SelectFile("常用图片文件(*.png;*.jpg;*.gif;*.webp)|*.png;*.jpg;*.gif;*.webp", "选择图片")
        If FileName = "" Then
            FrmMain.ImageTitleLogo.Source = Nothing
            e.Handled = True
        Else
            Try
                '拷贝文件
                File.Delete(ExePath & "PCL\Logo.png")
                CopyFile(FileName, ExePath & "PCL\Logo.png")
                GoTo Refresh
            Catch ex As Exception
                Log(ex, "复制标题栏图片失败", LogLevel.Msgbox)
            End Try
        End If
    End Sub
    Private Sub BtnLogoDelete_Click(sender As Object, e As EventArgs) Handles BtnLogoDelete.Click
        Try
            File.Delete(ExePath & "PCL\Logo.png")
            RadioLogoType1.SetChecked(True, True)
            Hint("标题栏图片已清空！", HintType.Finish)
        Catch ex As Exception
            Log(ex, "清空标题栏图片失败", LogLevel.Msgbox)
        End Try
    End Sub

    '主页
    Private Sub BtnCustomFile_Click(sender As Object, e As EventArgs) Handles BtnCustomFile.Click
        Try
            If File.Exists(ExePath & "PCL\Custom.xaml") Then
                If MyMsgBox("当前已存在布局文件，继续生成教学文件将会覆盖现有布局文件！", "覆盖确认", "继续", "取消", IsWarn:=True) = 2 Then Return
            End If
            WriteFile(ExePath & "PCL\Custom.xaml", GetResourceStream("Resources/Custom.xml"))
            Hint("教学文件已生成！", HintType.Finish)
            OpenExplorer(ExePath & "PCL\Custom.xaml")
        Catch ex As Exception
            Log(ex, "生成教学文件失败", LogLevel.Feedback)
        End Try
    End Sub
    Private Sub BtnCustomRefresh_Click() Handles BtnCustomRefresh.Click
        FrmLaunchRight.ForceRefresh()
        Hint("已刷新主页！", HintType.Finish)
    End Sub
    Private Sub BtnCustomTutorial_Click(sender As Object, e As EventArgs) Handles BtnCustomTutorial.Click
        MyMsgBox("1. 点击 生成教学文件 按钮，这会在 PCL 文件夹下生成 Custom.xaml 布局文件。" & vbCrLf &
                 "2. 使用记事本等工具打开这个文件并进行修改，修改完记得保存。" & vbCrLf &
                 "3. 点击 刷新主页 按钮，查看主页现在长啥样了。" & vbCrLf &
                 vbCrLf &
                 "你可以在生成教学文件后直接刷新主页，对照着进行修改，更有助于理解。" & vbCrLf &
                 "直接将主页文件拖进 PCL 窗口也可以快捷加载。", "主页自定义教程")
    End Sub

    '主题
    Private Sub ThemeColor_Change(sender As MyComboBox, e As EventArgs) Handles ComboDarkColor.SelectionChanged, ComboLightColor.SelectionChanged
        Setup.Set(sender.Tag, sender.SelectedIndex)
        ThemeRefresh()
    End Sub

    '主题自定义（已移除相关控件）
    'Private Sub RadioLauncherTheme14_Change(sender As Object, e As RouteEventArgs) Handles RadioLauncherTheme14.Changed
    '    If LabLauncherHue.Visibility = Visibility.Collapsed Then Return
    '    LabLauncherHue.Visibility = Visibility.Collapsed
    '    SliderLauncherHue.Visibility = Visibility.Collapsed
    '    LabLauncherSat.Visibility = Visibility.Collapsed
    '    SliderLauncherSat.Visibility = Visibility.Collapsed
    '    LabLauncherDelta.Visibility = Visibility.Collapsed
    '    SliderLauncherDelta.Visibility = Visibility.Collapsed
    '    LabLauncherLight.Visibility = Visibility.Collapsed
    '    SliderLauncherLight.Visibility = Visibility.Collapsed
    '    CardLauncher.TriggerForceResize()
    'End Sub
    'Private Sub HSL_Change() Handles SliderLauncherHue.Change, SliderLauncherLight.Change, SliderLauncherSat.Change, SliderLauncherDelta.Change
    '    If AniControlEnabled <> 0 OrElse SliderLauncherSat Is Nothing OrElse Not SliderLauncherSat.IsLoaded Then Return
    '    ThemeRefresh()
    'End Sub

#Region "功能隐藏"

    Private Shared _HiddenForceShow As Boolean = False
    ''' <summary>
    ''' 是否强制显示被禁用的功能。
    ''' </summary>
    Public Shared Property HiddenForceShow As Boolean
        Get
            Return _HiddenForceShow
        End Get
        Set(value As Boolean)
            _HiddenForceShow = value
            HiddenRefresh()
        End Set
    End Property

    ''' <summary>
    ''' 更新功能隐藏带来的显示变化。
    ''' </summary>
    Public Shared Sub HiddenRefresh() Handles Me.Loaded
        If FrmMain.PanTitleSelect Is Nothing OrElse Not FrmMain.PanTitleSelect.IsLoaded Then Return
        Try
            ' 获取配置组引用以缩短代码
            Dim conf = Config.Preference.Hide

            ' 顶部栏：下载、设置、工具
            Dim IsAllTitleHidden As Boolean = Not HiddenForceShow AndAlso
                                    conf.PageDownload AndAlso
                                    conf.PageSetup AndAlso
                                    conf.PageTools

            If IsAllTitleHidden Then
                FrmMain.PanTitleSelect.Visibility = Visibility.Collapsed
            Else
                FrmMain.PanTitleSelect.Visibility = Visibility.Visible
                FrmMain.BtnTitleSelect1.Visibility = If(Not HiddenForceShow AndAlso conf.PageDownload, Visibility.Collapsed, Visibility.Visible)
                FrmMain.BtnTitleSelect2.Visibility = If(Not HiddenForceShow AndAlso conf.PageSetup, Visibility.Collapsed, Visibility.Visible)
            End If

            ' 功能隐藏设置卡片
            If FrmSetupUI IsNot Nothing Then
                FrmSetupUI.CardSwitch.Visibility = If(Not HiddenForceShow AndAlso conf.FunctionHidden, Visibility.Collapsed, Visibility.Visible)
                FrmSetupUI.CardSwitch.Title = If(HiddenForceShow, "功能隐藏（已暂时关闭，按 F12 以重新启用）", "功能隐藏")
            End If

            ' 设置子页面 (FrmSetupLeft)
            If FrmSetupLeft IsNot Nothing Then
                FrmSetupLeft.ItemLaunch.Visibility = If(Not HiddenForceShow AndAlso conf.SetupLaunch, Visibility.Collapsed, Visibility.Visible)
                FrmSetupLeft.ItemUI.Visibility = If(Not HiddenForceShow AndAlso conf.SetupUi, Visibility.Collapsed, Visibility.Visible)
                FrmSetupLeft.ItemGameManage.Visibility = If(Not HiddenForceShow AndAlso conf.SetupGameManage, Visibility.Collapsed, Visibility.Visible)
                FrmSetupLeft.ItemLauncherMisc.Visibility = If(Not HiddenForceShow AndAlso conf.SetupLauncherMisc, Visibility.Collapsed, Visibility.Visible)
                FrmSetupLeft.ItemJava.Visibility = If(Not HiddenForceShow AndAlso conf.SetupJava, Visibility.Collapsed, Visibility.Visible)
                FrmSetupLeft.ItemAbout.Visibility = If(Not HiddenForceShow AndAlso conf.SetupAbout, Visibility.Collapsed, Visibility.Visible)
                FrmSetupLeft.ItemLog.Visibility = If(Not HiddenForceShow AndAlso conf.SetupLog, Visibility.Collapsed, Visibility.Visible)

                Dim categories = {
    (FrmSetupLeft.TextGameCategory, Not (conf.SetupLaunch AndAlso conf.SetupJava AndAlso conf.SetupGameManage)),
    (FrmSetupLeft.TextLauncherCategory, Not (conf.SetupUi AndAlso conf.SetupLauncherMisc)),
    (FrmSetupLeft.TextToolboxCategory, True),
    (FrmSetupLeft.TextAboutCategory, Not (conf.SetupAbout AndAlso conf.SetupLog))
}

                For Each category In categories
                    Dim isVisible = category.Item2 OrElse HiddenForceShow
                    category.Item1.Visibility = If(isVisible, Visibility.Visible, Visibility.Collapsed)
                    If isVisible Then category.Item1.Opacity = 0.6
                Next

                ' 统计设置页可用项数量
                Dim SetupCount As Integer = 0
                If Not conf.SetupLaunch Then SetupCount += 1
                If Not conf.SetupUi Then SetupCount += 1
                If Not conf.SetupGameManage Then SetupCount += 1
                If Not conf.SetupLauncherMisc Then SetupCount += 1
                If Not conf.SetupJava Then SetupCount += 1
                If Not conf.SetupGameLink Then SetupCount += 1
                If Not conf.SetupAbout Then SetupCount += 1
                If Not conf.SetupLog Then SetupCount += 1
                FrmSetupLeft.PanItem.Visibility = If(SetupCount < 2 AndAlso Not HiddenForceShow, Visibility.Collapsed, Visibility.Visible)
            End If

            ' 其他入口刷新
            If FrmMain.PageCurrent = FormMain.PageType.InstanceSelect Then FrmSelectRight.BtnEmptyDownload_Loaded()
            If FrmMain.PageCurrent = FormMain.PageType.Launch Then FrmLaunchLeft.RefreshButtonsUI()
            If FrmMain.PageCurrent = FormMain.PageType.InstanceSetup AndAlso FrmInstanceModDisabled IsNot Nothing Then FrmInstanceModDisabled.BtnDownload_Loaded()

        Catch ex As Exception
            Log(ex, "刷新功能隐藏项目失败", LogLevel.Feedback)
        End Try
    End Sub

    ' ================= 设置页面协同 =================
    Private Sub HiddenSetupMain() Handles CheckHiddenPageSetup.Change
        Dim IsChecked As Boolean = CheckHiddenPageSetup.Checked
        CheckHiddenSetupLaunch.Checked = IsChecked
        CheckHiddenSetupUI.Checked = IsChecked
        CheckHiddenSetupGameManage.Checked = IsChecked
        CheckHiddenLauncherMisc.Checked = IsChecked
        CheckHiddenSetupJava.Checked = IsChecked
        CheckHiddenSetupGameLink.Checked = IsChecked
        CheckHiddenSetupAbout.Checked = IsChecked
        CheckHiddenSetupLog.Checked = IsChecked
    End Sub

    ' ================= 设置页面协同 =================
    Private Sub HiddenSetupMain(sender As Object, user As Boolean) Handles CheckHiddenPageSetup.Change
        If Not user Then Return ' 仅处理用户点击，防止死循环
        Dim IsChecked As Boolean = CheckHiddenPageSetup.Checked
        CheckHiddenSetupLaunch.Checked = IsChecked
        CheckHiddenSetupUI.Checked = IsChecked
        CheckHiddenSetupGameManage.Checked = IsChecked
        CheckHiddenLauncherMisc.Checked = IsChecked
        CheckHiddenSetupJava.Checked = IsChecked
        CheckHiddenSetupGameLink.Checked = IsChecked
        CheckHiddenSetupAbout.Checked = IsChecked
        CheckHiddenSetupLog.Checked = IsChecked
    End Sub

    Private Sub HiddenSetupSub(sender As Object, user As Boolean) Handles CheckHiddenSetupLaunch.Change, CheckHiddenSetupUI.Change,
    CheckHiddenSetupJava.Change, CheckHiddenSetupGameManage.Change, CheckHiddenLauncherMisc.Change, CheckHiddenSetupGameLink.Change,
    CheckHiddenSetupAbout.Change, CheckHiddenSetupLog.Change

        If Not user Then Return
        Dim conf = Config.Preference.Hide
        ' 判断是否全部勾选
        Dim AllChecked As Boolean = conf.SetupLaunch AndAlso conf.SetupUi AndAlso conf.SetupJava AndAlso
                               conf.SetupGameLink AndAlso conf.SetupAbout AndAlso
                               conf.SetupLog AndAlso conf.SetupLauncherMisc AndAlso conf.SetupGameManage
        CheckHiddenPageSetup.Checked = AllChecked
    End Sub

    ' ================= 工具页面协同 =================
    Private Sub HiddenToolsMain(sender As Object, user As Boolean) Handles CheckHiddenPageTools.Change
        If Not user Then Return
        Dim IsChecked As Boolean = CheckHiddenPageTools.Checked
        CheckHiddenToolsGameLink.Checked = IsChecked
        CheckHiddenToolsHelp.Checked = IsChecked
        CheckHiddenToolsTest.Checked = IsChecked
    End Sub

    Private Sub HiddenToolsSub(sender As Object, user As Boolean) Handles CheckHiddenToolsGameLink.Change,
    CheckHiddenToolsHelp.Change, CheckHiddenToolsTest.Change

        If Not user Then Return
        Dim conf = Config.Preference.Hide
        Dim AllChecked As Boolean = conf.ToolsGameLink AndAlso conf.ToolsHelp AndAlso conf.ToolsTest
        CheckHiddenPageTools.Checked = AllChecked
    End Sub

    '警告提示
    Private Sub HiddenHint(sender As Object, user As Boolean) Handles CheckHiddenFunctionHidden.Change, CheckHiddenPageSetup.Change, CheckHiddenSetupUI.Change
        If AniControlEnabled = 0 AndAlso sender.Checked Then Hint("按 F12 即可暂时关闭功能隐藏设置。千万别忘了，要不然设置就改不回来了……")
    End Sub

#End Region

    '赞助（已移除相关控件）
    'Private Sub BtnLauncherDonate_Click(sender As Object, e As EventArgs) Handles BtnLauncherDonate.Click
    '    OpenWebsite("https://afdian.com/a/LTCat")
    'End Sub

    '滑动条
    Private Sub SliderLoad()
    End Sub
    Private Sub BtnHomepageMarket_Click(sender As Object, e As EventArgs) Handles BtnGotoHomepageMarket.Click
        FrmMain.PageChange(New FormMain.PageStackData With {.Page = FormMain.PageType.HomepageMarket})
    End Sub
End Class
