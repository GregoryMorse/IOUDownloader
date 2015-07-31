Public Class frmIOUDown

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
        Public TimeModified As String
        Public Overrides Function ToString() As String
            Return FileName
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
        If System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Success Then
            lbFiles.Items.Add(New FileItem With {.FileName = Name.Replace(" ", String.Empty) + ".exe", .FileURL = System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Value})
        ElseIf System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Success Then
            CrawlUrl(New Uri(New Uri(Url).GetLeftPart(UriPartial.Path) + "\..\").GetLeftPart(UriPartial.Path) + Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Groups(1).Value), String.Empty)
        End If
        Dim Matches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Str, "http:\/\/www.islamiconlineuniversity.com\/(?:open)?campus/pluginfile\.php.*(?=\"".*\>(.*)\<\/a\>)")
        For MatchCount = 0 To Matches.Count - 1
            lbFiles.Items.Add(New FileItem With {.FileName = Matches(MatchCount).Groups(1).Value, .FileURL = Matches(MatchCount).Value})
        Next
    End Sub
    Public Sub AddFileNodes(CourseNodes As Xml.XmlNodeList)
        For Count = 0 To CourseNodes.Count - 1
            AddFileNodes(CourseNodes(Count).SelectNodes("KEY/MULTIPLE/SINGLE"))
            If Not CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE").InnerText = "file" Then
                If Not CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText.EndsWith(".html") Then
                    lbFiles.Items.Add(New FileItem With {.FileName = CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText, .FileURL = CourseNodes(Count).SelectSingleNode("KEY[@name='fileurl']/VALUE").InnerText})
                End If
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso (CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "data" Or CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "wiziq") Then
                CrawlUrl(CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE").InnerText, CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText)
            End If
        Next
    End Sub
    Private Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
        GetLoginCookies()
        'how to get a sesskey?
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
        AddFileNodes(XmlDoc.SelectNodes("/RESPONSE/MULTIPLE/SINGLE"))
        Reader.Close()
        Resp.Close()
        Dim Path As String = If(txtDownloadFolder.Text = String.Empty, String.Empty, txtDownloadFolder.Text + "\") + CStr(lbCourseList.SelectedItem.ShortName).Replace(" ", String.Empty)
        If Not IO.Directory.Exists(Path) Then
            IO.Directory.CreateDirectory(Path)
        End If
        For Count As Integer = 0 To lbFiles.Items.Count - 1
            Dim FileReq As Net.HttpWebRequest = Net.WebRequest.Create(lbFiles.Items(Count).FileURL)
            Dim FileResp As Net.WebResponse = FileReq.GetResponse()
            Dim RespStream As IO.Stream = FileResp.GetResponseStream()
            'check modified/creation date
            Dim FStream As IO.FileStream = IO.File.OpenWrite(Path + "\" + lbFiles.Items(Count).FileName)
            Dim Buf(4095) As Byte
            Dim BytesRead As Integer = RespStream.Read(Buf, 0, 4096)
            While BytesRead > 0
                FStream.Write(Buf, 0, BytesRead)
                BytesRead = RespStream.Read(Buf, 0, 4096)
            End While
            RespStream.Close()
            FStream.Close()
            FileResp.Close()
        Next
    End Sub

    Private Sub btnSetDownloadFolder_Click(sender As Object, e As EventArgs) Handles btnSetDownloadFolder.Click
        If fbdMain.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtDownloadFolder.Text = fbdMain.SelectedPath
        End If
    End Sub
End Class