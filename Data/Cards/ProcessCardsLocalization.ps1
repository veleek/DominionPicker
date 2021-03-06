﻿param(
	$Languages = @("CS", "DE", "ES", "FI", "FR", "IT", "NL", "PL"),
	[Switch]$OnlyDefault
)

$VerbosePreference = "Continue"

function GenerateCardsXml($Language = $null, [string[]]$Properties, $PropertyColumns)
{
    $fileName = ".\DominionPickerCards$(if($Language){ "." + $Language.ToLower() }).xml"
    
    Write-Progress -Activity "Generating file $fileName" -Status "Creating File" -ParentId 10
    
    if(Test-Path $fileName)
    {
        Remove-Item $fileName
    }

    '<?xml version="1.0" encoding="utf-8" ?>' | Add-Content $fileName
    '<ArrayOfCard>' | Add-Content $fileName

    $lastSetName = $null

    for($i = 0; $i -lt $cardCount; $i++)
    {
        Write-Progress -Activity "Generating file $fileName" -Status "Adding Cards" -CurrentOperation "Card $i of $cardCount" -PercentComplete (($i / $cardCount) * 100) -ParentId 10

        $row = 2+$i
        $values = $Properties | % { 
            
            $propertyValue = $cards.Cells[$row, $PropertyColumns[$_]].Text

            $propertyName = $_
            if($Language -and $propertyName.Contains("$Language"))
            {
                $propertyName=$propertyName.Substring(0, $propertyName.Length-3)
            }

            if($propertyName -eq "Set")
            {
                if($propertyValue -ne $lastSetName)
                {
                    $lastSetName = $propertyValue
                    "  <!-- $lastSetName -->" | Add-Content $fileName
                }
            }
            
            if($propertyName -eq "Pickable")
            {
              $propertyValue = $propertyValue.ToLower();
            }

            if(![string]::IsNullOrWhiteSpace($propertyValue))
            {
                $propertyValue = $propertyValue.Replace([char]0x2018,"`'")
                $propertyValue = $propertyValue.Replace([char]0x2019,"`'")
                $propertyValue = $propertyValue.Replace([char]0x201C,"`"")
                $propertyValue = $propertyValue.Replace([char]0x201D,"`"")

                "$propertyName=`"$propertyValue`"" 
            }
        }

        $content = [string]::Join(" ", $values)

        if($content.Contains("Name"))
        {
            "  <Card $content />" | Add-Content $fileName -Encoding UTF8
        }
    }    

    '</ArrayOfCard>' | Add-Content $fileName

    Write-Progress -Activity "Generating file $fileName" -Completed
}

function GetColumnForValue($Value)
{
    $col = 1
    while($cards.Cells(1,$col).Text)
    {
        if($cards.Cells(1,$col).Text -eq $Value)
        {
            return $col
        }
        $col++
    }

    return -1;
}

Write-Verbose "Starting Card Processing"

cd $PSScriptRoot

Write-Verbose "Creating Excel Instance"
$xl = New-Object -ComObject "Excel.Application"

try
{
	$xl.ScreenUpdating = $false

	$cardInfoPath = (Resolve-Path .\DominionPickerData.xlsx).Path
	$cardsDoc = $xl.Workbooks.Open($cardInfoPath)
	$cards = $cardsDoc.WorkSheets["DominionPickerCards"]

	$cardCount = 0 
	$row=2
	do
	{
		$cardCount++
		$row++
		
	}
	while($cards.Cells[$row,1].Formula)

	"Found $cardCount Cards"
	#$cardCount = 10

	Write-Progress -Activity "Generating Localized Card Files" -Status "Generating EN (default) Cards List" -Id 10 -PercentComplete 1

	# Make the default language file
	$props = @("ID","Name","Set","Type","Cost","Pickable","Rules")
	$propertyColumns = @{}
	$props | % { $propertyColumns[$_] = GetColumnForValue $_ }
	GenerateCardsXml -Properties $props -PropertyColumns $propertyColumns

	if($Languages -and ($Languages.Count -gt 0) -and (!$OnlyDefault))
	{
		$langCount = 0
		$Languages | % { 
			$langCount++
			$progress = (($langCount/($Languages.Count+1))*100)
			Write-Progress -Activity "Generating Localized Card Files" -Status "Generating $_ Cards List" -Id 10 -PercentComplete $progress
			Write-Verbose "Generating $_ Cards List"
			$locProps = @("ID","Name_$_","Rules_$_")
			$locPropColumns = @{}
			$locProps | % { $locPropColumns[$_] = GetColumnForValue $_ }
			GenerateCardsXml -Language $_ -Properties $locProps -PropertyColumns $locPropColumns
		}
	}

	Write-Progress -Activity "Generating Localized Card Files" -Id 10 -Completed
}
finally
{
	Write-Host "Killing Excel instance"
	gps Excel | kill
}

Write-Host "Processing Complete.  Copying all localized card files to clipboard."
dir DominionPickerCards*.xml | Set-Clipboard