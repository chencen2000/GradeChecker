param(
    [string]$folder="C:\Tools\avia\test",
    [string]$specfn="C:\Tools\avia\classify.xml"
)

. .\flaw.ps1

function FunctionName {
    param (
        $dev,
        $specs
    )
    $grades = @("A+", "A", "B", "C", "D+", "D")
    $grade_score = @{}
    foreach($g in $grades) {
        $grade_score_total = 0.0
        $spec = $specs[$g]
        $score = $specs["score"]
        foreach($flaw_key in $spec.Keys) {
            $def_count = $dev["counts"][$flaw_key] -as [int]
            $spec_allow = $spec[$flaw_key]
            $v = 1.0 * $score[$flaw_key] * ($spec_allow - $def_count) / [Math]::Max(1,$spec_allow)
            $grade_score[$flaw_key] = $v
            $grade_score_total += $v
        }
    }
}
function test {
    #$d = parse_log_file C:\Tools\avia\test\classify-0028.txt
    $d = parse_log_folder C:\Tools\avia\test
    $d = merge_verizon_data $d
    $d | ConvertTo-Json -Depth 8 | Out-File "samples.json"
}


$spec = load_spec
$devs = parse_log_folder C:\Tools\avia\test 
foreach($d in $devs) {
    FunctionName $d $spec
}