Imports Microsoft.VisualBasic
Imports System.Management

Module CheckWindows11Requirements

    Public Sub RunChecks()
        Console.WriteLine("Checking requirements for Windows 11...")

        CheckCPUArchitecture()
        CheckMemory()
        CheckStorage()
        CheckTPM()
        CheckSecureBoot()

        Console.WriteLine("Check completed. Press any key to exit.")
        Console.ReadKey()
    End Sub

    Sub CheckCPUArchitecture()
        Dim query As String = "SELECT Architecture FROM Win32_Processor"
        Dim searcher As New ManagementObjectSearcher(query)
        Dim results = searcher.Get()

        If results.Count = 0 Then
            Console.WriteLine("CPU Architecture: Unable to retrieve information.")
            Return
        End If

        For Each obj As ManagementObject In results
            Dim architecture As Integer = CInt(obj("Architecture"))
            If architecture = 9 Then
                Console.WriteLine("CPU Architecture: 64-bit (Compatible)")
            Else
                Console.WriteLine("CPU Architecture: Not 64-bit (Incompatible)")
            End If
        Next
    End Sub

    Sub CheckMemory()
        Dim query As String = "SELECT Capacity FROM Win32_PhysicalMemory"
        Dim searcher As New ManagementObjectSearcher(query)
        Dim results = searcher.Get()
        Dim totalMemory As Long = 0

        If results.Count = 0 Then
            Console.WriteLine("Memory: Unable to retrieve information.")
            Return
        End If

        For Each obj As ManagementObject In results
            totalMemory += CLng(obj("Capacity"))
        Next

        totalMemory = totalMemory / (1024 * 1024 * 1024) ' Convert to GB

        If totalMemory >= 4 Then
            Console.WriteLine("Memory: " & totalMemory & " GB (Compatible)")
        Else
            Console.WriteLine("Memory: " & totalMemory & " GB (Incompatible, minimum 4 GB required)")
        End If
    End Sub

    Sub CheckStorage()
        Dim query As String = "SELECT Size FROM Win32_LogicalDisk WHERE DriveType=3"
        Dim searcher As New ManagementObjectSearcher(query)
        Dim results = searcher.Get()
        Dim totalStorage As Long = 0

        If results.Count = 0 Then
            Console.WriteLine("Storage: Unable to retrieve information.")
            Return
        End If

        For Each obj As ManagementObject In results
            totalStorage += CLng(obj("Size"))
        Next

        totalStorage = totalStorage / (1024 * 1024 * 1024) ' Convert to GB

        If totalStorage >= 64 Then
            Console.WriteLine("Storage: " & totalStorage & " GB (Compatible)")
        Else
            Console.WriteLine("Storage: " & totalStorage & " GB (Incompatible, minimum 64 GB required)")
        End If
    End Sub

    Sub CheckTPM()
        Try
            Dim searcher As New ManagementObjectSearcher("ROOT\\CIMV2\\Security\\MicrosoftTpm", "SELECT * FROM Win32_Tpm")
            Dim tpmFound As Boolean = False

            For Each obj As ManagementObject In searcher.Get()
                Dim version As String = obj("SpecVersion").ToString()
                If version.StartsWith("2.0") Then
                    Console.WriteLine("TPM Version: " & version & " (Compatible)")
                    tpmFound = True
                End If
            Next

            If Not tpmFound Then
                Console.WriteLine("TPM: Not found or incompatible (Requires TPM 2.0)")
            End If
        Catch ex As Exception
            Console.WriteLine("TPM: Unable to check (Requires TPM 2.0)")
        End Try
    End Sub

    Sub CheckSecureBoot()
        Try
            Dim query As String = "SELECT SecureBootEnabled FROM Win32_ComputerSystem"
            Dim searcher As New ManagementObjectSearcher(query)
            Dim results = searcher.Get()

            If results.Count = 0 Then
                Console.WriteLine("Secure Boot: Unable to retrieve information.")
                Return
            End If

            For Each obj As ManagementObject In results
                Dim secureBootEnabled As Boolean = CBool(obj("SecureBootEnabled"))
                If secureBootEnabled Then
                    Console.WriteLine("Secure Boot: Enabled (Compatible)")
                Else
                    Console.WriteLine("Secure Boot: Disabled (Incompatible)")
                End If
            Next
        Catch ex As Exception
            Console.WriteLine("Secure Boot: Unable to check")
        End Try
    End Sub

End Module
