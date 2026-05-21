Public Module ModVideoBack
    Public Class BooleanEventArgs
        Inherits EventArgs
        Public Property Value As Boolean

        Public Sub New(value As Boolean)
            Me.Value = value
        End Sub
    End Class

    Public Event GamingStateChanged As EventHandler(Of BooleanEventArgs)
    Public Event ForcePlayChanged As EventHandler(Of BooleanEventArgs)

    Private _isGaming As Boolean = False
    Private _forcePlay As Boolean = False

    Public Property IsGaming As Boolean
        Get
            Return _isGaming
        End Get
        Set(value As Boolean)
            If _isGaming <> value Then
                _isGaming = value
                RaiseEvent GamingStateChanged(Nothing, New BooleanEventArgs(value))
            End If
        End Set
    End Property

    Public Property ForcePlay As Boolean
        Get
            Return _forcePlay
        End Get
        Set(value As Boolean)
            If _forcePlay <> value Then
                _forcePlay = value
                RaiseEvent ForcePlayChanged(Nothing, New BooleanEventArgs(value))
            End If
        End Set
    End Property

    Public Sub OnGamingStateChanged(sender As Object, e As ModVideoBack.BooleanEventArgs)
        ' Background video removed - no-op
    End Sub

    Public Sub OnForcePlayChanged(sender As Object, e As ModVideoBack.BooleanEventArgs)
        ' Background video removed - no-op
    End Sub

    Public IsMinimized As Boolean = False

    ''' <summary>
    ''' No-op: video background removed.
    ''' </summary>
    Public Sub VideoPlay()
    End Sub

    ''' <summary>
    ''' No-op: video background removed.
    ''' </summary>
    Public Sub VideoStop()
    End Sub

    ''' <summary>
    ''' No-op: video background removed.
    ''' </summary>
    Public Sub VideoPause()
    End Sub
End Module
