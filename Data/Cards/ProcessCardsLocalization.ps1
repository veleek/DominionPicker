$lang = @("CS", "DE", "ES", "FI", "FR", "IT", "NL", "PL")

$VerbosePreference = "Continue"

function GenerateCardsXml($Language = $null, [string[]]$Properties)
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
    $propertyColumns = @{}
    $Properties | % {
        $propertyColumns[$_] = GetColumnForValue $_ 
    }

    for($i = 0; $i -lt $cardCount; $i++)
    {
        Write-Progress -Activity "Generating file $fileName" -Status "Adding Cards" -CurrentOperation "Card $i of $cardCount" -PercentComplete (($i / $cardCount) * 100) -ParentId 10

        $row = 2+$i
        $values = $Properties | % { 
            
            $propertyValue = $cards.Cells[$row, $propertyColumns[$_]].Text

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

            if(![string]::IsNullOrWhiteSpace($propertyValue))
            {
                if($propertyValue -eq "Young Witch") 
                { 
                    $na = 4 
                }
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

cd 'C:\code\vso\veleek\Dominion Picker\Data\Cards'


if(!(Test-Path variable:\cards) -or ((Test-Path variable:\xl) -and $xl.Visible -eq $false))
{
    Write-Verbose "Creating Excel Instance"
    $xl = New-Object -ComObject "Excel.Application"
    $xl.ScreenUpdating = $false
    #$xl.Visible = $true

    $cardInfoPath = (Resolve-Path .\DominionPickerData.xlsx).Path
    $cardsDoc = $xl.Workbooks.Open($cardInfoPath)
    $cards = $cardsDoc.WorkSheets["DominionPickerCards"]
}
else
{
    Write-Verbose "An instance of Excel already exists so we are using that."
}

$cardCount = 0 
$row=2
do
{
    $cardCount++
    $row++
    
}
while($cards.Cells[$row,1].Formula)

"Found $cardCount Cards"

Write-Progress -Activity "Generating Localized Card Files" -Status "Generating EN (default) Cards List" -Id 10 -PercentComplete 1

# Make the default language file
GenerateCardsXml -Properties "ID","Name","Set","Type","Cost","Rules"

$langCount = 0
$lang | % { 
    $langCount++
    $progress = (($langCount/($lang.Count+1))*100)
    Write-Progress -Activity "Generating Localized Card Files" -Status "Generating $_ Cards List" -Id 10 -PercentComplete $progress
    Write-Verbose "Generating $_ Cards List"
    GenerateCardsXml -Language $_ -Properties "ID","Name_$_","Rules_$_"
}

Write-Progress -Activity "Generating Localized Card Files" -Id 10 -Completed

dir DominionPickerCards*.xml | Set-Clipboard