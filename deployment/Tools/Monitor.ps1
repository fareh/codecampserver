$database = $args[0]

[string[]]$services="w3svc","msdepsvc"
[string[]]$AppPoolNames = "WebApp"
[string[]]$databases = "codecampserver_prd"
[string[]]$taskFolders = @()  # "\Microsoft\Windows\Application Experience","\"

[bool]$script:alert=$false

$now = get-date


# IIS App pools
    if (get-pssnapin webadministration) {add-pssnapin WebAdministration}
    elseif (-not (get-module webadministration)) {import-module webadministration}

    "IIS Status"    
    dir iis:\apppools | ft Name,State
    try{
        $AppPoolNames |foreach-object {
        $apppool = get-item iis:\apppools\$_ 

        "$_ is " + $apppool.state
        if(-not( $apppool.state -eq "Started")) { $alert=$true  }
        }
    }
    catch
    {
        $alert=$true
    }

#services
    " "   
    "Suspect Windows Services (Auto start that are stopped)"
    Get-WmiObject win32_service | where { $_.startmode -eq "auto" -and $_.state -eq "stopped" } | ft
    $services | foreach-object {
        $servicename=$_        
        $service = get-service | where-object {$_.name -eq $servicename}
        if($service) {
            "$servicename is " + $service.status
            if( !($service.status -eq "running")) {
                $alert=$true
            }
        } else {
        $servicename + " does not exist"
        $alert=$true
        }
    }

#processes
" "
"Process by Memory Usage"
get-process |sort-object ws -descending | select -first 10| ft ProcessName,Cpu,ws,vm
#diskspace
Get-WmiObject Win32_LogicalDisk | Where-Object {$_.DriveType -eq 3}  `
    | Select DeviceID,`
      @{n="% Freespace";e={"{0:P2}" -f ([long]$_.FreeSpace/[long]$_.Size)}},`
      @{Name=”Freespace(GB)”;Expression={“{0:N1}” -f($_.freespace/1gb)}} | ft
     
Get-WmiObject -Class Win32_OperatingSystem | `
    ft @{Name=”Free memory (GB)”;Expression={“{0:N1}” `
    -f($_.FreePhysicalMemory/1mb)}}


#Tasks
" "   
    "Tasks Reporting Errors"
    $ST = new-object -com("Schedule.Service")
    $ST.connect()
    foreach ($folder in $taskFolders)
    {
        $activationfolder = $ST.getfolder($folder)
        $ScheduledTasks = $activationfolder.GetTasks(0)
        foreach ($task in $ScheduledTasks)
        {
            if (($task.Enabled -eq $true) -and  ($task.LastTaskResult -ne 0))
            {
                $task.path
                $alert=$true
            }
        }
    }
" " 


    function checkdatabases()
    {
        param( [string] $servername)
    
        $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
        $SqlCmd = New-Object System.Data.SqlClient.SqlCommand
        $SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter
        $DataSet = New-Object System.Data.DataSet
        $DataSet2 = New-Object System.Data.DataSet
        $DataSet3 = New-Object System.Data.DataSet
        $DataSet4 = New-Object System.Data.DataSet
        $SqlConnection.ConnectionString = 
          "Server=$servername;Database=master;Integrated Security=True"
        $SqlCmd.CommandText = "select name from master.dbo.sysdatabases"
        $SqlCmd.Connection = $SqlConnection
        $SqlAdapter.SelectCommand = $SqlCmd
        $SqlAdapter.Fill($DataSet)|out-null
        $dbs =$DataSet.Tables[0]
        #$dbs 
        foreach ($db in $dbs)
        {
            if($databases -contains $db.name)
            {
            #$db.name
            $SqlCmd.CommandText = $db.name+"..sp_spaceused "
            $SqlCmd.Connection = $SqlConnection
            $SqlAdapter.SelectCommand = $SqlCmd
            $SqlAdapter.Fill($DataSet2) |out-null
            }
        }
        $DataSet2.Tables[0]| format-table -autosize

        foreach ($db in $dbs)
        {
            if($databases -contains $db.name)
            {
                #$db.name
                $SqlCmd.CommandText = "
                select '"+$db.name+"' as Dbname,
                DATABASEPROPERTY('"+$db.name+"','IsInRecovery') as Inrecovery,
                DATABASEPROPERTY('"+$db.name+"','IsInLoad')  as InLoad,
                DATABASEPROPERTY('"+$db.name+"','IsEmergencyMode')  as InEmergency,
                DATABASEPROPERTY('"+$db.name+"','IsOffline') as Isoffline,
                DATABASEPROPERTY('"+$db.name+"','IsReadOnly')  as IsReadonly,
                DATABASEPROPERTY('"+$db.name+"','IsSingleUser')  as IsSingleuser,
                DATABASEPROPERTY('"+$db.name+"','IsSuspect') as IsSuspect,
                DATABASEPROPERTY('"+$db.name+"','IsInStandBy') as IsStandby,
                DATABASEPROPERTY('"+$db.name+"','Version') as version,
                DATABASEPROPERTY('"+$db.name+"','IsTruncLog') as IsTrunclog
                "
                #$SqlCmd.CommandText 
                $SqlCmd.Connection = $SqlConnection
                $SqlAdapter.SelectCommand = $SqlCmd
                $SqlAdapter.Fill($DataSet4) |out-null
            }
        }
        $DataSet4.Tables[0]| format-table -autosize
        
        $DataSet4.Tables[0] | foreach-object{
            if($_.InEmergency -eq "1") {$script:alert=$true}
            if($_.InLoad      -eq "1") {$script:alert=$true}
            if($_.Inrecovery  -eq "1") {$script:alert=$true}
            if($_.Isoffline  -eq "1") {$script:alert=$true}
            if($_.IsReadonly -eq "1") {$script:alert=$true}
            if($_.IsSingleuser -eq "1") {$script:alert=$true}
            if($_.IsStandby -eq "1") {$script:alert=$true}
            if($_.IsSuspect  -eq "1") {$script:alert=$true}
        }
        
        $SqlCmd.CommandText = "DBCC SQLPERF(LOGSPACE) WITH NO_INFOMSGS "
        $SqlCmd.Connection = $SqlConnection
        $SqlAdapter.SelectCommand = $SqlCmd
        $SqlAdapter.Fill($DataSet3)|out-null
        $DataSet3.Tables[0] | foreach {
            if( $databases -contains $_."database name" ) {
             $_   
            }
        }
        
        #| where-object { $."database name" -eq  } |format-table -autosize
        $SqlConnection.Close()
    }
    
checkdatabases $database



if($alert){
    "The Application is Not OK"# | out-default
    }
    else
    {
    "The Application is OK" #| out-default
    }
    
 "Updated at $now"
 