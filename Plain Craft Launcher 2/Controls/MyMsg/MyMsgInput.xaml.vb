Imports PCL.Core.UI.Controls

Public Class MyMsgInput

    Private ReadOnly MyConverter As MyMsgBoxConverter
    Private ReadOnly Uuid As Integer = GetUuid()

    Public Sub New(Converter As MyMsgBoxConverter)
        Try

            InitializeComponent()
            Btn1.Name = Btn1.Name & GetUuid()
            Btn2.Name = Btn2.Name & GetUuid()
            MyConverter = Converter
            LabTitle.Text = Converter.Title
            LabText.Text = Converter.Text
            PanText.Visibility = If(Converter.Text = "", Visibility.Collapsed, Visibility.Visible)
            TextArea.Text = Converter.Content
            TextArea.HintText = Converter.HintText
            TextArea.ValidateRules = Converter.ValidateRules
            Btn1.Text = Converter.Button1
            If Converter.IsWarn Then
                Btn1.ColorType = MyButton.ColorState.Red
                LabTitle.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrushRedLight")
            End If
            Btn2.Text = Converter.Button2
            Btn2.Visibility = If(Converter.Button2 = "", Visibility.Collapsed, Visibility.Visible)
            ShapeLine.StrokeThickness = GetWPFSize(1)

        Catch ex As Exception
            Log(ex, "输入弹窗初始化失败", LogLevel.Hint)
        End Try
    End Sub

    Private Sub Load(sender As Object, e As EventArgs) Handles MyBase.Loaded
        Try

            'UI 初始化
            If Btn2.IsVisible AndAlso Not Btn1.ColorType = MyButton.ColorState.Red Then Btn1.ColorType = MyButton.ColorState.Highlight
            TextArea.Focus()
            TextArea.SelectionStart = TextArea.Text.Length
            '动画
            Opacity = 0
            AniStart(AaColor(FrmMain.PanMsgBackground, BlurBorder.BackgroundProperty, If(MyConverter.IsWarn, New MyColor(140, 80, 0, 0), New MyColor(90, 0, 0, 0)) - FrmMain.PanMsgBackground.Background, 150), "PanMsgBackground Background")
            AniStart({
                AaOpacity(Me, 1, 120, 30),
                AaDouble(Sub(i) TransformScale.ScaleX += i, 1 - TransformScale.ScaleX, 150, 30, New AniEaseOutBack(AniEasePower.Weak)),
                AaDouble(Sub(i) TransformScale.ScaleY += i, 1 - TransformScale.ScaleY, 150, 30, New AniEaseOutBack(AniEasePower.Weak))
            }, "MyMsgBox " & Uuid)
            '记录日志
            Log("[Control] 输入弹窗：" & LabTitle.Text)

        Catch ex As Exception
            Log(ex, "输入弹窗加载失败", LogLevel.Hint)
        End Try
    End Sub
    Private Sub Close()
        '结束线程阻塞
        MyConverter.WaitFrame.Continue = False
        Interop.ComponentDispatcher.PopModal()
        '动画
        AniStart({
            AaCode(
            Sub()
                If Not WaitingMyMsgBox.Any() Then
                    AniStart(AaColor(FrmMain.PanMsgBackground, BlurBorder.BackgroundProperty, New MyColor(0, 0, 0, 0) - FrmMain.PanMsgBackground.Background, 150, Ease:=New AniEaseOutFluent(AniEasePower.Weak)))
                End If
            End Sub, 20),
            AaOpacity(Me, -Opacity, 100, 20),
            AaDouble(Sub(i) TransformScale.ScaleX += i, 0.95 - TransformScale.ScaleX, 100, 20, New AniEaseInFluent(AniEasePower.Weak)),
            AaDouble(Sub(i) TransformScale.ScaleY += i, 0.95 - TransformScale.ScaleY, 100, 20, New AniEaseInFluent(AniEasePower.Weak)),
            AaCode(Sub() CType(Parent, Grid).Children.Remove(Me), , True)
        }, "MyMsgBox " & Uuid)
    End Sub

    Public Sub Btn1_Click() Handles Btn1.Click
        TextArea.Validate() '#5773
        If MyConverter.IsExited OrElse Not TextArea.IsValidated Then Return
        MyConverter.IsExited = True
        MyConverter.Result = TextArea.Text
        Close()
    End Sub
    Public Sub Btn2_Click() Handles Btn2.Click
        If MyConverter.IsExited Then Return
        MyConverter.IsExited = True
        MyConverter.Result = Nothing
        Close()
    End Sub

    Private Sub TextCaption_ValidateChanged(sender As Object, e As EventArgs) Handles TextArea.ValidateChanged
        Btn1.IsEnabled = TextArea.IsValidated
    End Sub

    Private Sub Drag(sender As Object, e As MouseButtonEventArgs) Handles PanBorder.MouseLeftButtonDown, LabTitle.MouseLeftButtonDown
        Try
            If e.LeftButton = MouseButtonState.Pressed Then
                If e.GetPosition(ShapeLine).Y <= 2 Then
                    FrmMain.DragMove()
                End If
            End If
        Catch ex As Exception
            Log(ex, "拖拽移动失败", LogLevel.Hint)
        End Try
    End Sub

End Class
