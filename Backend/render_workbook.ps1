$ErrorActionPreference = 'Stop'
$bookPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Assets/Tables/ClashRoyale_V5.4.xlsx'))
$outputDirectory = Join-Path $PSScriptRoot 'Validation'
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
try {
    $book = $excel.Workbooks.Open($bookPath,0,$true)
    foreach ($sheet in $book.Worksheets) {
        $lastColumn = if ($sheet.Name -in @('一级任务','二级任务','三级任务','特殊任务','奖励','战令40级')) { 'D' } elseif ($sheet.Name -in @('使用与总览','行动点与规则','任务池配额')) { 'C' } else { 'B' }
        $lastRow = [Math]::Min(11,$sheet.UsedRange.Rows.Count)
        if ($sheet.Name -in @('一级任务','二级任务','三级任务','特殊任务')) { $lastRow=[Math]::Min(8,$lastRow) }
        $sheet.PageSetup.PrintArea = "A1:$lastColumn$lastRow"
        $sheet.PageSetup.Orientation = 2
        $sheet.PageSetup.Zoom = $false
        $sheet.PageSetup.FitToPagesWide = 1
        $sheet.PageSetup.FitToPagesTall = 1
        $sheet.PageSetup.LeftMargin = 15
        $sheet.PageSetup.RightMargin = 15
        $sheet.PageSetup.TopMargin = 15
        $sheet.PageSetup.BottomMargin = 15
        $sheet.ExportAsFixedFormat(0,(Join-Path $outputDirectory ($sheet.Name+'.pdf')))
        Write-Output $sheet.Name
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($sheet)
    }
    $book.Close($false)
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($book)
} finally { $excel.Quit(); [void][Runtime.InteropServices.Marshal]::ReleaseComObject($excel) }
