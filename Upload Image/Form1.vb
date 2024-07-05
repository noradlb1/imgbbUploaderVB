Imports System.Net.Http
Imports System.IO
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Public Class Form1
    'الأيميل sokniyirta@gufum.com
    ' دعم السحب والإفلات
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.AllowDrop = True
    End Sub

    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
        End If
    End Sub

    Private Async Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
        Try
            Dim files() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
            If files.Length > 0 Then
                Await UploadImageAsync(files(0))
            End If
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Dim openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif"
            openFileDialog.Multiselect = True ' للسماح باختيار ملفات متعددة

            If openFileDialog.ShowDialog() = DialogResult.OK Then
                For Each file In openFileDialog.FileNames
                    Await UploadImageAsync(file)
                Next
            End If
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Async Function UploadImageAsync(imagePath As String) As Task
        Dim apiKey As String = TextBox1.Text ' أدخل مفتاح API هنا أو في TextBox
        If String.IsNullOrEmpty(apiKey) Then
            MessageBox.Show("Please enter your API key.")
            Return
        End If

        ProgressBar1.Style = ProgressBarStyle.Marquee
        StatusLabel.Text = "Uploading image..."

        Try
            Dim client As New HttpClient()
            Dim requestContent As New MultipartFormDataContent()
            requestContent.Add(New StringContent(apiKey), "key")

            If Not String.IsNullOrEmpty(imagePath) Then
                Dim imageBytes As Byte() = File.ReadAllBytes(imagePath)
                Dim base64Image As String = Convert.ToBase64String(imageBytes)
                requestContent.Add(New StringContent(base64Image), "image")
            End If

            Dim response As HttpResponseMessage = Await client.PostAsync("https://api.imgbb.com/1/upload", requestContent)
            response.EnsureSuccessStatusCode() ' This will throw an exception if the status code is not 2xx

            Dim responseContent As String = Await response.Content.ReadAsStringAsync()

            ' استخدام JObject لتحليل الاستجابة واستخراج رابط الصورة والتفاصيل الإضافية:
            Dim jsonResponse As JObject = JObject.Parse(responseContent)
            Dim imageUrl As String = jsonResponse("data")("url").ToString()
            TextBox2.Text = imageUrl ' أو Label2.Text لعرض الرابط

            ' عرض تفاصيل إضافية عن الصورة
            Dim imageDetails As String = $"Size: {jsonResponse("data")("size")} bytes{Environment.NewLine}" &
                                         $"Width: {jsonResponse("data")("width")}{Environment.NewLine}" &
                                         $"Height: {jsonResponse("data")("height")}"
            TextBox3.Text = imageDetails

            ' تحميل الصورة وعرضها في PictureBox
            Dim imageStream As Stream = Await client.GetStreamAsync(imageUrl)
            PictureBox1.Image = Image.FromStream(imageStream)


            If CheckBox1.Checked = True Then
                ' حفظ الصورة محلياً بعد التحميل
                Dim saveDialog As New SaveFileDialog()
                saveDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif"
                If saveDialog.ShowDialog() = DialogResult.OK Then
                    PictureBox1.Image.Save(saveDialog.FileName)
                End If
            End If


            StatusLabel.Text = "Image uploaded successfully."
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message)
            StatusLabel.Text = "Error uploading image."
        Finally
            ProgressBar1.Style = ProgressBarStyle.Blocks
        End Try
    End Function

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        Process.Start("https://t.me/MONSTERMCSY")
    End Sub
End Class
