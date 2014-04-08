Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging

Public Class Form1

	''' <summary>
	''' ノードの直径。
	''' </summary>
	Private Const diameter = 80.0F

	''' <summary>
	'''  ノード情報。
	''' </summary>
	Private Structure Node
		''' <summary>
		''' X座標。
		''' </summary>
		Public Property X As Integer

		''' <summary>
		''' Y座標。
		''' </summary>
		Public Property Y As Integer

		''' <summary>
		''' 色。
		''' </summary>
		Public Property Color As Color

		''' <summary>
		''' 表示テキスト。
		''' </summary>
		Public Property Text As String
	End Structure

	''' <summary>
	''' ノードリスト。
	''' </summary>
	Private nodes As New List(Of Node)

	''' <summary>
	''' リンク情報。
	''' </summary>
	Private Structure Link
		''' <summary>
		''' 始点ノード。
		''' </summary>
		Public Property Souce As Node

		''' <summary>
		''' 終点ノード。
		''' </summary>
		Public Property Destination As Node

		''' <summary>
		''' リンクの太さ。
		''' </summary>
		Public Property Weight As Single

		Public Sub New(source As Node, destination As Node, weight As Single)
			Me.Souce = source
			Me.Destination = destination
			Me.Weight = weight
		End Sub
	End Structure

	''' <summary>
	''' 選択された箇所。
	''' </summary>
	Private selectedPoint As Point? = Nothing

	Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
		' ノードリスト作成
		nodes.Add(New Node With {.Text = "Ａ", .Color = Color.Gray, .X = 50, .Y = 50})
		nodes.Add(New Node With {.Text = "Ｂ", .Color = Color.Cyan, .X = 125, .Y = 250})
		nodes.Add(New Node With {.Text = "Ｃ", .Color = Color.Orange, .X = 200, .Y = 50})

		' リンクリスト作成
		Dim links As New List(Of Link)
		links.Add(New Link(nodes(0), nodes(1), 5.0F))
		links.Add(New Link(nodes(2), nodes(1), 3.0F))
		links.Add(New Link(nodes(2), nodes(0), 1.0F))

		' 描画キャンバス作成
		Dim canvas As New Bitmap(PictureBox1.Width, PictureBox1.Height)

		' 描画開始
		Using g = Graphics.FromImage(canvas)

			' アンチエイリアスを有効にする
			g.SmoothingMode = SmoothingMode.AntiAlias

			' 白で塗りつぶす
			g.Clear(Color.White)

			' ノードを描画する
			For Each node In nodes
				Using pen As New Pen(Brushes.Black, 3.0F), brush As New SolidBrush(node.Color)
					' 円で塗りつぶし
					g.DrawEllipse(pen, node.X - diameter / 2, node.Y - diameter / 2, diameter, diameter)
					' 枠線描画
					g.FillEllipse(brush, node.X - diameter / 2, node.Y - diameter / 2, diameter, diameter)

					' テキスト描画
					Dim sf = New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}
					g.DrawString(node.Text, Me.Font, Brushes.Black, New Rectangle(node.X - diameter / 2, node.Y - 20, diameter, 40), sf)
				End Using
			Next

			' リンクを描画する
			For Each link In links
				Dim source = link.Souce
				Dim destination = link.Destination

				' 始点と終点を決定する
				Dim start_x As Integer
				Dim start_y As Integer
				Dim end_x As Integer
				Dim end_y As Integer

				If source.Y < destination.Y Then
					start_x = source.X
					start_y = source.Y + diameter / 2
					end_x = destination.X
					end_y = destination.Y - diameter / 2

				ElseIf source.Y = destination.Y Then
					If source.X < destination.X Then
						start_x = source.X + diameter / 2
						end_x = destination.X - diameter / 2
					Else
						start_x = source.X - diameter / 2
						end_x = destination.X + diameter / 2
					End If
					start_y = source.Y
					end_y = start_y
				End If

				' 矢印を描画する
				Using pen As New Pen(Brushes.Indigo, link.Weight) With {.CustomEndCap = New AdjustableArrowCap(3, 3)}
					g.DrawLine(pen, New Point(start_x, start_y), New Point(end_x, end_y))
				End Using
			Next

			' 描画終了
			g.Dispose()
		End Using

		' キャンバスを表示する
		PictureBox1.Image = canvas
	End Sub

	''' <summary>
	''' クリックされた箇所を更新します。
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
		selectedPoint = New Point(e.X, e.Y)
		' 画像を再描画し、クリックされたかどうか判定する処理を動かす
		PictureBox1.Refresh()
	End Sub

	''' <summary>
	''' ピクチャーボックスを描画します。
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint
		If Not selectedPoint.HasValue Then
			' ノードが選択されていなければ中断
			Return
		End If

		' クリックされたノードを探索
		For Each node In nodes
			Using myPath As New GraphicsPath()
				' ノードを表示している領域を作成
				myPath.AddEllipse(node.X - diameter / 2, node.Y - diameter / 2, diameter, diameter)

				' クリックされた箇所がノードの表示領域内かどうか取得する
				Dim visible As Boolean = myPath.IsVisible(selectedPoint.Value.X, selectedPoint.Value.Y, e.Graphics)
				If visible Then
					' クリックされたと判断する
					MessageBox.Show("Node " & node.Text & " Clicked!")
					Exit For
				End If
			End Using
		Next
	End Sub

	''' <summary>
	''' 有向グラフを保存します。
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
		Using dialog As New SaveFileDialog()
			With dialog
				.DefaultExt = "jpg"
				.AddExtension = True
				.RestoreDirectory = True
			End With

			Dim result = dialog.ShowDialog()
			If result = Windows.Forms.DialogResult.OK Then
				PictureBox1.Image.Save(dialog.FileName, ImageFormat.Jpeg)
			End If
		End Using
	End Sub
End Class
