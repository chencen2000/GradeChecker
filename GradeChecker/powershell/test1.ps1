function FunctionName {
    $grade_level = @("A+", "A", "B", "C", "D+", "D")
    $db = Get-Content .\device_270_info.json | ConvertFrom-Json
    $new_db = @()
    foreach($r in $db) {
        $new_r = @{}
        $label= [array]::IndexOf($grade_level, $r.device."VZW Grade")
        if($label -ge 0) {
            $new_r["VZW"] = [array]::IndexOf($grade_level, $r.device."VZW Grade")
            foreach($g in $grade_level) {
                $gscore = $r.Grade.$g
                $gscore.psobject.Properties | foreach { $new_r["$g-$($_.Name)" -replace "\+", "P"] = $_.Value }
            }
            $new_db += $new_r
        }
    }

    $new_db | Convertto-Json | Out-file test.json
}

function FunctionName1 {
    $sort = @(  
        @{ surface="AA"; name="Scratch"; count=20}, 
        @{ surface="A"; name="Scratch"; count=20}, 
        @{ surface="B"; name="Scratch"; count=20}, 
        @{ surface="C"; name="Scratch"; count=20}, 
        @{ surface="AA"; name="Nick"; count=20},
        @{ surface="A"; name="Nick"; count=20},
        @{ surface="B"; name="Nick"; count=20},
        @{ surface="C"; name="Nick"; count=20},
        @{ surface="A"; name="PinDotGroup"; count=20},
        @{ surface="B"; name="PinDotGroup"; count=20},
        @{ surface="C"; name="PinDotGroup"; count=20},
        @{ surface="A"; name="Discoloration"; count=20}
        @{ surface="B"; name="Discoloration"; count=20}
        @{ surface="C"; name="Discoloration"; count=20}
    )
    $grade_level = @("A+", "A", "B", "C", "D+", "D")
    $db = Get-Content .\device_270_info.json | ConvertFrom-Json
    $new_db = @()
    foreach($r in $db) {
        # $label= [array]::IndexOf($grade_level, $r.device."VZW Grade")
        $label = 0
        if ($r.device."VZW Grade" -eq "A") {
            $label = 1
        }
        if($label -ge 0) {
            $no_data=$true
            $new_r = [ordered]@{}
            $new_r["VZW"] = $label
            #$new_r["imei"] = r.imei
            foreach($s in $sort) {
                $ss = $r.device.flaws | Where-Object { $_.sort -eq $s.name -and $_.surface -eq $s.surface }
                for($i=0; $i -lt $s.count; $i++) {
                    $length=0.0
                    $width=0.0
                    $area=0.0
                    if($ss) {
                        if($ss.count) {
                            if($i -lt $ss.count) {
                                $length=$ss[$i].length
                                $width =$ss[$i].width
                                $area = $ss[$i].area
                                $no_data = $false
                            }
                        }
                        else {
                            $length=$ss.length
                            $width =$ss.width
                            $area = $ss.area
                            $no_data = $false
                            $ss = $null
                        }
                    }
                    $new_r["{0}_{1}_{2}_length" -f $s.surface, $s.name, $i]=$length
                    $new_r["{0}_{1}_{2}_width" -f $s.surface, $s.name, $i]=$width
                    # $new_r["{0}_{1}_{2}_area" -f $s.surface, $s.name, $i]=$area                
                }
            }
            if (!$no_data) {
                $new_db +=$new_r
            }
            else{
                Write-Host $r.device.IMEI
            }
        }
    }
    $new_db | ConvertTo-Json | Out-File .\test.json
}

function FunctionName2 {
    $sort = @(  
        @{ surface="AA"; name="Scratch"; count=20}, 
        @{ surface="A"; name="Scratch"; count=20}, 
        @{ surface="B"; name="Scratch"; count=20}, 
        @{ surface="C"; name="Scratch"; count=20}, 
        @{ surface="AA"; name="Nick"; count=20},
        @{ surface="A"; name="Nick"; count=20},
        @{ surface="B"; name="Nick"; count=20},
        @{ surface="C"; name="Nick"; count=20},
        @{ surface="A"; name="PinDotGroup"; count=20},
        @{ surface="B"; name="PinDotGroup"; count=20},
        @{ surface="C"; name="PinDotGroup"; count=20},
        @{ surface="A"; name="Discoloration"; count=20}
        @{ surface="B"; name="Discoloration"; count=20}
        @{ surface="C"; name="Discoloration"; count=20}
    )
    $grade_level = @("A+", "A", "B", "C", "D+", "D")
    $db = Get-Content .\device_270_info.json | ConvertFrom-Json
    $new_db = @()
    foreach($r in $db) {
        $label= [array]::IndexOf($grade_level, $r.device."VZW Grade")
        # $label = 0
        # if ($r.device."VZW Grade" -eq "A") {
        #     $label = 1
        # }
        if($label -ge 0) {
            $no_data=$true
            $new_r = [ordered]@{}
            $new_r["VZW"] = $label
            #$new_r["imei"] = r.imei
            foreach($s in $sort) {
                $ss = $r.device.flaws | Where-Object { $_.sort -eq $s.name -and $_.surface -eq $s.surface }
                for($i=0; $i -lt $s.count; $i++) {
                    $length=0.0
                    $width=0.0
                    $area=0.0
                    if($ss) {
                        if($ss.count) {
                            if($i -lt $ss.count) {
                                $length=$ss[$i].length
                                $width =$ss[$i].width
                                $area = $ss[$i].area
                                $no_data = $false
                            }
                        }
                        else {
                            $length=$ss.length
                            $width =$ss.width
                            $area = $ss.area
                            $no_data = $false
                            $ss = $null
                        }
                    }
                    $new_r["{0}_{1}_{2}_length" -f $s.surface, $s.name, $i]=$length
                    $new_r["{0}_{1}_{2}_width" -f $s.surface, $s.name, $i]=$width
                    # $new_r["{0}_{1}_{2}_area" -f $s.surface, $s.name, $i]=$area                
                }
            }
            if (!$no_data) {
                $new_db +=$new_r
            }
            else{
                Write-Host $r.device.IMEI
            }
        }
    }
    $new_db | ConvertTo-Json | Out-File .\test.json
}
