param(
    [switch]$Continuous = $false,
    [string]$NetstatParams = "ano",
    [string]$Pattern = "\s+(TCP|UDP)"
)
    
function Get-NetworkStatistics {
    netstat -$NetstatParams | sls -pattern $Pattern | % {
        $itm = $_.line.split(" ", [System.StringSplitOptions]::RemoveEmptyEntries)

        if (-not $itm[1].StartsWith('[::')) {
            $localaddr, $localport = get-address-port $itm 1
            $remoteaddr, $remoteport = get-address-port $itm 2

            [pscustomobject] @{
                pid           = $itm[-1]
                processname   = get-process-name $itm[-1]
                protocol      = $itm[0]
                localaddress  = $localaddr
                localport     = $localport
                remoteaddress = $remoteaddr
                remoteport    = $remoteport
                state         = if ($itm[0] -eq 'tcp') { $itm[3] } else { $null }
            }
        }
    }
}

function get-address-port($itm, $idx) {
    $address, $port = if (($itm[$idx] -as [ipaddress]).AddressFamily -eq 'InterNetworkV6') {
        $ipToString, $itm[$idx].split(']:')[-1]
    }
    else {
        $itm[$idx].split(':')[0], $itm[$idx].split(':')[1]
    }
    $address, $port
}

function get-process-name($id) {
    (Get-Process -id $id -ErrorAction SilentlyContinue).Name
}

do {
    Get-NetworkStatistics | ft -a
    
    if ($Continuous) { 
        Start-Sleep -Seconds 2 
        Clear-Host
    }
} while ($Continuous)