# quick and dirt script to Start/Stop MsSqlServer
# needs to run on a MWE environment (maybe in the same VLan)

# parameters
$dbHostname = 'msfqamockdb.mwe.local'
$serviceName = 'MSSQLSERVER'


# waits for either 'Running' or 'Stopped' state for the given service name
function Wait-ForStableStatus {
	param(
		[string] $srvName,
		[string] $pcName
	)

	$done = $false;
	do {
		$srv = Get-Service -Name $srvName -ComputerName $pcName

		if($srv.Status -eq 'Running' -or $srv.Status -eq 'Stopped' ) { 
			$done = $true 
		}

		if($done -eq $false) {
			Write-Host "waiting for stable service status on '$pcName'"
			Start-Sleep -Seconds 1
		}
		
	} while ($done -eq $false)
}



$ips = @()
$dbs = @()
[System.Net.Dns]::Resolve($dbHostname).AddressList | % { $ips += $_.IPAddressToString }

# DNS based failover check
if($ips.Count -lt 2) { Write-Error "Can't run this script with less than 2 IPs associated with the domain name!" -ErrorAction Stop }

$ips | % { $dbs += Get-WmiObject Win32_Service -Filter "Name='$serviceName'" -ComputerName $_ } 

$ips | % { Wait-ForStableStatus $serviceName $_ }





if(($dbs | ? { $_.State -eq 'Stopped' } | measure).Count -lt 1) {
	$dbs[0].StartService() | Out-Null
	Wait-ForStableStatus $serviceName $dbs[0].PSComputerName
}

if(($dbs | ? { $_.State -eq 'Running' } | measure).Count -lt 1) {
	$dbs[0].StopService() | Out-Null
	Wait-ForStableStatus $serviceName $dbs[0].PSComputerName
}


$dbs | ? { $_.State -eq 'Stopped' } | % { $_.StartService() | Out-Null }

$dbs | %{ Wait-ForStableStatus $serviceName $_.PSComputerName } 

$dbs | ? { $_.State -eq 'Running' } | % { $_.StopService() | Out-Null }


$ips | % { Get-Service -Name $serviceName -ComputerName $_ } 
