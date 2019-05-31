param(
    [string]$folder="C:\Tools\avia\test",
    [string]$specfn="C:\Tools\avia\classify.xml"
)

function no_use_1() {
    $doc = New-Object System.Xml.XmlDocument
    $doc.Load('C:\Users\qa\source\repos\chencen2000\GradeChecker\GradeChecker\bin\Debug\score.xml')
    $keys=Get-Content C:\Users\qa\source\repos\chencen2000\GradeChecker\GradeChecker\bin\Debug\data\all_spec_keys.json | ConvertFrom-Json
    foreach($a in $keys) 
    {
        $child = $doc.DocumentElement.AppendChild($doc.CreateElement($a))
        $child.AppendChild($doc.CreateTextNode("1000"))
    }
    $doc.Save('C:\Users\qa\source\repos\chencen2000\GradeChecker\GradeChecker\bin\Debug\score1.xml')
}

$vdata= Get-Content (Join-Path -Path (Split-Path -Parent $PSCommandPath) -ChildPath "verizon_db.json") | ConvertFrom-Json
$samples=@()
Get-ChildItem $folder | ForEach-Object {
    $digits=($_.BaseName | Select-String -Pattern ".+-(\d+)").Matches.Groups[1].Value
    $d = @{}
    $v = $vdata | Where-Object IMEI -Like "*$digits"
    $v.psobject.Properties | foreach { $d[$_.Name] = $_.Value }
    $flaws=@()
    $counts=@{}
    $grade=""    
    foreach($line in Get-Content $_.FullName){
        $m = $line | Select-String -Pattern "^flaw = ([\w-]+), region = ([@\w]+), surface = (\w+), sort = (\w+), length = ([\d.]+) mm, width = ([\d.]+) mm, area = ([\d.]+) mm$"
        if($m.Matches.Success){
            $f=@{
                flaw = $m.Matches.Groups[1].Value;
                region = $m.Matches.Groups[2].Value;
                surface = $m.Matches.Groups[3].Value;
                sort = $m.Matches.Groups[4].Value;
                length = $m.Matches.Groups[5].Value;
                width = $m.Matches.Groups[6].Value;
                area = $m.Matches.Groups[7].Value
            }
            $flaws +=$f
        }
    }
    $d["flaws"] = $flaws
    $d["counts"] = $counts
    $d["grade"] = $grade
}