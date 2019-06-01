param(
    [string]$folder="C:\Tools\avia\test",
    [string]$specfn="C:\Tools\avia\classify.xml"
)

. .\grade.ps1

function test {
    #$d = parse_log_file C:\Tools\avia\test\classify-0028.txt
    $d = parse_log_folder C:\Tools\avia\test
    $d = merge_verizon_data $d
    $d | ConvertTo-Json -Depth 8 | Out-File "samples.json"
}

<#
$spec = load_spec
$devs = parse_log_folder C:\Tools\avia\test 
$grades = @()
foreach($d in $devs) {
    $grades += grade_score_one_device $d $spec
}
#>
$data = grade_score_by_folder C:\Tools\avia\test  .\classify.xml
$data | ConvertTo-Json | Out-File .\test.json