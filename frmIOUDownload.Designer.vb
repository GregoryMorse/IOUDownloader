<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmIOUDownload
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmIOUDownload))
        Me.rbDiploma = New System.Windows.Forms.RadioButton()
        Me.rbMainCampus = New System.Windows.Forms.RadioButton()
        Me.txtUsername = New System.Windows.Forms.TextBox()
        Me.txtPassword = New System.Windows.Forms.TextBox()
        Me.btnLogin = New System.Windows.Forms.Button()
        Me.lbCourseList = New System.Windows.Forms.ListBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lblError = New System.Windows.Forms.Label()
        Me.btnDownload = New System.Windows.Forms.Button()
        Me.fbdMain = New System.Windows.Forms.FolderBrowserDialog()
        Me.btnSetDownloadFolder = New System.Windows.Forms.Button()
        Me.txtDownloadFolder = New System.Windows.Forms.TextBox()
        Me.lvFiles = New System.Windows.Forms.ListView()
        Me.cbLiveSessions = New System.Windows.Forms.CheckBox()
        Me.cbCourseNotes = New System.Windows.Forms.CheckBox()
        Me.cbModuleFiles = New System.Windows.Forms.CheckBox()
        Me.cbPrintModuleTestBooklet = New System.Windows.Forms.CheckBox()
        Me.clbFileFormats = New System.Windows.Forms.CheckedListBox()
        Me.btnListFiles = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'rbDiploma
        '
        Me.rbDiploma.AutoSize = True
        Me.rbDiploma.Checked = True
        Me.rbDiploma.Location = New System.Drawing.Point(12, 12)
        Me.rbDiploma.Name = "rbDiploma"
        Me.rbDiploma.Size = New System.Drawing.Size(63, 17)
        Me.rbDiploma.TabIndex = 0
        Me.rbDiploma.TabStop = True
        Me.rbDiploma.Text = "Diploma"
        Me.rbDiploma.UseVisualStyleBackColor = True
        '
        'rbMainCampus
        '
        Me.rbMainCampus.AutoSize = True
        Me.rbMainCampus.Location = New System.Drawing.Point(81, 12)
        Me.rbMainCampus.Name = "rbMainCampus"
        Me.rbMainCampus.Size = New System.Drawing.Size(89, 17)
        Me.rbMainCampus.TabIndex = 1
        Me.rbMainCampus.TabStop = True
        Me.rbMainCampus.Text = "Main Campus"
        Me.rbMainCampus.UseVisualStyleBackColor = True
        '
        'txtUsername
        '
        Me.txtUsername.Location = New System.Drawing.Point(278, 12)
        Me.txtUsername.Name = "txtUsername"
        Me.txtUsername.Size = New System.Drawing.Size(133, 20)
        Me.txtUsername.TabIndex = 2
        '
        'txtPassword
        '
        Me.txtPassword.Location = New System.Drawing.Point(278, 38)
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPassword.Size = New System.Drawing.Size(133, 20)
        Me.txtPassword.TabIndex = 3
        '
        'btnLogin
        '
        Me.btnLogin.Location = New System.Drawing.Point(424, 15)
        Me.btnLogin.Name = "btnLogin"
        Me.btnLogin.Size = New System.Drawing.Size(90, 43)
        Me.btnLogin.TabIndex = 4
        Me.btnLogin.Text = "Login"
        Me.btnLogin.UseVisualStyleBackColor = True
        '
        'lbCourseList
        '
        Me.lbCourseList.FormattingEnabled = True
        Me.lbCourseList.Location = New System.Drawing.Point(12, 84)
        Me.lbCourseList.Name = "lbCourseList"
        Me.lbCourseList.Size = New System.Drawing.Size(406, 69)
        Me.lbCourseList.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(217, 15)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(55, 13)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Username"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(217, 41)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(53, 13)
        Me.Label2.TabIndex = 7
        Me.Label2.Text = "Password"
        '
        'lblError
        '
        Me.lblError.AutoSize = True
        Me.lblError.ForeColor = System.Drawing.Color.Red
        Me.lblError.Location = New System.Drawing.Point(142, 65)
        Me.lblError.Name = "lblError"
        Me.lblError.Size = New System.Drawing.Size(0, 13)
        Me.lblError.TabIndex = 8
        '
        'btnDownload
        '
        Me.btnDownload.Location = New System.Drawing.Point(394, 294)
        Me.btnDownload.Name = "btnDownload"
        Me.btnDownload.Size = New System.Drawing.Size(90, 27)
        Me.btnDownload.TabIndex = 9
        Me.btnDownload.Text = "Download"
        Me.btnDownload.UseVisualStyleBackColor = True
        '
        'btnSetDownloadFolder
        '
        Me.btnSetDownloadFolder.Location = New System.Drawing.Point(424, 148)
        Me.btnSetDownloadFolder.Name = "btnSetDownloadFolder"
        Me.btnSetDownloadFolder.Size = New System.Drawing.Size(90, 34)
        Me.btnSetDownloadFolder.TabIndex = 11
        Me.btnSetDownloadFolder.Text = "Set Download Folder"
        Me.btnSetDownloadFolder.UseVisualStyleBackColor = True
        '
        'txtDownloadFolder
        '
        Me.txtDownloadFolder.Location = New System.Drawing.Point(12, 156)
        Me.txtDownloadFolder.Name = "txtDownloadFolder"
        Me.txtDownloadFolder.Size = New System.Drawing.Size(406, 20)
        Me.txtDownloadFolder.TabIndex = 12
        '
        'lvFiles
        '
        Me.lvFiles.CheckBoxes = True
        Me.lvFiles.FullRowSelect = True
        Me.lvFiles.Location = New System.Drawing.Point(12, 209)
        Me.lvFiles.Name = "lvFiles"
        Me.lvFiles.Size = New System.Drawing.Size(340, 102)
        Me.lvFiles.TabIndex = 14
        Me.lvFiles.UseCompatibleStateImageBehavior = False
        Me.lvFiles.View = System.Windows.Forms.View.Details
        '
        'cbLiveSessions
        '
        Me.cbLiveSessions.AutoSize = True
        Me.cbLiveSessions.Checked = True
        Me.cbLiveSessions.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbLiveSessions.Location = New System.Drawing.Point(423, 186)
        Me.cbLiveSessions.Name = "cbLiveSessions"
        Me.cbLiveSessions.Size = New System.Drawing.Size(91, 17)
        Me.cbLiveSessions.TabIndex = 15
        Me.cbLiveSessions.Text = "Live Sessions"
        Me.cbLiveSessions.UseVisualStyleBackColor = True
        '
        'cbCourseNotes
        '
        Me.cbCourseNotes.AutoSize = True
        Me.cbCourseNotes.Checked = True
        Me.cbCourseNotes.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbCourseNotes.Location = New System.Drawing.Point(326, 186)
        Me.cbCourseNotes.Name = "cbCourseNotes"
        Me.cbCourseNotes.Size = New System.Drawing.Size(90, 17)
        Me.cbCourseNotes.TabIndex = 16
        Me.cbCourseNotes.Text = "Course Notes"
        Me.cbCourseNotes.UseVisualStyleBackColor = True
        '
        'cbModuleFiles
        '
        Me.cbModuleFiles.AutoSize = True
        Me.cbModuleFiles.Checked = True
        Me.cbModuleFiles.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbModuleFiles.Location = New System.Drawing.Point(220, 186)
        Me.cbModuleFiles.Name = "cbModuleFiles"
        Me.cbModuleFiles.Size = New System.Drawing.Size(85, 17)
        Me.cbModuleFiles.TabIndex = 17
        Me.cbModuleFiles.Text = "Module Files"
        Me.cbModuleFiles.UseVisualStyleBackColor = True
        '
        'cbPrintModuleTestBooklet
        '
        Me.cbPrintModuleTestBooklet.AutoSize = True
        Me.cbPrintModuleTestBooklet.Checked = True
        Me.cbPrintModuleTestBooklet.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbPrintModuleTestBooklet.Location = New System.Drawing.Point(24, 186)
        Me.cbPrintModuleTestBooklet.Name = "cbPrintModuleTestBooklet"
        Me.cbPrintModuleTestBooklet.Size = New System.Drawing.Size(148, 17)
        Me.cbPrintModuleTestBooklet.TabIndex = 18
        Me.cbPrintModuleTestBooklet.Text = "Print Module Test Booklet"
        Me.cbPrintModuleTestBooklet.UseVisualStyleBackColor = True
        '
        'clbFileFormats
        '
        Me.clbFileFormats.FormattingEnabled = True
        Me.clbFileFormats.Location = New System.Drawing.Point(358, 209)
        Me.clbFileFormats.Name = "clbFileFormats"
        Me.clbFileFormats.Size = New System.Drawing.Size(165, 79)
        Me.clbFileFormats.TabIndex = 19
        '
        'btnListFiles
        '
        Me.btnListFiles.Location = New System.Drawing.Point(424, 103)
        Me.btnListFiles.Name = "btnListFiles"
        Me.btnListFiles.Size = New System.Drawing.Size(90, 27)
        Me.btnListFiles.TabIndex = 20
        Me.btnListFiles.Text = "List Files"
        Me.btnListFiles.UseVisualStyleBackColor = True
        '
        'frmIOUDownload
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(526, 323)
        Me.Controls.Add(Me.btnListFiles)
        Me.Controls.Add(Me.clbFileFormats)
        Me.Controls.Add(Me.cbPrintModuleTestBooklet)
        Me.Controls.Add(Me.cbModuleFiles)
        Me.Controls.Add(Me.cbCourseNotes)
        Me.Controls.Add(Me.cbLiveSessions)
        Me.Controls.Add(Me.lvFiles)
        Me.Controls.Add(Me.txtDownloadFolder)
        Me.Controls.Add(Me.btnSetDownloadFolder)
        Me.Controls.Add(Me.btnDownload)
        Me.Controls.Add(Me.lblError)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lbCourseList)
        Me.Controls.Add(Me.btnLogin)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.txtUsername)
        Me.Controls.Add(Me.rbMainCampus)
        Me.Controls.Add(Me.rbDiploma)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "frmIOUDownload"
        Me.Text = "IOU Downloader"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Friend WithEvents rbDiploma As System.Windows.Forms.RadioButton
    Friend WithEvents rbMainCampus As System.Windows.Forms.RadioButton
    Friend WithEvents txtUsername As System.Windows.Forms.TextBox
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents btnLogin As System.Windows.Forms.Button
    Friend WithEvents lbCourseList As System.Windows.Forms.ListBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lblError As System.Windows.Forms.Label
    Friend WithEvents btnDownload As System.Windows.Forms.Button
    Friend WithEvents fbdMain As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents btnSetDownloadFolder As System.Windows.Forms.Button
    Friend WithEvents txtDownloadFolder As System.Windows.Forms.TextBox
    Friend WithEvents lvFiles As System.Windows.Forms.ListView
    Friend WithEvents cbLiveSessions As System.Windows.Forms.CheckBox
    Friend WithEvents cbCourseNotes As System.Windows.Forms.CheckBox
    Friend WithEvents cbModuleFiles As System.Windows.Forms.CheckBox
    Friend WithEvents cbPrintModuleTestBooklet As System.Windows.Forms.CheckBox
    Friend WithEvents clbFileFormats As System.Windows.Forms.CheckedListBox
    Friend WithEvents btnListFiles As System.Windows.Forms.Button
End Class
