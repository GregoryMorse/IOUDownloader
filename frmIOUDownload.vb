Public Class frmIOUDownload
    'Needs asynchronous coding of downloading portions
    'Needs proper testing and handling for size/date redownloading management code
    'Would be nice to have assignment upload verification and automatic grade checker
    Public IOUCampus As String = "http://www.islamiconlineuniversity.com/campus"
    Public IOUOpenCampus As String = "http://www.islamiconlineuniversity.com/opencampus"
    Public Extensions As String = "*.pdf,*.pptx,*.ppt,*.docx,*.doc,*.rtf,*.xlsx,*.xls,*.mp3,*.mp4,*.flv"
    Public CourseDownloadFolder As String
    Public Token As String
    Public UserID As String
    Public LoginCookies As Net.CookieCollection
    Structure CourseItem
        Public ID As String
        Public ShortName As String
        Public FullName As String
        Public Overrides Function ToString() As String
            Return FullName
        End Function
    End Structure
    Structure FileItem
        Public FileURL As String
        Public FileName As String
        Public TimeCreated As DateTime
        Public TimeModified As DateTime
        Public FileSize As Long
        Public Status As String
        Public Sub UpdateStatus(Str As String)
            Status = Str
        End Sub
        Public Overrides Function ToString() As String
            Return If(Status <> String.Empty, "Status - ", String.Empty) + FileName
        End Function
    End Structure
    Public Sub PopulateCourseList()
        lbCourseList.Items.Clear()
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/webservice/rest/server.php?wstoken=" + Token + "&wsfunction=core_enrol_get_users_courses&userid=" + UserID)
        Dim Resp As Net.HttpWebResponse = Req.GetResponse()
        Dim MemStream As New IO.MemoryStream
        Resp.GetResponseStream().CopyTo(MemStream)
        MemStream.Seek(0, IO.SeekOrigin.Begin)
        Dim Reader As System.Xml.XmlReader
        If Resp.ContentType = "application/xml; charset=utf-8" Then
            Reader = Xml.XmlReader.Create(MemStream)
        Else 'application/json; charset=utf-8
            Reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(MemStream.ToArray(), New System.Xml.XmlDictionaryReaderQuotas())
        End If
        Reader.Read()
        Dim XmlDoc As New Xml.XmlDocument
        XmlDoc.Load(Reader)
        Dim CourseNodes As Xml.XmlNodeList = XmlDoc.SelectNodes("/RESPONSE/MULTIPLE/SINGLE")
        For Count = 0 To CourseNodes.Count - 1
            lbCourseList.Items.Add(New CourseItem With {.FullName = CourseNodes(Count).SelectSingleNode("KEY[@name='fullname']/VALUE").InnerText, .ShortName = CourseNodes(Count).SelectSingleNode("KEY[@name='shortname']/VALUE").InnerText, .ID = CourseNodes(Count).SelectSingleNode("KEY[@name='id']/VALUE").InnerText})
        Next
        Reader.Close()
        Resp.Close()
    End Sub
    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        lblError.Text = "Logging in..."
        'moodle_mobile_app does not have REST access only XML RPC
        'local_mobile service - additional features not tested yet
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/login/token.php?username=" + Net.WebUtility.HtmlEncode(txtUsername.Text) + "&password=" + Net.WebUtility.HtmlEncode(txtPassword.Text) + "&service=android")
        Dim Resp As Net.HttpWebResponse = Req.GetResponse()
        Dim MemStream As New IO.MemoryStream
        Resp.GetResponseStream().CopyTo(MemStream)
        MemStream.Seek(0, IO.SeekOrigin.Begin)
        Dim Reader As System.Xml.XmlReader
        If Resp.ContentType = "application/xml; charset=utf-8" Then
            Reader = Xml.XmlReader.Create(MemStream)
        Else 'application/json; charset=utf-8
            Reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(MemStream.ToArray(), System.Xml.XmlDictionaryReaderQuotas.Max)
        End If
        Dim XmlDoc As New Xml.XmlDocument
        XmlDoc.Load(Reader)
        If Not XmlDoc.SelectSingleNode("/root/error") Is Nothing Then
            lblError.Text = XmlDoc.SelectSingleNode("/root/error").InnerText
        Else
            UserID = XmlDoc.SelectSingleNode("/root/userid").InnerText
            Token = XmlDoc.SelectSingleNode("/root/token").InnerText
            lblError.Text = String.Empty
            PopulateCourseList()
        End If
        Reader.Close()
        Resp.Close()
    End Sub
    Public Sub GetLoginCookies()
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create("http://www.islamiconlineuniversity.com/" + If(rbDiploma.Checked, "opencampus", "campus") + "/login/index.php")
        Req.AllowAutoRedirect = False
        Dim PostData As String = "username=" + Net.WebUtility.HtmlEncode(txtUsername.Text) + "&password=" + Net.WebUtility.HtmlEncode(txtPassword.Text) + "&rememberusername=1"
        Dim Buf As Byte() = System.Text.Encoding.UTF8.GetBytes(PostData)
        Req.Method = "POST"
        Req.ContentLength = Buf.Length
        Req.ContentType = "application/x-www-form-urlencoded"
        Dim ReqStream As IO.Stream = Req.GetRequestStream()
        ReqStream.Write(Buf, 0, Buf.Length)
        ReqStream.Close()
        Dim Resp As Net.HttpWebResponse = Req.GetResponse()
        Dim Stream As New IO.StreamReader(Resp.GetResponseStream())
        Dim Str As String = Stream.ReadToEnd()
        If Resp.Cookies.Count = 0 Then
            LoginCookies = New Net.CookieCollection
            For Count As Integer = 0 To Resp.Headers.Count - 1
                If Resp.Headers.GetKey(Count) = "Set-Cookie" Then
                    Dim Matches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Resp.Headers.Get(Count), "(.+?)=(.+?);(?: expires=.*?;)?(?: path=(.+?))?.*?(,|$)")
                    For MatchCount = 0 To Matches.Count - 1
                        LoginCookies.Add(New Net.Cookie(Matches(MatchCount).Groups(1).Value, Matches(MatchCount).Groups(2).Value, Matches(MatchCount).Groups(3).Value, Resp.ResponseUri.Host))
                    Next
                End If
            Next
        Else
            LoginCookies = Resp.Cookies
        End If
        Stream.Close()
        Resp.Close()
    End Sub
    Public Sub CrawlUrl(Url As String, Name As String)
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(Url)
        Req.CookieContainer = New Net.CookieContainer
        Req.CookieContainer.Add(LoginCookies)
        Dim Resp As Net.HttpWebResponse = Req.GetResponse()
        Dim Stream As New IO.StreamReader(Resp.GetResponseStream())
        Dim Str As String = Stream.ReadToEnd()
        'No file size and modified date information so this can only be verified with HTTP headers which are usually not set
        If System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Success Then
            lbFiles.Items.Add(New FileItem With {.FileName = Name.Replace(" ", String.Empty) + If(rbDiploma.Checked, ".zip", ".exe"), .FileURL = System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Value})
        ElseIf System.Text.RegularExpressions.Regex.Match(Str, "http:\/\/www\.islamiconlineuniversity\.com\/campus\/mod\/quiz\/review.php\?attempt=.*?(?=\"")").Success Then
            lbFiles.Items.Add(New FileItem With {.FileName = Name.Replace(" ", String.Empty) + ".html", .FileURL = System.Text.RegularExpressions.Regex.Match(Str, "http:\/\/www\.islamiconlineuniversity\.com\/campus\/mod\/quiz\/review.php\?attempt=.*?(?=\"")").Value})
        ElseIf System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Success Then
            'must page through each page through next links until exhausted recursively to get all notes
            CrawlUrl(New Uri(New Uri(Url).GetLeftPart(UriPartial.Path) + "\..\").GetLeftPart(UriPartial.Path) + Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Groups(1).Value), String.Empty)
        End If
        Dim Matches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Str, "http:\/\/www.islamiconlineuniversity.com\/(?:open)?campus/pluginfile\.php.*(?=\"".*\>(.*)\<\/a\>)")
        For MatchCount = 0 To Matches.Count - 1
            lbFiles.Items.Add(New FileItem With {.FileName = Matches(MatchCount).Groups(1).Value, .FileURL = Matches(MatchCount).Value})
        Next
    End Sub
    Public Sub AddFileNodes(CourseNodes As Xml.XmlNodeList, bQuizOnly As Boolean)
        For Count = 0 To CourseNodes.Count - 1
            AddFileNodes(CourseNodes(Count).SelectNodes("KEY/MULTIPLE/SINGLE"), bQuizOnly)
            If bQuizOnly Then
                If Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "quiz" Then
                    CrawlUrl(CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE").InnerText, CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText)
                End If
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE").InnerText = "file" Then
                If Not CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText.EndsWith(".html") Then
                    lbFiles.Items.Add(New FileItem With {.FileName = CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText, .TimeCreated = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='timecreated']/VALUE").InnerText)), .TimeModified = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='timemodified']/VALUE").InnerText)), .FileSize = CourseNodes(Count).SelectSingleNode("KEY[@name='filesize']/VALUE").InnerText, .FileURL = CourseNodes(Count).SelectSingleNode("KEY[@name='fileurl']/VALUE").InnerText + "&token=" + Token})
                End If
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso (CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "data" Or CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "wiziq") Then
                CrawlUrl(CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE").InnerText, CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText)
            End If
        Next
    End Sub
    Sub DoDownload(bQuizOnly As Boolean)
        If lbCourseList.SelectedIndex = -1 Then Return
        GetLoginCookies()
        'how to get a sesskey without crawling page that has the download link anyway?
        'perhaps with sesskey, it is possible to get mp4 or other video format?
        '"http://www.islamiconlineuniversity.com/opencampus/mod/wiziq/index.php?id=" + CourseID + "&sesskey=" + "&download=xhtml"
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/webservice/rest/server.php?wstoken=" + Token + "&wsfunction=core_course_get_contents&courseid=" + lbCourseList.SelectedItem.ID())
        lbFiles.Items.Clear()
        Dim Resp As Net.HttpWebResponse = Req.GetResponse()
        Dim MemStream As New IO.MemoryStream
        Resp.GetResponseStream().CopyTo(MemStream)
        MemStream.Seek(0, IO.SeekOrigin.Begin)
        Dim Reader As System.Xml.XmlReader
        If Resp.ContentType = "application/xml; charset=utf-8" Then
            Reader = Xml.XmlReader.Create(MemStream)
        Else 'application/json; charset=utf-8
            Reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(MemStream.ToArray(), New System.Xml.XmlDictionaryReaderQuotas())
        End If
        Dim XmlDoc As New Xml.XmlDocument
        XmlDoc.Load(Reader)
        AddFileNodes(XmlDoc.SelectNodes("/RESPONSE/MULTIPLE/SINGLE"), bQuizOnly)
        Reader.Close()
        Resp.Close()
        Dim Path As String = If(txtDownloadFolder.Text = String.Empty, String.Empty, txtDownloadFolder.Text + "\") + CStr(lbCourseList.SelectedItem.ShortName).Replace(" ", String.Empty)
        If Not IO.Directory.Exists(Path) Then
            IO.Directory.CreateDirectory(Path)
        End If
        Dim msOutput As New IO.MemoryStream()
        Dim Doc As New iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30, 30, 30, 30)
        Dim Writer As iTextSharp.text.pdf.PdfWriter = iTextSharp.text.pdf.PdfWriter.GetInstance(Doc, msOutput)
        Writer.CloseStream = False
        Doc.Open()
        For Count As Integer = 0 To lbFiles.Items.Count - 1
            Dim FileReq As Net.HttpWebRequest = Net.WebRequest.Create(lbFiles.Items(Count).FileURL)
            FileReq.CookieContainer = New Net.CookieContainer
            FileReq.CookieContainer.Add(LoginCookies)
            Dim FileResp As Net.HttpWebResponse = FileReq.GetResponse()
            Dim RespStream As IO.Stream = FileResp.GetResponseStream()
            If bQuizOnly Then
                Dim XHtmlFix As String = NSoup.Parse.Parser.HtmlParser.ParseInput(New IO.StreamReader(RespStream).ReadToEnd(), FileResp.ResponseUri.AbsoluteUri).Html()
                'remove problematic style sheet that causes iTextSharp to crash
                XHtmlFix = System.Text.RegularExpressions.Regex.Replace(XHtmlFix, "\<link rel=\""stylesheet\"" type=\""text\/css\"" href=\""http:\/\/www\.islamiconlineuniversity\.com\/campus\/theme\/styles\.php\/_s\/elegance\/\d*\/all\"" \/\>", String.Empty)
                'Dim XHtmlStream As New IO.MemoryStream(System.Text.Encoding.GetEncoding(FileResp.CharacterSet).GetBytes(XHtmlFix))
                'XHtmlStream.Seek(0, IO.SeekOrigin.Begin)
                'iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(Writer, Doc, XHtmlStream, System.Text.Encoding.GetEncoding(FileResp.CharacterSet))
                'XHtmlStream.Close()
                Dim ElementList As iTextSharp.tool.xml.ElementList = iTextSharp.tool.xml.XMLWorkerHelper.ParseToElementList(XHtmlFix, String.Empty)
                Doc.Add(ElementList(5)) '5th element has the relevant content to eliminate headers and footers
            Else
                'check modified/creation date
                Dim Length As Long = 0
                If IO.File.Exists(Path + "\" + lbFiles.Items(Count).FileName) Then
                    Dim File As IO.FileStream = IO.File.Open(Path + "\" + lbFiles.Items(Count).FileName, IO.FileMode.Open)
                    Length = File.Length
                    File.Close()
                End If
                If IO.File.Exists(Path + "\" + lbFiles.Items(Count).FileName) AndAlso Length <> 0 AndAlso (Length = lbFiles.Items(Count).FileSize Or Length = Resp.ContentLength) AndAlso ((Resp.LastModified <> New DateTime(0) And Resp.LastModified.Subtract(Now).TotalSeconds >= 1) AndAlso IO.File.GetLastWriteTime(Path + "\" + lbFiles.Items(Count).FileName) >= Resp.LastModified Or lbFiles.Items(Count).TimeModified <> New DateTime(0) AndAlso IO.File.GetLastWriteTime(Path + "\" + lbFiles.Items(Count).FileName) >= lbFiles.Items(Count).TimeModified) Then
                Else
                    Dim FStream As IO.FileStream = IO.File.OpenWrite(Path + "\" + lbFiles.Items(Count).FileName)
                    Dim Buf(4095) As Byte
                    Dim BytesRead As Integer = RespStream.Read(Buf, 0, 4096)
                    While BytesRead > 0
                        FStream.Write(Buf, 0, BytesRead)
                        BytesRead = RespStream.Read(Buf, 0, 4096)
                    End While
                    FStream.Close()
                    If lbFiles.Items(Count).TimeModified <> New DateTime(0) Then
                        IO.File.SetLastWriteTime(Path + "\" + lbFiles.Items(Count).FileName, lbFiles.Items(Count).TimeModified)
                    ElseIf Resp.LastModified <> New DateTime(0) And Resp.LastModified.Subtract(Now).TotalSeconds >= 1 Then
                        IO.File.SetLastWriteTime(Path + "\" + lbFiles.Items(Count).FileName, Resp.LastModified)
                    End If
                End If
            End If
            RespStream.Close()
            FileResp.Close()
            lbFiles.Items(Count).UpdateStatus("Complete")
            lbFiles.Update()
        Next
        Doc.Close()
        If bQuizOnly Then
            Dim OutFile As IO.FileStream = IO.File.Create(Path + "\ModuleQuizBooklet.pdf")
            msOutput.Seek(0, IO.SeekOrigin.Begin)
            OutFile.Write(msOutput.ToArray(), 0, msOutput.Length)
            OutFile.Close()
        End If
        Writer.CloseStream = False
        Writer.Close()
        msOutput.Close()
    End Sub
    Private Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
        DoDownload(False)
    End Sub
    Private Sub btnSetDownloadFolder_Click(sender As Object, e As EventArgs) Handles btnSetDownloadFolder.Click
        If fbdMain.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtDownloadFolder.Text = fbdMain.SelectedPath
        End If
    End Sub

    Private Sub rbMainCampus_CheckedChanged(sender As Object, e As EventArgs) Handles rbMainCampus.CheckedChanged
        lbCourseList.Items.Clear()
        lbFiles.Items.Clear()
        UserID = String.Empty
        Token = String.Empty
        LoginCookies = Nothing
    End Sub

    Private Sub rbDiploma_CheckedChanged(sender As Object, e As EventArgs) Handles rbDiploma.CheckedChanged
        lbCourseList.Items.Clear()
        lbFiles.Items.Clear()
        UserID = String.Empty
        Token = String.Empty
        LoginCookies = Nothing
    End Sub

    Private Sub btnPrintModuleTests_Click(sender As Object, e As EventArgs) Handles btnPrintModuleTests.Click
        DoDownload(True)
    End Sub
End Class