﻿Imports System.ComponentModel
Imports System.Linq
Public Class frmIOUDownload
    'Would be nice to have assignment upload verification and automatic grade checker
    'Requests for news feed PDF and discussion forum PDF
    'Password not saved and cannot be without using system encryption but goes against general policy so not implemented
    'Needs cancel/and cancel for smooth shutdown without crash
    'Linux: sudo apt-get install mono-complete mono-mcs monodevelop libmono-microsoft-visualbasic8.0-cil libmono-microsoft-visualbasic10.0-cil
    Public IOUCampus As String = "http://www.islamiconlineuniversity.com/campus"
    Public IOUOpenCampus As String = "http://www.islamiconlineuniversity.com/opencampus"
    Public CourseDownloadFolder As String
    Public Extensions As String() = {"pdf", "pptx", "ppt", "docx", "doc", "rtf", "xlsx", "xls", "mp3", "mp4", "flv", "epub", "txt", "png", "jpg", "exe", "zip", String.Empty}
    Public ExtensionDesc As String() = {"Portable Document Format", "PowerPoint OpenXML", "PowerPoint", "Document OpenXML", "Document", "Rich Text Format", "Excel Spreadsheet OpenXML", "Excel Spreadsheet", "Mpeg Layer 3", "Mpeg Layer 4", "Flash Video", "ePublication", "Text", "Portable Network Graphics", "Joint Photographic Experts Group", "Executable", "Zip Archive", "All Others"}

    Public Ext As Specialized.NameValueCollection
    Public Token As String
    Public UserID As String
    Public LoginCookies As Net.CookieCollection
    Private _TokenSource As Threading.CancellationTokenSource
    Private _DownloadTask As System.Threading.Tasks.Task
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
        Public Folder As String
        Public IsQuiz As Boolean
        Public Sub New()
            Text = FileName
            Status = "Pending"
            SubItems.Add(Status)
            Checked = True
        End Sub
        Public Sub UpdateStatus(Str As String)
            Status = Str
            SubItems(1).Text = Status
        End Sub
    End Class
    Public Sub PopulateCourseList()
        lbCourseList.Items.Clear()
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/webservice/rest/server.php?wstoken=" + Token + "&wsfunction=core_enrol_get_users_courses&userid=" + UserID)
        Dim Resp As Net.HttpWebResponse
        Try
            Resp = Req.GetResponse()
        Catch ex As Net.WebException
            lblError.Text = ex.Message
            Return
        End Try
        Dim MemStream As New IO.MemoryStream
        Resp.GetResponseStream().CopyTo(MemStream)
        MemStream.Seek(0, IO.SeekOrigin.Begin)
        Dim Reader As System.Xml.XmlReader
        If Resp.ContentType = "application/xml; charset=utf-8" Then
            Reader = Xml.XmlReader.Create(MemStream)
        Else 'application/json; charset=utf-8
            Reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(MemStream.ToArray(), New System.Xml.XmlDictionaryReaderQuotas())
        End If
        'Reader.Read()
        Dim XmlDoc As New Xml.XmlDocument
        Try
            XmlDoc.Load(Reader)
        Catch ex As System.Xml.XmlException
            lblError.Text = ex.Message
            Reader.Close()
            MemStream.Close()
            Resp.Close()
            Return
        End Try
        Dim CourseNodes As Xml.XmlNodeList = XmlDoc.SelectNodes("/RESPONSE/MULTIPLE/SINGLE")
        For Count = 0 To CourseNodes.Count - 1
            If Not CourseNodes(Count).SelectSingleNode("KEY[@name='fullname']/VALUE") Is Nothing And Not CourseNodes(Count).SelectSingleNode("KEY[@name='shortname']/VALUE") Is Nothing And Not CourseNodes(Count).SelectSingleNode("KEY[@name='id']/VALUE") Is Nothing Then
                lbCourseList.Items.Add(New CourseItem With {.FullName = CourseNodes(Count).SelectSingleNode("KEY[@name='fullname']/VALUE").InnerText, .ShortName = CourseNodes(Count).SelectSingleNode("KEY[@name='shortname']/VALUE").InnerText, .ID = CourseNodes(Count).SelectSingleNode("KEY[@name='id']/VALUE").InnerText})
            End If
        Next
        Reader.Close()
        MemStream.Close()
        Resp.Close()
    End Sub
    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        lblError.Text = "Logging in..."
        My.Settings.Username = txtUsername.Text
        My.Settings.UseMainCampus = rbMainCampus.Checked
        My.Settings.Save()

        'moodle_mobile_app does not have REST access only XML RPC
        'local_mobile service - additional features not tested yet
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/login/token.php?username=" + Net.WebUtility.HtmlEncode(txtUsername.Text) + "&password=" + Net.WebUtility.HtmlEncode(txtPassword.Text) + "&service=android")
        Dim Resp As Net.HttpWebResponse
        Try
            Resp = Req.GetResponse()
        Catch ex As Net.WebException
            lblError.Text = ex.Message
            Return
        End Try
        Dim MemStream As New IO.MemoryStream
        Try
            Resp.GetResponseStream().CopyTo(MemStream)
        Catch ex As IO.IOException
            lblError.Text = ex.Message
            MemStream.Close()
            Resp.Close()
            Return
        End Try
        MemStream.Seek(0, IO.SeekOrigin.Begin)
        Dim Reader As System.Xml.XmlReader
        If Resp.ContentType = "application/xml; charset=utf-8" Then
            Reader = Xml.XmlReader.Create(MemStream)
        Else 'application/json; charset=utf-8
            Reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(MemStream.ToArray(), System.Xml.XmlDictionaryReaderQuotas.Max)
        End If
        Dim XmlDoc As New Xml.XmlDocument
        Try
            XmlDoc.Load(Reader)
        Catch ex As System.Xml.XmlException
            lblError.Text = ex.Message
            Reader.Close()
            MemStream.Close()
            Resp.Close()
            Return
        End Try
        If Not XmlDoc.SelectSingleNode("/root/error") Is Nothing Then
            lblError.Text = XmlDoc.SelectSingleNode("/root/error").InnerText
        Else
            If XmlDoc.SelectSingleNode("/root/userid") Is Nothing Or XmlDoc.SelectSingleNode("/root/token") Is Nothing Then
                lblError.Text = "Cannot login - server returned bad XML document"
            Else
                UserID = XmlDoc.SelectSingleNode("/root/userid").InnerText
                Token = XmlDoc.SelectSingleNode("/root/token").InnerText
                lblError.Text = String.Empty
                PopulateCourseList()
            End If
        End If
        Reader.Close()
        MemStream.Close()
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
        Dim Resp As Net.HttpWebResponse
        Try
            Resp = Req.GetResponse()
        Catch ex As IO.IOException
            lblError.Text = ex.Message
            Return
        End Try
        Dim Stream As New IO.StreamReader(Resp.GetResponseStream())
        Dim Str As String
        Try
            Str = Stream.ReadToEnd()
        Catch ex As IO.IOException
            lblError.Text = ex.Message
            Stream.Close()
            Resp.Close()
            Return
        End Try
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
        Dim Resp As Net.HttpWebResponse
        Try
            Resp = Await Threading.Tasks.Task.Factory.FromAsync(Req.BeginGetResponse(Sub()
                                                                                     End Sub, Req), AddressOf Req.EndGetResponse)
        Catch ex As Net.WebException
            lblError.Text = ex.Message
            Return
        End Try
        Dim Stream As New IO.StreamReader(Resp.GetResponseStream())
        Dim Str As String
        Try
            Str = Stream.ReadToEnd()
        Catch ex As IO.IOException
            lblError.Text = ex.Message
            Stream.Close()
            Resp.Close()
            Return
        End Try
        'No file size and modified date information so this can only be verified with HTTP headers which are usually not set
        If System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Success Then
            lvFiles.Invoke(Sub() lvFiles.Items.Add(New FileItem With {.FileName = UrlName.Replace(" ", String.Empty) + ".zip", .Folder = "LiveSessions", .FileURL = System.Text.RegularExpressions.Regex.Match(Str, "https:\/\/www\.wiziq\.com\/class\/download.aspx\?.*(?=\"")").Value}))
        ElseIf System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Success Then
            'must page through each page through next links until exhausted recursively to get all notes
            Await CrawlUrl(New Uri(Req.RequestUri.GetLeftPart(UriPartial.Path) + "\..\").GetLeftPart(UriPartial.Path) + Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(Str, "class=\""next\"" href=\""(.*)\""").Groups(1).Value), String.Empty)
        End If
        Dim Matches As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(Str, "http:\/\/www.islamiconlineuniversity.com\/(?:open)?campus/pluginfile\.php.*(?=\"".*\>(.*)\<\/a\>)")
        Dim MatchCount As Integer
        For MatchCount = 0 To Matches.Count - 1
            If clbFileFormats.GetItemChecked(If(Array.IndexOf(Extensions, IO.Path.GetExtension(Matches(MatchCount).Groups(1).Value).ToLower().TrimStart(".")) <> -1, Array.IndexOf(Extensions, IO.Path.GetExtension(Matches(MatchCount).Groups(1).Value).ToLower().TrimStart(".")), Extensions.Length - 1)) Then
                lvFiles.Invoke(Sub() lvFiles.Items.Add(New FileItem With {.FileName = Matches(MatchCount).Groups(1).Value, .Folder = "CourseNotes", .FileURL = Matches(MatchCount).Value}))
            End If
        Next
        Matches = System.Text.RegularExpressions.Regex.Matches(Str, "http:\/\/www\.islamiconlineuniversity\.com\/(?:open)?campus\/mod\/quiz\/review.php\?attempt=.*?(?=\"")")
        For MatchCount = 0 To Matches.Count - 1
            lvFiles.Invoke(Sub() lvFiles.Items.Add(New FileItem With {.FileName = UrlName.Replace(" ", String.Empty) + CStr(MatchCount) + ".html", .Folder = "ModuleQuizzes", .FileURL = Matches(MatchCount).Value, .IsQuiz = True}))
        Next
        Stream.Close()
        Resp.Close()
    End Function
    Public Async Function AddFileNodes(CourseNodes As Xml.XmlNodeList, Name As String) As Threading.Tasks.Task
        Dim Count As Integer
        For Count = 0 To CourseNodes.Count - 1
            Await AddFileNodes(CourseNodes(Count).SelectNodes("KEY/MULTIPLE/SINGLE"), If(Not CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE") Is Nothing, CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText, String.Empty))
            If Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "quiz" And cbPrintModuleTestBooklet.Checked And Not CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE") Is Nothing And Not CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE") Is Nothing Then
                Await CrawlUrl(CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE").InnerText, Net.WebUtility.HtmlDecode(CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE").InnerText).Replace("&", "+").Replace(":", "-"))
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE") Is Nothing AndAlso CourseNodes(Count).SelectSingleNode("KEY[@name='type']/VALUE").InnerText = "file" Then
                If cbModuleFiles.Checked And Not CourseNodes(Count).SelectSingleNode("KEY[@name='fileurl']/VALUE") Is Nothing And Not CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE") Is Nothing AndAlso Not CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText.EndsWith(".html") And clbFileFormats.GetItemChecked(If(Array.IndexOf(Extensions, IO.Path.GetExtension(CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText).ToLower().TrimStart(".")) <> -1, Array.IndexOf(Extensions, IO.Path.GetExtension(CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText).ToLower().TrimStart(".")), Extensions.Length - 1)) Then
                    lvFiles.Invoke(Sub() lvFiles.Items.Add(New FileItem With {.FileName = CourseNodes(Count).SelectSingleNode("KEY[@name='filename']/VALUE").InnerText, .Folder = If(Name <> String.Empty, Name.Replace(" "c, String.Empty).Replace(":"c, String.Empty), "ModuleFiles"), .TimeCreated = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(If(CourseNodes(Count).SelectSingleNode("KEY[@name='timecreated']/VALUE").InnerText Is Nothing, 0, CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='timecreated']/VALUE").InnerText))), .TimeModified = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(If(CourseNodes(Count).SelectSingleNode("KEY[@name='timemodified']/VALUE").InnerText Is Nothing, 0, CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='timemodified']/VALUE").InnerText))), .FileSize = If(CourseNodes(Count).SelectSingleNode("KEY[@name='filesize']/VALUE") Is Nothing, 0, CLng(CourseNodes(Count).SelectSingleNode("KEY[@name='filesize']/VALUE").InnerText)), .FileURL = CourseNodes(Count).SelectSingleNode("KEY[@name='fileurl']/VALUE").InnerText + "&token=" + Token}))
                End If
            ElseIf Not CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE") Is Nothing AndAlso (CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "data" And cbCourseNotes.Checked Or CourseNodes(Count).SelectSingleNode("KEY[@name='modname']/VALUE").InnerText = "wiziq" And cbLiveSessions.Checked) And Not CourseNodes(Count).SelectSingleNode("KEY[@name='url']/VALUE") Is Nothing And Not CourseNodes(Count).SelectSingleNode("KEY[@name='name']/VALUE") Is Nothing Then
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
    Public Sub SaveDLSettings()
        My.Settings.DownloadFolder = txtDownloadFolder.Text
        My.Settings.GetCourseNotes = cbCourseNotes.Checked
        My.Settings.GetLiveSessions = cbLiveSessions.Checked
        My.Settings.GetModuleFiles = cbModuleFiles.Checked
        My.Settings.GetSubfolders = cbSubfolders.Checked
        My.Settings.PrintModuleTestBooklet = cbPrintModuleTestBooklet.Checked
        For Count = 0 To Extensions.Length - 1
            Ext(Extensions(Count)) = If(clbFileFormats.GetItemChecked(Count), "1", "")
        Next
        Dim StreamObj As New IO.MemoryStream
        Dim BinForm As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        BinForm.Serialize(StreamObj, Ext)
        My.Settings.Extensions = New System.Text.ASCIIEncoding().GetString(StreamObj.ToArray())
        My.Settings.Save()
    End Sub
    Private Async Function DoDownload(Path As String, lvFileItems() As FileItem, SkipArray() As Boolean, ct As Threading.CancellationToken) As Threading.Tasks.Task
        Dim msOutput As New IO.MemoryStream()
        Dim Doc As iTextSharp.text.Document = Nothing
        Dim Writer As iTextSharp.text.pdf.PdfWriter = Nothing
        Dim Count As Integer
        For Count = 0 To lvFileItems.Length - 1
            If ct.IsCancellationRequested Then Exit For
            If Not SkipArray(Count) Then
                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Checking"))
                Dim FileReq As Net.HttpWebRequest = Net.WebRequest.Create(lvFileItems(Count).FileURL)
                FileReq.CookieContainer = New Net.CookieContainer
                FileReq.CookieContainer.Add(LoginCookies)
                Dim FileResp As Net.HttpWebResponse
                Try
                    FileResp = Await Threading.Tasks.Task.Factory.FromAsync(FileReq.BeginGetResponse(Sub()
                                                                                                     End Sub, FileReq), AddressOf FileReq.EndGetResponse)
                Catch ex As Net.WebException
                    lblError.Invoke(Sub() lblError.Text = ex.Message)
                    lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Error"))
                    Continue For
                End Try
                Dim RespStream As IO.Stream = FileResp.GetResponseStream()
                If lvFileItems(Count).IsQuiz Then
                    If Doc Is Nothing Then
                        Doc = New iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30, 30, 30, 30)
                        Writer = iTextSharp.text.pdf.PdfWriter.GetInstance(Doc, msOutput)
                        Writer.CloseStream = False
                        Doc.Open()
                    End If
                    lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Downloading and Fixing HTML"))
                    Dim XHtmlFix As String
                    Try
                        XHtmlFix = NSoup.Parse.Parser.HtmlParser.ParseInput(New IO.StreamReader(RespStream).ReadToEnd(), FileResp.ResponseUri.AbsoluteUri).Html()
                    Catch ex As IO.IOException
                        lblError.Invoke(Sub() lblError.Text = ex.Message)
                        lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Error"))
                        RespStream.Close()
                        FileResp.Close()
                        Continue For
                    End Try
                    lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Converting to PDF"))
                    'remove problematic style sheet that causes iTextSharp to crash
                    XHtmlFix = System.Text.RegularExpressions.Regex.Replace(XHtmlFix, "\<link rel=\""stylesheet\"" type=\""text\/css\"" href=\""http:\/\/www\.islamiconlineuniversity\.com\/(?:open)?campus\/theme\/styles\.php\/_s\/(?:elegance|genesis|pioneer)\/\d*\/all\"" \/\>", String.Empty)
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
                    FixArabic(handler(If(Not rbDiploma.Checked, 3, 6)))
                    'If Not rbDiploma.Checked Then Doc.Add(handler(2))
                    Doc.Add(handler(If(Not rbDiploma.Checked, 3, 6))) 'nth element has the relevant content to eliminate headers and footers
                Else
                    'check modified/creation date
                    Dim Length As Long = 0
                    If cbSubfolders.Checked AndAlso Not IO.Directory.Exists(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty))) Then
                        IO.Directory.CreateDirectory(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty)))
                    End If
                    If IO.File.Exists(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName)) Then
                        Dim File As IO.FileStream = IO.File.Open(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName), IO.FileMode.Open)
                        Length = File.Length
                        File.Close()
                    End If
                    If IO.File.Exists(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName)) AndAlso Length <> 0 AndAlso (Length = lvFileItems(Count).FileSize Or Length = FileResp.ContentLength) AndAlso ((FileResp.LastModified <> New DateTime(0) And FileResp.LastModified.Subtract(Now).TotalSeconds <= 1) AndAlso IO.File.GetLastWriteTime(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName)) >= FileResp.LastModified Or lvFileItems(Count).TimeModified <> New DateTime(0) AndAlso IO.File.GetLastWriteTime(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName)) >= lvFileItems(Count).TimeModified) Then
                    Else
                        lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Downloading"))
                        Dim FStream As IO.FileStream = IO.File.OpenWrite(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName))
                        Dim Buf(4095) As Byte
                        Dim BytesRead As Integer
                        Try
                            BytesRead = Await RespStream.ReadAsync(Buf, 0, 4096)
                        Catch ex As IO.IOException
                            lblError.Invoke(Sub() lblError.Text = ex.Message)
                            lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Error"))
                            FStream.Close()
                            RespStream.Close()
                            FileResp.Close()
                            Continue For
                        End Try
                        Dim TotalBytes As Integer = 0
                        While BytesRead > 0
                            If ct.IsCancellationRequested Then
                                lblError.Invoke(Sub() lblError.Text = "Cancellation Requested")
                                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Cancelled"))
                                FStream.Close()
                                RespStream.Close()
                                FileResp.Close()
                                Exit For
                            End If
                            TotalBytes += BytesRead
                            If FileResp.ContentLength = 0 Then
                                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus(CStr(TotalBytes) + " bytes"))
                            Else
                                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus((TotalBytes / FileResp.ContentLength * 100).ToString("F") + "% (" + CStr(TotalBytes) + "/" + CStr(FileResp.ContentLength) + " bytes)"))
                            End If
                            Await FStream.WriteAsync(Buf, 0, BytesRead)
                            Try
                                BytesRead = Await RespStream.ReadAsync(Buf, 0, 4096)
                            Catch ex As IO.IOException
                                lblError.Invoke(Sub() lblError.Text = ex.Message)
                                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Error"))
                                FStream.Close()
                                RespStream.Close()
                                FileResp.Close()
                                Continue For
                            End Try
                        End While
                        FStream.Close()
                        If lvFileItems(Count).TimeModified <> New DateTime(0) Then
                            IO.File.SetLastWriteTime(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName), lvFileItems(Count).TimeModified)
                        ElseIf FileResp.LastModified <> New DateTime(0) And FileResp.LastModified.Subtract(Now).TotalSeconds <= 1 Then
                            IO.File.SetLastWriteTime(IO.Path.Combine(Path, If(cbSubfolders.Checked, lvFileItems(Count).Folder, String.Empty), lvFileItems(Count).FileName), FileResp.LastModified)
                        End If
                    End If
                End If
                RespStream.Close()
                FileResp.Close()
                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Complete"))
            Else
                lvFiles.Invoke(Sub() lvFileItems(Count).UpdateStatus("Skipped"))
            End If
        Next
        If Not Doc Is Nothing Then
            Doc.Close()
            If cbPrintModuleTestBooklet.Checked Then

                If cbSubfolders.Checked AndAlso Not IO.Directory.Exists(If(cbSubfolders.Checked, IO.Path.Combine(Path, "ModuleQuizzes"), Path)) Then
                    IO.Directory.CreateDirectory(If(cbSubfolders.Checked, IO.Path.Combine(Path, "ModuleQuizzes"), Path))
                End If
                Dim OutFile As IO.FileStream = IO.File.Create(IO.Path.Combine(If(cbSubfolders.Checked, IO.Path.Combine(Path, "ModuleQuizzes"), Path), "ModuleQuizBooklet.pdf"))
                msOutput.Seek(0, IO.SeekOrigin.Begin)
                OutFile.Write(msOutput.ToArray(), 0, msOutput.Length)
                OutFile.Close()
            End If
            Writer.Close()
        End If
        msOutput.Close()
    End Function
    Private Async Sub btnDownload_Click(sender As Object, e As EventArgs) Handles btnDownload.Click
        SaveDLSettings()
        If lbCourseList.SelectedIndex = -1 Then Return
        If (btnDownload.Text = "Download") Then
            btnDownload.Text = "Stop"
            Dim Path As String = IO.Path.Combine(If(txtDownloadFolder.Text = String.Empty, String.Empty, txtDownloadFolder.Text + "/"), CStr(lbCourseList.SelectedItem.ShortName).Replace(" ", String.Empty))
            If Not IO.Directory.Exists(Path) Then
                IO.Directory.CreateDirectory(Path)
            End If
            Dim lvFilesItems As New List(Of FileItem)
            Dim SkipArray As New List(Of Boolean)
            For Count As Integer = 0 To lvFiles.Items.Count - 1
                lvFilesItems.Add(CType(lvFiles.Items(Count), FileItem))
                SkipArray.Add(Not lvFiles.Items(Count).Checked)
            Next
            _TokenSource = New Threading.CancellationTokenSource
            _DownloadTask = DoDownload(Path, lvFilesItems.ToArray(), SkipArray.ToArray(), _TokenSource.Token)
            Await _DownloadTask
            btnDownload.Text = "Download"
            _TokenSource = Nothing
            _DownloadTask = Nothing
        Else
            If Not _DownloadTask Is Nothing Then
                _TokenSource.Cancel()
                Await _DownloadTask
            End If
        End If
    End Sub
    Private Sub btnSetDownloadFolder_Click(sender As Object, e As EventArgs) Handles btnSetDownloadFolder.Click
        If fbdMain.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtDownloadFolder.Text = fbdMain.SelectedPath
            SaveDLSettings()
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
        txtUsername.Text = My.Settings.Username
        rbDiploma.Checked = Not My.Settings.UseMainCampus
        rbMainCampus.Checked = My.Settings.UseMainCampus

        txtDownloadFolder.Text = My.Settings.DownloadFolder
        cbCourseNotes.Checked = My.Settings.GetCourseNotes
        cbLiveSessions.Checked = My.Settings.GetLiveSessions
        cbModuleFiles.Checked = My.Settings.GetModuleFiles
        cbSubfolders.Checked = My.Settings.GetSubfolders
        cbPrintModuleTestBooklet.Checked = My.Settings.PrintModuleTestBooklet
        lvFiles.Columns.Add("URL", lvFiles.Width * 7 \ 10)
        lvFiles.Columns.Add("Status")
        If My.Settings.Extensions <> String.Empty Then
            Try
                Ext = New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(New IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(My.Settings.Extensions.ToCharArray())))
            Catch ex As System.Runtime.Serialization.SerializationException
            Catch ex As System.NotSupportedException
            End Try
        End If
        If Ext Is Nothing Then
            Ext = New Specialized.NameValueCollection
            For Count = 0 To Extensions.Length - 1
                Ext(Extensions(Count)) = "1"
            Next
        End If
        For Count = 0 To Extensions.Length - 1
            clbFileFormats.Items.Add(Extensions(Count) + " (" + ExtensionDesc(Count) + ")")
            clbFileFormats.SetItemChecked(Count, Not String.IsNullOrEmpty(Ext(Extensions(Count))))
        Next
        If Not My.Settings.AcceptedDisclaimer AndAlso MsgBox("I promise to use this application lawfully and Islamically and never to distribute the copyrighted and confidential material of Islamic Online University (IOU) and I promise to keep the module quiz printouts private and never to share them with anyone outside the IOU administration.", MsgBoxStyle.YesNo, "IOU Respect and Integrity Disclaimer") <> MsgBoxResult.Yes Then Me.Close()
        My.Settings.AcceptedDisclaimer = True
        My.Settings.Save()
    End Sub

    Private Async Sub btnListFiles_Click(sender As Object, e As EventArgs) Handles btnListFiles.Click
        SaveDLSettings()
        If lbCourseList.SelectedIndex = -1 Then Return
        GetLoginCookies()
        'how to get a sesskey without crawling page that has the download link anyway?
        'perhaps with sesskey, it is possible to get mp4 or other video format?
        '"http://www.islamiconlineuniversity.com/opencampus/mod/wiziq/index.php?id=" + CourseID + "&sesskey=" + "&download=xhtml"
        Dim Req As Net.HttpWebRequest = Net.WebRequest.Create(If(rbDiploma.Checked, IOUOpenCampus, IOUCampus) + "/webservice/rest/server.php?wstoken=" + Token + "&wsfunction=core_course_get_contents&courseid=" + lbCourseList.SelectedItem.ID())
        lvFiles.Items.Clear()
        Dim Resp As Net.HttpWebResponse
        Try
            Resp = Await Threading.Tasks.Task.Factory.FromAsync(Req.BeginGetResponse(Sub()
                                                                                     End Sub, Req), AddressOf Req.EndGetResponse)
        Catch ex As Net.WebException
            lblError.Text = ex.Message
            Return
        End Try
        Dim MemStream As New IO.MemoryStream
        Try
            Resp.GetResponseStream().CopyTo(MemStream)
        Catch ex As IO.IOException
            lblError.Text = ex.Message
            MemStream.Close()
            Resp.Close()
            Return
        End Try
        MemStream.Seek(0, IO.SeekOrigin.Begin)
        Dim Reader As System.Xml.XmlReader
        If Resp.ContentType = "application/xml; charset=utf-8" Then
            'Dim settings As New Xml.XmlReaderSettings
            'settings.XmlResolver = Nothing
            'settings.DtdProcessing = Xml.DtdProcessing.Ignore
            'settings.CheckCharacters = False
            Reader = Xml.XmlReader.Create(New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(Linq.Enumerable.Where(System.Text.Encoding.UTF8.GetString(MemStream.ToArray()), Function(Ch As Char) Xml.XmlConvert.IsXmlChar(Ch)).ToArray())))
        Else 'application/json; charset=utf-8
            Reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(MemStream.ToArray(), New System.Xml.XmlDictionaryReaderQuotas())
        End If
        Dim XmlDoc As New Xml.XmlDocument
        Try
            XmlDoc.Load(Reader)
        Catch ex As System.Xml.XmlException
            lblError.Text = ex.Message
            Reader.Close()
            MemStream.Close()
            Resp.Close()
            Return
        End Try
        Await AddFileNodes(XmlDoc.SelectNodes("/RESPONSE/MULTIPLE/SINGLE"), String.Empty)
        Reader.Close()
        MemStream.Close()
        Resp.Close()
        Dim Path As String = IO.Path.Combine(If(txtDownloadFolder.Text = String.Empty, String.Empty, txtDownloadFolder.Text + "/"), CStr(lbCourseList.SelectedItem.ShortName).Replace(" ", String.Empty))
        If Not IO.Directory.Exists(Path) Then
            IO.Directory.CreateDirectory(Path)
        End If
    End Sub

    Private Sub lbCourseList_SelectedValueChanged(sender As Object, e As EventArgs) Handles lbCourseList.SelectedValueChanged
        lvFiles.Items.Clear()
    End Sub

    Private Async Sub frmIOUDownload_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Not _DownloadTask Is Nothing Then
            _TokenSource.Cancel()
            Await _DownloadTask
        End If
    End Sub
End Class