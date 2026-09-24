param(
    [Parameter(Mandatory = $true)]
    [string]$ValheimDir
)

$ErrorActionPreference = "Stop"
$dll = Join-Path $ValheimDir "valheim_Data\Managed\assembly_valheim.dll"
if (!(Test-Path $dll)) {
    throw "assembly_valheim.dll not found: $dll"
}

$managed = Split-Path $dll -Parent
$resolveHandler = [System.ResolveEventHandler]{
    param($sender, $args)
    $name = ([System.Reflection.AssemblyName]$args.Name).Name + ".dll"
    $candidate = Join-Path $managed $name
    if (Test-Path $candidate) {
        return [System.Reflection.Assembly]::LoadFrom($candidate)
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolveHandler)

try {
    $asm = [System.Reflection.Assembly]::LoadFrom($dll)
    Write-Host "Assembly: $($asm.FullName)"

    $types = @()
    try {
        $types = $asm.GetTypes()
    }
    catch [System.Reflection.ReflectionTypeLoadException] {
        $types = $_.Exception.Types | Where-Object { $_ -ne $null }
        Write-Warning "Some types could not be loaded; continuing with available types."
    }

    $trader = $types | Where-Object { $_.Name -eq "Trader" } | Select-Object -First 1
    if (!$trader) {
        Write-Host "Trader type not found. Matching types:"
        $types | Where-Object { $_.Name -like "*Trader*" -or $_.FullName -like "*Trader*" } |
            Select-Object -ExpandProperty FullName
        exit 2
    }

    Write-Host ""
    Write-Host "Trader type: $($trader.FullName)"
    Write-Host "Nested types:"
    $trader.GetNestedTypes([System.Reflection.BindingFlags]"Public,NonPublic") |
        ForEach-Object { Write-Host "  $($_.FullName)" }

    $tradeItem = $trader.GetNestedTypes([System.Reflection.BindingFlags]"Public,NonPublic") |
        Where-Object { $_.Name -eq "TradeItem" } | Select-Object -First 1

    if (!$tradeItem) {
        Write-Host ""
        Write-Host "TradeItem nested type not found. Trade-related types in assembly:"
        $types | Where-Object { $_.Name -like "*Trade*" -or $_.FullName -like "*Trade*" } |
            Select-Object -ExpandProperty FullName
        exit 3
    }

    Write-Host ""
    Write-Host "TradeItem: $($tradeItem.FullName)"
    Write-Host "Fields:"
    $tradeItem.GetFields([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static") |
        ForEach-Object { Write-Host ("  {0,-32} {1}" -f $_.Name, $_.FieldType.FullName) }

    Write-Host ""
    Write-Host "Properties:"
    $tradeItem.GetProperties([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static") |
        ForEach-Object { Write-Host ("  {0,-32} {1}" -f $_.Name, $_.PropertyType.FullName) }

    Write-Host ""
    Write-Host "Constructors:"
    $tradeItem.GetConstructors([System.Reflection.BindingFlags]"Public,NonPublic,Instance") |
        ForEach-Object { Write-Host "  $($_.ToString())" }

    Write-Host ""
    Write-Host "StoreGui FillList methods:"
    $storeGui = $types | Where-Object { $_.Name -eq "StoreGui" } | Select-Object -First 1
    if ($storeGui) {
        $storeGui.GetMethods([System.Reflection.BindingFlags]"Public,NonPublic,Instance,Static") |
            Where-Object { $_.Name -eq "FillList" } |
            ForEach-Object { Write-Host "  $($_.ToString())" }
    }
}
finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolveHandler)
}
