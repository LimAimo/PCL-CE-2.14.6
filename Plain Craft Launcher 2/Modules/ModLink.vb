Imports System.Runtime.InteropServices
Imports PCL.Core.App
Imports PCL.Core.Utils.OS

Public Module ModLink

#Region "端口查找"
    Public Class PortFinder
        ' 定义需要的结构和常量
        <StructLayout(LayoutKind.Sequential)>
        Public Structure MIB_TCPROW_OWNER_PID
            Public dwState As Integer
            Public dwLocalAddr As Integer
            Public dwLocalPort As Integer
            Public dwRemoteAddr As Integer
            Public dwRemotePort As Integer
            Public dwOwningPid As Integer
        End Structure

        <DllImport("iphlpapi.dll", SetLastError:=True)>
        Public Shared Function GetExtendedTcpTable(
        ByVal pTcpTable As IntPtr,
        ByRef dwOutBufLen As Integer,
        ByVal bOrder As Boolean,
        ByVal ulAf As Integer,
        ByVal TableClass As Integer,
        ByVal reserved As Integer) As Integer
        End Function

        Public Shared Function GetProcessPort(ByVal dwProcessId As Integer) As List(Of Integer)
            Dim ports As New List(Of Integer)
            Dim tcpTable As IntPtr = IntPtr.Zero
            Dim dwSize As Integer = 0
            Dim dwRetVal As Integer

            If dwProcessId = 0 Then
                Return ports
            End If

            dwRetVal = GetExtendedTcpTable(IntPtr.Zero, dwSize, True, 2, 3, 0)
            If dwRetVal <> 0 AndAlso dwRetVal <> 122 Then ' 122 表示缓冲区不足
                Return ports
            End If

            tcpTable = Marshal.AllocHGlobal(dwSize)
            Try
                If GetExtendedTcpTable(tcpTable, dwSize, True, 2, 3, 0) <> 0 Then
                    Return ports
                End If

                Dim tablePtr As IntPtr = tcpTable
                Dim dwNumEntries As Integer = Marshal.ReadInt32(tablePtr)
                tablePtr = IntPtr.Add(tablePtr, 4)

                For i As Integer = 0 To dwNumEntries - 1
                    Dim row As MIB_TCPROW_OWNER_PID = Marshal.PtrToStructure(Of MIB_TCPROW_OWNER_PID)(tablePtr)
                    If row.dwOwningPid = dwProcessId Then
                        ports.Add(row.dwLocalPort >> 8 Or (row.dwLocalPort And &HFF) << 8) ' 转换端口号
                    End If
                    tablePtr = IntPtr.Add(tablePtr, Marshal.SizeOf(Of MIB_TCPROW_OWNER_PID)())
                Next
            Finally
                Marshal.FreeHGlobal(tcpTable)
            End Try

            Return ports
        End Function
    End Class
#End Region

#Region "Minecraft 实例探测（已简化）"
    ''' <summary>
    ''' Minecraft 实例探测功能已简化（McPing 模块已移除）
    ''' </summary>
    Public Async Function MCInstanceFinding() As Task(Of List(Of Tuple(Of Integer, Object, String)))
        Log("[MCDetect] Minecraft 实例探测功能已禁用（Link 模块已移除）", LogLevel.Hint)
        Return New List(Of Tuple(Of Integer, Object, String))()
    End Function

    Public Function GetLauncherBrand(pid As Integer) As String
        Try
            Dim cmd = ProcessInterop.GetCommandLine(pid)
            If cmd.Contains("-Dminecraft.launcher.brand=") Then
                Return cmd.AfterFirst("-Dminecraft.launcher.brand=").BeforeFirst("-").TrimEnd("'", " ")
            Else
                Return cmd.AfterFirst("--versionType ").BeforeFirst("-").TrimEnd("'", " ")
            End If
        Catch ex As Exception
            Log(ex, $"[MCDetect] 检测 PID {pid} 进程的启动参数失败")
            Return ""
        End Try
    End Function
#End Region

#Region "EasyTier（已禁用）"
    ''' <summary>
    ''' EasyTier 下载功能已禁用（Link 模块已移除）
    ''' </summary>
    Public Function DownloadEasyTier() As Integer
        Log("[Link] EasyTier 下载功能已禁用", LogLevel.Hint)
        Return 0
    End Function
#End Region

#Region "大厅操作（已禁用）"
    ''' <summary>
    ''' 大厅预检查功能已禁用（Link 模块已移除）
    ''' </summary>
    Public Function LobbyPrecheck() As Boolean
        Hint("联机大厅功能已禁用", HintType.Critical)
        Return False
    End Function
#End Region

End Module