﻿Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes
#Region "#usings"
Imports DevExpress.XtraRichEdit
Imports DevExpress.XtraRichEdit.API.Native
Imports DevExpress.XtraRichEdit.Commands
#End Region ' #usings

Namespace Table_API_Example
	Partial Public Class MainPage
		Inherits UserControl
		Public Sub New()
			InitializeComponent()
		End Sub
			  #Region "#applystyles"
		Private Shared Sub ApplyStyles(ByVal doc As Document, ByVal _table As Table, ByVal header_Row As TableRow)
			_table.Style = doc.TableStyles("MyTableStyle")
			For Each p As Paragraph In doc.GetParagraphs(header_Row.Range)
				p.Style = doc.ParagraphStyles("MyTableHeaderStyle")
			Next p
			For Each tr As TableRow In _table.Rows
				If (Not tr.IsFirst) Then
					tr.FirstCell.BackgroundColor = Colors.Transparent
					doc.GetParagraph(tr.FirstCell.Range.Start).Style = doc.ParagraphStyles("MyTableCaptionStyle")
				End If
			Next tr
		End Sub
		#End Region ' #applystyles
		#Region "#applyborders"
		Private Shared Sub ApplyBorders(ByVal _table As Table)
			_table.Borders.InsideHorizontalBorder.LineThickness = 1
			_table.Borders.InsideHorizontalBorder.LineStyle = TableBorderLineStyle.Double
			_table.Borders.InsideVerticalBorder.LineThickness = 1
			_table.Borders.InsideVerticalBorder.LineStyle = TableBorderLineStyle.Double

			For Each _cell As TableCell In _table.Rows.First.Cells
				_cell.BackgroundColor = Colors.Transparent
				_cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None
				_cell.Borders.Top.LineStyle = TableBorderLineStyle.None
				_cell.Borders.Left.LineStyle = TableBorderLineStyle.None
				_cell.Borders.Right.LineStyle = TableBorderLineStyle.None
			Next _cell
		End Sub
		#End Region ' #applyborders

		Private Sub btnInsertTable_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim doc As Document = richEditControl1.Document
			CreateTableStyles(doc)
			' Insert a table
			Dim _table As Table = doc.Tables.Add(doc.Selection.Start, 8, 8, AutoFitBehaviorType.AutoFitToWindow)
			' Start table modification
			_table.BeginUpdate()
			' Insert multiplication values
			IterateCells(_table)
			' Insert a column to the left for row numbering 
			doc.Selection = _table.Rows(1).FirstCell.Range
			Dim cmd As New InsertTableColumnToTheLeftCommand(richEditControl1)
			cmd.Execute()
			_table.ForEachRow(New TableRowProcessorDelegate(AddressOf InsertRowNumber))
			' Insert a row for column captions
			Dim column_Captions_Row As TableRow = _table.Rows.InsertBefore(0)
			InsertColumnNumbers(column_Captions_Row)
			' Insert a header row
			Dim header_Row As TableRow = InsertHeader(_table, "Multiplication Table")
			' Finalize formatting
			ApplyBorders(_table)
			ApplyStyles(doc, _table, header_Row)
			' End table modification
			_table.EndUpdate()
		End Sub
		#Region "#createtablestyles"
		Private Sub CreateTableStyles(ByVal doc As Document)
			Dim tStyleMain As TableStyle = doc.TableStyles.CreateNew()
			tStyleMain.AllCaps = True
			tStyleMain.FontName = "Courier New"
			tStyleMain.FontSize = 10
			tStyleMain.Alignment = ParagraphAlignment.Center
			tStyleMain.Name = "MyTableStyle"
			Dim tableStyleCellProperties As TableCellPropertiesBase = CType(tStyleMain, TableCellPropertiesBase)
			tableStyleCellProperties.CellBackgroundColor = Colors.Yellow
			doc.TableStyles.Add(tStyleMain)

			Dim style_Header As ParagraphStyle = doc.ParagraphStyles.CreateNew()
			style_Header.FontName = "Comic Sans"
			style_Header.FontSize = 18
			style_Header.ForeColor = Colors.Blue
			style_Header.Bold = True
			style_Header.Name = "MyTableHeaderStyle"
			doc.ParagraphStyles.Add(style_Header)

			Dim style_Caption As ParagraphStyle = doc.ParagraphStyles.CreateNew()
			style_Caption.FontName = "Comic Sans"
			style_Caption.FontSize = 14
			style_Caption.ForeColor = Colors.Red
			style_Caption.Bold = True
			style_Caption.Name = "MyTableCaptionStyle"
			doc.ParagraphStyles.Add(style_Caption)
		End Sub
		#End Region ' #createtablestyles
		#Region "#celldelegate"
		Private Sub IterateCells(ByVal _table As Table)
			_table.BeginUpdate()
			_table.ForEachCell(New TableCellProcessorDelegate(AddressOf MakeMultiplicationCell))
			_table.EndUpdate()
		End Sub
		Private Sub MakeMultiplicationCell(ByVal cell As TableCell, ByVal i As Integer, ByVal j As Integer)
			richEditControl1.Document.InsertText((cell).Range.Start, String.Format("{0}*{1} is {2}", i + 2, j + 2, (i + 2) * (j + 2)))
		End Sub
		#End Region ' #celldelegate
		#Region "#insertcolumnnumbers"
		Private Sub InsertColumnNumbers(ByVal row As TableRow)
			For Each _cell As TableCell In row.Cells
				If _cell.Index = 0 Then
					Continue For
				End If
				_cell.BackgroundColor = Colors.Transparent
				Dim doc As SubDocument = _cell.Range.BeginUpdateDocument()
				Dim range As DocumentRange = doc.InsertSingleLineText(_cell.Range.Start, String.Format("{0}", _cell.Index + 1))
				Dim cp As CharacterProperties = doc.BeginUpdateCharacters(range)
				cp.Bold = True
				cp.FontName = "Comic Sans"
				cp.FontSize = 12
				cp.ForeColor = Colors.Red
				doc.EndUpdateCharacters(cp)
				range.EndUpdateDocument(doc)
			Next _cell
		End Sub
		#End Region ' #insertcolumnnumbers
		#Region "#insertheader"
		Private Function InsertHeader(ByVal _table As Table, ByVal caption As String) As TableRow
			_table.BeginUpdate()
			Dim row As TableRow = _table.Rows.InsertBefore(0)
			_table.MergeCells(row.FirstCell, row.LastCell)
			Dim doc As SubDocument = _table.Range.BeginUpdateDocument()
			Dim header_Range As DocumentRange = doc.InsertText(row.FirstCell.Range.Start, caption)
			_table.Range.EndUpdateDocument(doc)
			_table.EndUpdate()
			Return row
		End Function
		#End Region ' #insertheader
		#Region "#insertrownumber"
		Private Sub InsertRowNumber(ByVal row As TableRow, ByVal rowNumber As Integer)
			Dim doc As SubDocument = row.FirstCell.Range.BeginUpdateDocument()
			Dim range As DocumentRange = doc.InsertText(row.FirstCell.Range.Start, String.Format("{0}", rowNumber + 2))
			range.EndUpdateDocument(doc)
		End Sub
		#End Region ' #insertrownumber

	End Class
End Namespace
