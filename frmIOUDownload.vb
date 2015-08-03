Public Class frmIOUDownload
    'Would be nice to have assignment upload verification and automatic grade checker
    Public IOUCampus As String = "http://www.islamiconlineuniversity.com/campus"
    Public IOUOpenCampus As String = "http://www.islamiconlineuniversity.com/opencampus"
    Public Extensions As String = "*.pdf,*.pptx,*.ppt,*.docx,*.doc,*.rtf,*.xlsx,*.xls,*.mp3,*.mp4,*.flv"
    Public CourseDownloadFolder As String
    Public Token As String
    Public UserID As String
    Public LoginCookies As Net.CookieCollection
    Public Class UnicodeFontProvider
        Inherits iTextSharp.text.FontFactoryImp
        Public Sub New()
            iTextSharp.text.FontFactory.Register(IO.Path.Combine(Environment.GetEnvironmentVariable("windir"), "Fonts\times.ttf"))
            iTextSharp.text.FontFactory.Register(IO.Path.Combine(Environment.GetEnvironmentVariable("windir"), "Fonts\trado.ttf"))
        End Sub
        Public Overrides Function GetFont(fontname As String, encoding As String, embedded As Boolean, size As Single, style As Integer, color As iTextSharp.text.BaseColor) As iTextSharp.text.Font
            If String.IsNullOrWhiteSpace(fontname) Then Return New iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.UNDEFINED, size, style, color)
            Return iTextSharp.text.FontFactory.GetFont(fontname, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED, size, style, color)
        End Function
    End Class

    Structure CourseItem
        Public ID As String
        Public ShortName As String
        Public FullName As String
        Public Overrides Function ToString() As String
            Return FullName
        End Function
    End Structure
    Class FileItem
        Inherits Windows.Forms.ListViewItem
        Public FileURL As String
        Public Property FileName As String
            Get
                Return Text
            End Get
            Set(value As String)
                Text = value
            End Set
        End Property
        Public TimeCreated As DateTime
        Public TimeModified As DateTime
        Public FileSize As Long
        Public Status As String
        Public IsQuiz As Boolean
        Public Sub New()
            Text = FileName
            Status = "Pending"
            SubItems.Add(Status)
        End Sub
        Public Sub UpdateStatus(Str As String)
            Status = Str
            SubItems(1).Text = Status
        End Sub
    End Class
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

    Public Async Function CrawlUrl(Url As String, UrlName As String) As Threading.Tasks.Task
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(Url)
        Req.CookieContainer = New Net.CookieContainer
        Req.CookieContainer.Add(LoginCookies)
        Dim Resp As Net.HttpWebResponse = Await Threading.Tasks.Task.Factory.FromAsync(Req.BeginGetResponse(Sub()
                                                                                                            End Sub, Req), AddressOf Req.EndGetResponse)
        Dim Stream As New IO.StreamReader(Resp.GetResponseStream())
        Dim Str As String = Stream.ReadToEnd()
        'No file size and modified date information so this can only be verified with HTTP headers which are usually not set
        If System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Success Then
            lvFiles.Items.Add(New FileItem With {.FileName = UrlName.Replace(" ", String.Empty) + ".zip", .FileURL = System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Value})
        ElseIf System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Success Then
            'must page through each page through next links until exhausted recursively to get all notes
            Await CrawlUrl(New Uri(Req.RequestUri.GetLeftPart(UriPartial.Path) + "\..\").GetLeftPart(UriPartial.Path) + Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Groups(1).Value), String.Empty)
        End If
        Dim Matches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Str, "http:\/\/www.islamiconlineuniversity.com\/(?:open)?campus/pluginfile\.php.*(?=\"".*\>(.*)\<\/a\>)")
        For MatchCount = 0 To Matches.Count - 1
            lvFiles.Items.Add(New FileItem With {.FileName = Matches(MatchCount).Groups(1).Value, .FileURL = Matches(MatchCount).Value})
        Next
        Matches = System.Text.RegularExpressions.Regex.Matches(Str, "http:\/\/www\.islamiconlineuniversity\.com\/(?:open)?campus\/mod\/quiz\/review.php\?attempt=.*?(?=\"")")
        For MatchCount = 0 To Matches.Count - 1
            lvFiles.Items.Add(New FileItem With {.FileName = UrlName.Replace(" ", String.Empty) + CStr(MatchCount) + ".html", .FileURL = Matches(MatchCount).Value, .IsQuiz = True})
        Next
    End Function
    Public Async Function AddFileNodes(CourseNodes As Xml.XmlNodeList) As Threading.Tasks.Task
        For Count = 0 To CourseNodes.Count - 1
            Await AddFileNodes(CourseNodes(Count).SelectNodes("KEY/MULTIPLE/SINGLE"))
            If Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "quiz" And cbPrintModuleTestBooklet.Checked Then
                Await CrawlUrl(CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE").InnerText, Net.WebUtility.HtmlDecode(CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText).Replace("&", "+").Replace(":", "-"))
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE").InnerText = "file" Then
                If cbModuleFiles.Checked And Not CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText.EndsWith(".html") Then
                    lvFiles.Items.Add(New FileItem With {.FileName = CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText, .TimeCreated = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='timecreated']/VALUE").InnerText)), .TimeModified = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='timemodified']/VALUE").InnerText)), .FileSize = CourseNodes(Count).SelectSingleNode("KEY[@name='filesize']/VALUE").InnerText, .FileURL = CourseNodes(Count).SelectSingleNode("KEY[@name='fileurl']/VALUE").InnerText + "&token=" + Token})
                End If
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso (CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "data" And cbCourseNotes.Checked Or CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "wiziq" And cbLiveSessions.Checked) Then
                Await CrawlUrl(CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE").InnerText, Net.WebUtility.HtmlDecode(CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText).Replace("&", "+").Replace(":", "-"))
            End If
        Next
    End Function
    Function FixArabic(Elem As iTextSharp.text.IElement) As Boolean
        If Elem.Type = iTextSharp.text.Element.DIV Then
            For Count As Integer = 0 To CType(Elem, iTextSharp.text.pdf.PdfDiv).Content.Count - 1
                If FixArabic(CType(Elem, iTextSharp.text.pdf.PdfDiv).Content(Count)) Then
                    CType(Elem, iTextSharp.text.pdf.PdfDiv).RunDirection = iTextSharp.text.pdf.PdfWriter.RUN_DIRECTION_RTL
                    'PdfDiv does not support Arabic diacritics and strange gaps appear as must use ColumnText or PdfPCell
                    'CType(Elem, iTextSharp.text.pdf.PdfDiv).ArabicOptions = iTextSharp.text.pdf.ColumnText.AR_COMPOSEDTASHKEEL
                End If
            Next
        ElseIf Elem.Type = iTextSharp.text.Element.PARAGRAPH Then
            Return System.Text.RegularExpressions.Regex.Match(CType(Elem, iTextSharp.text.Paragraph).Content, "[\p{IsArabic}\p{IsArabicPresentationForms-A}\p{IsArabicPresentationForms-B}]").Success
        End If
        Return False
    End Function
    Private Async Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
        If lbCourseList.SelectedIndex = -1 Then Return
        GetLoginCookies()
        'how to get a sesskey without crawling page that has the download link anyway?
        'perhaps with sesskey, it is possible to get mp4 or other video format?
        '"http://www.islamiconlineuniversity.com/opencampus/mod/wiziq/index.php?id=" + CourseID + "&sesskey=" + "&download=xhtml"
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/webservice/rest/server.php?wstoken=" + Token + "&wsfunction=core_course_get_contents&courseid=" + lbCourseList.SelectedItem.ID())
        lvFiles.Items.Clear()
        Dim Resp As Net.HttpWebResponse = Await Threading.Tasks.Task.Factory.FromAsync(Req.BeginGetResponse(Sub()
                                                                                                            End Sub, Req), AddressOf Req.EndGetResponse)
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
        Await AddFileNodes(XmlDoc.SelectNodes("/RESPONSE/MULTIPLE/SINGLE"))
        Reader.Close()
        Resp.Close()
        Dim Path As String = If(txtDownloadFolder.Text = String.Empty, String.Empty, txtDownloadFolder.Text + "\") + CStr(lbCourseList.SelectedItem.ShortName).Replace(" ", String.Empty)
        If Not IO.Directory.Exists(Path) Then
            IO.Directory.CreateDirectory(Path)
        End If
        Dim msOutput As New IO.MemoryStream()
        Dim Doc As iTextSharp.text.Document = Nothing
        Dim Writer As iTextSharp.text.pdf.PdfWriter = Nothing
        For Count As Integer = 0 To lvFiles.Items.Count - 1
            CType(lvFiles.Items(Count), FileItem).UpdateStatus("Checking")
            Dim FileReq As Net.HttpWebRequest = Net.WebRequest.Create(CType(lvFiles.Items(Count), FileItem).FileURL)
            FileReq.CookieContainer = New Net.CookieContainer
            FileReq.CookieContainer.Add(LoginCookies)
            Dim FileResp As Net.HttpWebResponse = Await Threading.Tasks.Task.Factory.FromAsync(FileReq.BeginGetResponse(Sub()
                                                                                                                        End Sub, FileReq), AddressOf FileReq.EndGetResponse)
            Dim RespStream As IO.Stream = FileResp.GetResponseStream()
            If CType(lvFiles.Items(Count), FileItem).IsQuiz Then
                If Doc Is Nothing Then
                    Doc = New iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30, 30, 30, 30)
                    Writer = iTextSharp.text.pdf.PdfWriter.GetInstance(Doc, msOutput)
                    Writer.CloseStream = False
                    Doc.Open()
                End If
                Dim XHtmlFix As String = NSoup.Parse.Parser.HtmlParser.ParseInput(New IO.StreamReader(RespStream).ReadToEnd(), FileResp.ResponseUri.AbsoluteUri).Html()
                'remove problematic style sheet that causes iTextSharp to crash
                XHtmlFix = System.Text.RegularExpressions.Regex.Replace(XHtmlFix, "\<link rel=\""stylesheet\"" type=\""text\/css\"" href=\""http:\/\/www\.islamiconlineuniversity\.com\/(?:open)?campus\/theme\/styles\.php\/_s\/(?:elegance|genesis)\/\d*\/all\"" \/\>", String.Empty)
                'Dim XHtmlStream As New IO.MemoryStream(System.Text.Encoding.GetEncoding(FileResp.CharacterSet).GetBytes(XHtmlFix))
                'XHtmlStream.Seek(0, IO.SeekOrigin.Begin)
                'iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(Writer, Doc, XHtmlStream, System.Text.Encoding.GetEncoding(FileResp.CharacterSet))
                'XHtmlStream.Close()
                'need to take the following snippet adapted from XML parser for Unicode fonts to support Arabic
                Dim cssResolver As New iTextSharp.tool.xml.css.StyleAttrCSSResolver
                Dim cssAppliers As New iTextSharp.tool.xml.html.CssAppliersImpl(New UnicodeFontProvider())
                Dim hpc As New iTextSharp.tool.xml.pipeline.html.HtmlPipelineContext(cssAppliers)
                hpc.CharSet(System.Text.Encoding.UTF8)
                hpc.SetTagFactory(iTextSharp.tool.xml.html.Tags.GetHtmlTagProcessorFactory)
                hpc.AutoBookmark(False)
                Dim handler As New iTextSharp.tool.xml.ElementList
                Dim [next] As New iTextSharp.tool.xml.pipeline.end.ElementHandlerPipeline(handler, Nothing)
                Dim pipeline2 As New iTextSharp.tool.xml.pipeline.html.HtmlPipeline(hpc, [next])
                Dim pipeline As New iTextSharp.tool.xml.pipeline.css.CssResolverPipeline(cssResolver, pipeline2)
                Dim listener As New iTextSharp.tool.xml.XMLWorker(pipeline, True)
                Dim XMLParser As New iTextSharp.tool.xml.parser.XMLParser(listener)
                XMLParser.Parse(New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(XHtmlFix)))
                FixArabic(handler(5))
                Doc.Add(handler(2))
                Doc.Add(handler(5)) '5th element has the relevant content to eliminate headers and footers
            Else
                'check modified/creation date
                Dim Length As Long = 0
                If IO.File.Exists(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName) Then
                    Dim File As IO.FileStream = IO.File.Open(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName, IO.FileMode.Open)
                    Length = File.Length
                    File.Close()
                End If
                If IO.File.Exists(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName) AndAlso Length <> 0 AndAlso (Length = CType(lvFiles.Items(Count), FileItem).FileSize Or Length = FileResp.ContentLength) AndAlso ((FileResp.LastModified <> New DateTime(0) And FileResp.LastModified.Subtract(Now).TotalSeconds <= 1) AndAlso IO.File.GetLastWriteTime(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName) >= FileResp.LastModified Or CType(lvFiles.Items(Count), FileItem).TimeModified <> New DateTime(0) AndAlso IO.File.GetLastWriteTime(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName) >= CType(lvFiles.Items(Count), FileItem).TimeModified) Then
                Else
                    CType(lvFiles.Items(Count), FileItem).UpdateStatus("Downloading")
                    Dim FStream As IO.FileStream = IO.File.OpenWrite(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName)
                    Dim Buf(4095) As Byte
                    Dim BytesRead As Integer = Await RespStream.ReadAsync(Buf, 0, 4096)
                    While BytesRead > 0
                        Await FStream.WriteAsync(Buf, 0, BytesRead)
                        BytesRead = Await RespStream.ReadAsync(Buf, 0, 4096)
                    End While
                    FStream.Close()
                    If CType(lvFiles.Items(Count), FileItem).TimeModified <> New DateTime(0) Then
                        IO.File.SetLastWriteTime(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName, CType(lvFiles.Items(Count), FileItem).TimeModified)
                    ElseIf FileResp.LastModified <> New DateTime(0) And FileResp.LastModified.Subtract(Now).TotalSeconds <= 1 Then
                        IO.File.SetLastWriteTime(Path + "\" + CType(lvFiles.Items(Count), FileItem).FileName, FileResp.LastModified)
                    End If
                End If
            End If
            RespStream.Close()
            FileResp.Close()
            CType(lvFiles.Items(Count), FileItem).UpdateStatus("Complete")
        Next
        If Not Doc Is Nothing Then
            Doc.Close()
            If cbPrintModuleTestBooklet.Checked Then
                Dim OutFile As IO.FileStream = IO.File.Create(Path + "\ModuleQuizBooklet.pdf")
                msOutput.Seek(0, IO.SeekOrigin.Begin)
                OutFile.Write(msOutput.ToArray(), 0, msOutput.Length)
                OutFile.Close()
            End If
            Writer.Close()
        End If
        msOutput.Close()
    End Sub
    Private Sub btnSetDownloadFolder_Click(sender As Object, e As EventArgs) Handles btnSetDownloadFolder.Click
        If fbdMain.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtDownloadFolder.Text = fbdMain.SelectedPath
        End If
    End Sub

    Private Sub rbMainCampus_CheckedChanged(sender As Object, e As EventArgs) Handles rbMainCampus.CheckedChanged
        lbCourseList.Items.Clear()
        lvFiles.Items.Clear()
        UserID = String.Empty
        Token = String.Empty
        LoginCookies = Nothing
    End Sub

    Private Sub rbDiploma_CheckedChanged(sender As Object, e As EventArgs) Handles rbDiploma.CheckedChanged
        lbCourseList.Items.Clear()
        lvFiles.Items.Clear()
        UserID = String.Empty
        Token = String.Empty
        LoginCookies = Nothing
    End Sub

    Private Sub frmIOUDownload_Load(sender As Object, e As EventArgs) Handles Me.Load
        lvFiles.Columns.Add("URL", lvFiles.Width * 4 \ 5)
        lvFiles.Columns.Add("Status")
        If MsgBox("I promise to use this application lawfully and Islamically and never to distribute the copyrighted material of Islamic Online University (IOU) and I promise to keep the module quiz printouts private and never to share them with anyone outside the IOU administration.", MsgBoxStyle.YesNo, "IOU Respect and Integrity Disclaimer") <> MsgBoxResult.Yes Then Me.Close()
    End Sub
End Class