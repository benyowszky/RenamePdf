Imports System.IO
Imports iTextSharp.text.pdf
Imports iTextSharp.text.pdf.parser

Module Module1
    Sub Main(args As String())
        ' Ellenőrizzük a bemeneti paramétereket
        If args.Length < 2 Then
            Console.WriteLine("Használat: RenamePDFs.exe <mappa_eleres> <keresett_szöveg>")
            Return
        End If

        Dim directoryPath As String = args(0)
        'Dim searchText As String = args(1)
        Dim searchText As String = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.Default.GetBytes(args(1)))


        ' Ellenőrizzük, hogy a megadott könyvtár létezik-e
        If Not Directory.Exists(directoryPath) Then
            Console.WriteLine("A megadott mappa nem létezik.")
            Return
        End If

        ' Bejárjuk az összes PDF fájlt a megadott mappában
        Dim pdfFiles As String() = Directory.GetFiles(directoryPath, "*.pdf")

        For Each file As String In pdfFiles
            Try
                Dim newName As String = ExtractTextAfterKeyword(file, searchText)
                If Not String.IsNullOrEmpty(newName) Then
                    Dim newFilePath As String = IO.Path.Combine(directoryPath, newName & ".pdf")

                    ' Ha a fájl még nem létezik ezzel a névvel, átnevezzük
                    If Not IO.File.Exists(newFilePath) Then
                        IO.File.Move(file, newFilePath)
                        Console.WriteLine($"Átnevezve: {file} -> {newFilePath}")
                    Else
                        Console.WriteLine($"Kihagyva (már létezik ilyen névvel): {newFilePath}")
                    End If
                Else
                    Console.WriteLine($"Kihagyva (nem található keresett szöveg után név): {file}")
                End If
            Catch ex As Exception
                Console.WriteLine($"Hiba történt a(z) {file} fájl feldolgozása közben: {ex.Message}")
            End Try
        Next
    End Sub

    Function ExtractTextAfterKeyword(pdfPath As String, keyword As String) As String
        Try
            Using reader As New PdfReader(pdfPath)
                For i As Integer = 1 To reader.NumberOfPages
                    Dim text As String = PdfTextExtractor.GetTextFromPage(reader, i, New SimpleTextExtractionStrategy())
                    text = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.Default.GetBytes(text)) ' UTF-8 átalakítás

                    Dim index As Integer = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase)
                    If index <> -1 Then
                        Dim remainingText As String = text.Substring(index + keyword.Length).Trim()
                        Dim words As String() = remainingText.Split(" "c, vbTab, vbCr, vbLf)
                        For Each word As String In words
                            If Not String.IsNullOrWhiteSpace(word) Then
                                ' Érvénytelen karakterek lecserélése alulvonásra
                                Dim invalidChars As Char() = System.IO.Path.GetInvalidFileNameChars()
                                Dim cleanedWord As String = invalidChars.Aggregate(word, Function(current, c) current.Replace(c, "_"))
                                Return cleanedWord
                            End If
                        Next
                    End If
                Next
            End Using
        Catch ex As Exception
            Console.WriteLine($"Hiba a PDF feldolgozásakor: {ex.Message}")
        End Try
        Return String.Empty
    End Function


End Module
