﻿Imports FailuresHandler
Public Class FutureRecordings
    Inherits IEX.ElementaryActions.EPG.SF.COGECO.FutureRecordings

    Dim _uUI As IEX.ElementaryActions.EPG.SF.GET.UI

    Sub New(ByVal _pIex As IEXGateway.IEX, ByVal pUI As IEX.ElementaryActions.EPG.SF.UI)
        MyBase.New(_pIex, pUI)
        _uUI = pUI
    End Sub

    Public Overrides Sub Navigate()
        _uUI.Utils.StartHideFailures("Navigating To Planner")

        Try
            _uUI.Utils.EPG_Milestones_NavigateByName("STATE:MY LIBRARY")
            _uUI.Utils.ClearEPGInfo()
            _uUI.Utils.EPG_Milestones_Navigate("BOOKINGS")

        Finally
            _iex.ForceHideFailure()
        End Try
    End Sub
    ''' <summary>
    '''   Checking If Planner Is Empty
    ''' </summary>
    ''' <returns>True If Empty False Is Not</returns>
    ''' <remarks></remarks>
    Public Overrides Function isEmpty() As Boolean
        Dim Msg As String = ""

        _UI.Utils.StartHideFailures("Checking If Planner Is Empty")

        Try
            Dim EventName As String = ""

            Try
                ' _Utils.GetEpgInfo("displayTitle", EventName)
                _uUI.Utils.VerifyState("LIBRARY ERROR")
                Msg = "Planner Is Not Empty"
                Return False
            Catch ex As EAException
                Msg = "Planner Is Empty !!!"
                Return True
            End Try
        Finally
            _iex.ForceHideFailure()
            If Msg <> "" Then
                _UI.Utils.LogCommentInfo(Msg)
            End If
        End Try
    End Function


    ''' <summary>
    '''   Finding Requested Event On Planner
    ''' </summary>
    ''' <param name="EventName">The Name Of The Event</param>
    ''' <param name="EventDate">The Event Converted Date</param>
    ''' <param name="StartTime">The Event Start Time</param>
    ''' <param name="EndTime">The Event End Time</param>
    ''' <remarks>
    ''' Possible Error Codes:
    ''' <para>300 - NavigationFailure</para> 
    ''' <para>301 - DictionaryFailure</para> 
    ''' <para>304 - IRVerificationFailure</para> 
    ''' <para>306 - GetEventInfoFailure</para> 
    ''' <para>328 - INIFailure</para> 
    ''' <para>321 - VerifyChannelAttributeFailure</para> 
    ''' <para>346 - FindEventFailure</para> 
    ''' <para>347 - SelectEventFailure</para> 
    ''' </remarks>
    Public Overrides Sub FindEvent(ByVal EventName As String, Optional ByVal EventDate As String = "", Optional ByVal StartTime As String = "", Optional ByVal EndTime As String = "")
        Dim FirstEvent As String = ""
        Dim CheckedEvent As String = ""
        Dim CheckedEventDate As String = ""
        Dim ReturnedValue As String = ""
        Dim EpgText As String = ""
        Dim PlannerEventDate As String = ""
        Dim PlannerEventTime As String = ""
        Dim PlannerStartTime As String = ""
        Dim PlannerEndTime As String = ""
        Dim FirstLoop As Boolean = True
        Dim VerifyByDetails As Boolean = False

        _uUI.Utils.StartHideFailures("Finding Event : " + EventName + If(String.IsNullOrEmpty(EventDate), "", " EventDate :" + EventDate) + If(String.IsNullOrEmpty(StartTime), "", " StartTime :" + StartTime) + If(String.IsNullOrEmpty(EndTime), "", " EndTime :" + EndTime) + " On Planner")

        Try


            EpgText = _uUI.Utils.GetValueFromDictionary("DIC_STORE_ADULT_CONTENT")

            _uUI.Utils.LogCommentWarning("Workaround : Checking If EventName Contains Checked Event")

            Do
                GetSelectedEventName(CheckedEvent)

                'In case of ADULT content
                If CheckedEvent = EpgText Then
                    GetAdultEventName(CheckedEvent)
                End If

                If EventName.Contains(CheckedEvent) And (CheckedEvent <> "") And (EventName <> "") Then



                    If Not String.IsNullOrEmpty(EventDate) Then
                        GetSelectedEventDetails(PlannerEventDate)
                        VerifyByDetails = True
                    End If

                    If Not String.IsNullOrEmpty(StartTime) OrElse Not String.IsNullOrEmpty(EndTime) Then
                        GetSelectedEventTime(PlannerEventTime)
                        VerifyByDetails = True
                    End If

                    If Not String.IsNullOrEmpty(StartTime) Then
                        _uUI.Utils.ParseEventTime(ReturnedTime:=PlannerStartTime, TimeString:=PlannerEventTime, IsStartTime:=True)
                    End If
                    If Not String.IsNullOrEmpty(EndTime) Then
                        _uUI.Utils.ParseEventTime(ReturnedTime:=PlannerEndTime, TimeString:=PlannerEventTime, IsStartTime:=False)
                    End If

                    If PlannerEventDate.Contains(EventDate.ToUpper) AndAlso StartTime = PlannerStartTime AndAlso EndTime = PlannerEndTime Then
                        _iex.ForceHideFailure()
                        Exit Sub
                    ElseIf String.IsNullOrEmpty(EventDate) AndAlso String.IsNullOrEmpty(StartTime) AndAlso String.IsNullOrEmpty(EndTime) Then
                        _iex.ForceHideFailure()
                        Exit Sub
                    End If
                End If

                NextEvent(times:=1, VerifyByDetails:=VerifyByDetails)

                If FirstLoop Then
                    FirstLoop = False
                    FirstEvent = CheckedEvent
                    CheckedEventDate = PlannerEventDate
                    CheckedEvent = ""
                    PlannerEventDate = ""
                End If

            Loop Until FirstEvent = CheckedEvent AndAlso PlannerEventDate = CheckedEventDate

            ExceptionUtils.ThrowEx(New EAException(ExitCodes.FindEventFailure, "Failed To Find Event : " + EventName))

        Finally
            _iex.ForceHideFailure()
        End Try
    End Sub

    ''' <summary>
    '''    Moving To Next Event In Planner X Times
    ''' </summary>
    ''' <param name="times">Optional Parameter Default = 1 : X Events To Move On Planner</param>
    ''' <remarks>
    ''' Possible Error Codes:
    ''' <para>300 - NavigationFailure</para> 
    ''' <para>302 - EmptyEpgInfoFailure</para> 
    ''' <para>304 - IRVerificationFailure</para> 
    ''' </remarks>
    Public Overrides Sub NextEvent(Optional ByVal times As Integer = 1, Optional ByVal VerifyByDetails As Boolean = False)
        _uUI.Utils.StartHideFailures("Moving Right " + times.ToString + " Times")

        Try

            For i As Integer = 1 To times
                Dim CurrentEvent As String = ""
                Dim CurrentEventDetails As String = ""
                Dim NextEventAfterMove As String = ""
                Dim NextEventDetailsAfterMove As String = ""

                GetSelectedEventName(CurrentEvent)

                If VerifyByDetails Then
                    GetSelectedEventDetails(CurrentEventDetails)
                End If

                _uUI.Utils.SendIR("SELECT_RIGHT")

                _iex.Wait(2)
                GetSelectedEventName(NextEventAfterMove)

                If VerifyByDetails Then
                    GetSelectedEventDetails(NextEventDetailsAfterMove)
                End If


                If CurrentEvent = NextEventAfterMove AndAlso CurrentEventDetails = NextEventDetailsAfterMove Then
                    ExceptionUtils.ThrowEx(New EAException(ExitCodes.NavigationFailure, "Failed To Move To Next Event On Planner"))
                End If
            Next

        Finally
            _iex.ForceHideFailure()
        End Try

    End Sub

    ''' <summary>
    '''    Moving To Previous Event In Planner X Times
    ''' </summary>
    ''' <param name="times">Optional Parameter Default = 1 : X Events To Move On Planner</param>
    ''' <remarks>
    ''' Possible Error Codes:
    ''' <para>300 - NavigationFailure</para> 
    ''' <para>302 - EmptyEpgInfoFailure</para> 
    ''' <para>304 - IRVerificationFailure</para> 
    ''' </remarks>
    Public Overrides Sub PreviousEvent(Optional ByVal times As Integer = 1, Optional ByVal VerifyByDetails As Boolean = False)
        _uUI.Utils.StartHideFailures("Moving Left " + times.ToString + " Times")

        Try
            For i As Integer = 1 To times
                Dim CurrentEvent As String = ""
                Dim CurrentEventDetails As String = ""
                Dim PrevEventAfterMove As String = ""
                Dim PrevEventDetailsAfterMove As String = ""

                GetSelectedEventName(CurrentEvent)

                If VerifyByDetails Then
                    GetSelectedEventDetails(CurrentEventDetails)
                End If

                _uUI.Utils.SendIR("SELECT_LEFT")
                _uUI.Utils.SendIR("SELECT")
				_uUI.Utils.VerifyState("ACTION BAR", 2)
                _iex.Wait(2)
                GetSelectedEventName(PrevEventAfterMove)
                _uUI.Utils.SendIR("BACK")
				_uUI.Utils.VerifyState("MY RECORDINGS", 2)
                If VerifyByDetails Then
                    GetSelectedEventDetails(PrevEventDetailsAfterMove)
                End If

                If CurrentEvent = PrevEventAfterMove AndAlso CurrentEventDetails = PrevEventDetailsAfterMove Then
                    ExceptionUtils.ThrowEx(New EAException(ExitCodes.NavigationFailure, "Failed To Move To Previous Event On Planner"))
                End If
            Next

        Finally
            _iex.ForceHideFailure()
        End Try

    End Sub
	 ''' <summary>
    '''   Canceling Event From Planner
    ''' </summary>
    ''' <param name="shouldSucceed">Optional Parameter Default = True : Just For The Comment Trying/Cancel</param>
    ''' <remarks>
    ''' Possible Error Codes:
    ''' <para>328 - INIFailure</para> 
    ''' <para>342 - CancelEventFailure</para> 
    ''' <para>345 - DeleteEventFailure</para> 
    ''' </remarks>
    Public Overrides Sub CancelEvent(Optional ByVal shouldSucceed As Boolean = True, Optional ByVal isSeriesEvent As Boolean = False, Optional ByVal isComplete As Boolean = False)
        Dim Milestones As String = ""

        _uUI.Utils.StartHideFailures(IIf(shouldSucceed, "", "Trying To ") + "Cancel Event From Planner")

        Try
            _uUI.Utils.EPG_Milestones_SelectMenuItem("CANCEL RECORDING")

            _uUI.Utils.SendIR("SELECT")

            If Not _uUI.Utils.VerifyState("CONFIRM RECORDING") Then
                ExceptionUtils.ThrowEx(New EAException(ExitCodes.CancelEventFailure, "Failed To Verify State Is CONFIRM DELETE"))
            End If

            _uUI.Utils.EPG_Milestones_SelectMenuItem("CONFIRM CANCEL RECORDING")

            Dim ActualLines As New ArrayList
            Milestones = _uUI.Utils.GetValueFromMilestones("CancelBooking")

            _uUI.Utils.BeginWaitForDebugMessages(Milestones, 10)

            _uUI.Utils.SendIR("SELECT")

            If Not _uUI.Utils.EndWaitForDebugMessages(Milestones, ActualLines) Then
                ExceptionUtils.ThrowEx(New EAException(ExitCodes.CancelEventFailure, "Failed To Verify CancelBooking Milestones : " + Milestones))
            End If

        Finally
            _iex.ForceHideFailure()
        End Try
    End Sub

End Class

