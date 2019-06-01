function parse_flaws([string]$flaw_str) {
    $flaws=@()
    $count=0
    $ms = $flaw_str | Select-String -Pattern "flaw = ([\w-]+), region = ([@\w]+), surface = (\w+), sort = (\w+), length = ([\d.]+) mm, width = ([\d.]+) mm, area = ([\d.]+) mm" -AllMatches
    foreach($m in $ms.Matches) {
        if($m.Success) {
            $f=@{
                flaw = $m.Groups[1].Value
                region = $m.Groups[2].Value
                surface = $m.Groups[3].Value
                sort = $m.Groups[4].Value
                length = $m.Groups[5].Value -as [double]
                width = $m.Groups[6].Value -as [double]
                area = $m.Groups[7].Value -as [double]
            }
            $flaws += $f
            if($f["sort"] -ne "Discoloration"){
                $count ++
            }
        }
    }
    return $flaws, $count
}

function parse_counts([string]$count_str){
    $ret = @{}
    $ms = $count_str | Select-String -Pattern "n([\w-]+) = (\d+)\s*" -AllMatches
    foreach($m in $ms.Matches) {
        $ret[$m.Groups[1].Value.Trim()] = $m.Groups[2].Value -as [int]
    }
    return $ret
}

function parse_surface([string]$surface, [string]$data){
    $ret = @{}
    $count = 0
    $lines = $data -split '\\n'
    foreach($line in $lines){
        $ms = $line | Select-String -Pattern "\s*([ \w-]+) = (\d+)\s*" 
        if($ms.Matches[0].Success){
            $k = $ms.Matches[0].Groups[1].Value.Trim()
            if($k -eq "Totoal number on $surface"){
                $ret["$surface-All-All"] = $ms.Matches[0].Groups[2].Value.Trim() -as [int]
            }
            elseif($k -eq "Totoal number of major on $surface"){
                $ret["$surface-Major-All"] = $ms.Matches[0].Groups[2].Value.Trim() -as [int]
            }
            elseif ($k -like "Zone*") {
                $ret["$surface-$k-All"] = $ms.Matches[0].Groups[2].Value.Trim() -as [int]            
                $count ++
            }            
        }
    }
    if($surface -eq "AA") {
        $ret["AA-region-all"] = $count
    }
    return $ret
}

function parse_log_file([string]$filename) {
    $all_text = Get-Content $filename
    $m = $all_text -join '\n' | Select-String -Pattern "Flaws:(.+)Count:(.+)AA Surface:(.+)A Surface:(.+)B Surface:(.+)C Surface:(.+)Grade =(.+)"
    $dev = @{}
    if($m.Matches.Success){
        # flaws:
        $count=0
        $dev["flaws"], $count = parse_flaws $m.Matches.Groups[1].Value
        $counts = parse_counts $m.Matches.Groups[2].Value 
        $counts += parse_surface "AA" $m.Matches.Groups[3].Value
        $counts += parse_surface "A" $m.Matches.Groups[4].Value
        $counts += parse_surface "B" $m.Matches.Groups[5].Value
        $counts += parse_surface "C" $m.Matches.Groups[6].Value
        $counts["All-All-All"] = $count
        $dev["counts"] = $counts
        $dev["grade"] = $m.Matches.Groups[7].Value.Trim()
        $dev["filename"] = $filename
#        $dev | ConvertTo-Json | Out-Host
    }
    return $dev
}

function parse_log_folder([string]$floder){
    $ret = @()
    Get-ChildItem $folder | ForEach-Object {
        $d = parse_log_file $_.FullName
        $ret += $d
    }
    return $ret
}

function merge_verizon_data {
    param (
        $db        
    )
    $vdata = Get-Content .\verizon_data.json | ConvertFrom-Json
    foreach($r in $db){
        $m = [System.IO.Path]::GetFileNameWithoutExtension($r["filename"]) | Select-String -Pattern "-(\d+)"
        if($m.Matches[0].Success){
            $v = $vdata | Where-Object IMEI -Like "*$($m.Matches[0].Groups[1].Value)"
            $v.psobject.Properties | foreach { $r[$_.Name] = $_.Value }
        }
    }
    return $db
}

function load_spec {
    param (
        [string]$specfn = ".\classify.xml"
    )
    [xml]$spec_xml = Get-Content $specfn
    $grades = $spec_xml.DocumentElement | Select-Xml "grade/item"
    $ret = @{}
    foreach($a in $grades) {
        $d = @{}
        $d["All-All-All"] = $a.Node.max_flaws -as [int]
        foreach($b in $a.Node.surface_grade.ChildNodes) {
            $surface = $b.surface
            $d["$surface-All-All"] = $b.max_flaws -as [int]
            $d["$surface-Major-All"] = $b.max_major_flaws  -as [int]
            $d["$surface-region-All"] = $b.max_region_flaws -as [int]
            foreach($c in $b.flaw_allow.ChildNodes) {
                $d[$c.flaw] = $c.allow -as [int]
            }
        }
        $ret[$a.Node.name] = $d
    }
    ## load score.xml
    [xml]$score = Get-Content .\score.xml
    $score_xml = @{}
    foreach($a in $score.DocumentElement.ChildNodes) {
        $score_xml[$a.Name] = $a.InnerText -as [int]
    }
    $ret["score"] = $score_xml
    return $ret
}