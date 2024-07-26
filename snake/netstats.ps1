param (
    [int]$port,
    [string]$processname,
    [string]$protocol,
    [switch]$continuous
)

function get-address-port($addressWithPort) {
    $address, $port = if (($addressWithPort -as [ipaddress]).AddressFamily -eq "InterNetworkV6") {
        $ipToString, $addressWithPort.split("]:")[-1]
    }
    else {
        $addressWithPort.split(":")[0], $addressWithPort.split(":")[1]
    }
    $address, $port
}
    
do {
    $netstatoutput = netstat -aon | % {
        $tokens = $_.trim() -split "\s+"
        if ($tokens[0] -in "tcp", "udp") {
            $localaddr, $localport = get-address-port $tokens[1]
            $remoteaddr, $remoteport = get-address-port $tokens[2]

            [pscustomobject] @{
                pid           = $tokens[-1]
                protocol      = $tokens[0]
                localaddress  = $localaddr
                localport     = $localport
                remoteaddress = $remoteaddr
                remoteport    = $remoteport
                state         = $tokens[0] -eq "tcp" ? $tokens[3] : $null
            }
        }
    }

    $netstatoutput | % {
        $process = gps -id $_.pid -ea SilentlyContinue
        $_ | add-member -MemberType NoteProperty -Name "processname" -Value $process.name
        $_ | add-member -MemberType NoteProperty -Name "ExecutablePath" -Value $process.path
    }

    $filteredoutput = $netstatoutput
    if ($port) {
        $filteredoutput = $filteredoutput | ? { $_.localaddress -like "*:$port" }
    }

    if ($processname) {
        $filteredoutput = $filteredoutput | ? { $_.processname -eq $processname }
    }

    if ($protocol) {
        $filteredoutput = $filteredoutput | ? { $_.protocol -eq $protocol }
    }

    # $netstatoutput | % {
    #     try {
    #         $ip = $_.remoteaddress
    #         if ($ip -ne '*') {
    #             $dnslookup = [System.Net.Dns]::GetHostEntry($ip)
    #             $_ | add-member -MemberType NoteProperty -Name 'DomainName' -Value $dnslookup.HostName 
    #         }
    #         else {
    #             $_ | add-member -MemberType NoteProperty -Name 'DomainName' -Value '*'
    #         }
    #     }
    #     catch {
    #         $_ | add-member -MemberType NoteProperty -Name 'DomainName' -Value 'Unresolved'
    #     }
    # }
  
    if ($continuous) {
        start-sleep -seconds 1
        clear-host
    }

    $groupedoutput = $filteredoutput | group-object -property "ExecutablePath"
    $groupedoutput | % {
        write-host $($_.name -eq "" ? "No executable found" : $_.name) -ForegroundColor DarkBlue
        $_.group | sort-object -Property ProcessName | select-object -Property * -ExcludeProperty ExecutablePath | ft -autosize
    }


} while ($continuous)

