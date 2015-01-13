$lang = @("CS", "DE", "ES", "FI", "FR", "IT", "NL", "PL")

function GenerateCardsXml($Language = $null, [string[]]$Properties)
{
    $fileName = ".\DominionPickerCards$(if($Language){ "_" + $Language }).xml"
    if(Test-Path $fileName)
    {
        Remove-Item $fileName
    }

    "Making file $fileName"

    '<?xml version="1.0" encoding="utf-8" ?>' | Add-Content $fileName
    '<ArrayOfCard>' | Add-Content $fileName

    $propertyColumns = @{}
    $Properties | % {
        $propertyColumns[$_] = GetColumnForValue $_ 
    }

    "Outputting cards"
    for($i = 0; $i -lt $cardCount; $i++)
    {
        $row = 2+$i
        $values = $Properties | % { 
            
            $propertyValue = $cards.Cells[$row, $propertyColumns[$_]].Text

            $propertyName = $_
            if($Language -and $propertyName.Contains("$Language"))
            {
                $propertyName=$propertyName.Substring(0, $propertyName.Length-3)
            }

            if(![string]::IsNullOrWhiteSpace($propertyValue))
            {
                "$propertyName=`"$propertyValue`"" 
            }
        }

        $content = [string]::Join(" ", $values)

        if($content.Contains("Name"))
        {
            "    <$content />" | Add-Content $fileName -Encoding UTF8
        }
    }    

    '</ArrayOfCard>' | Add-Content $fileName
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

cd 'C:\code\vso\veleek\Dominion Picker\Data\Cards'

if(!$cards)
{
    $xl = New-Object -ComObject "Excel.Application"
    $xl.Visible = $true

    $cardInfoPath = (Resolve-Path .\DominionPickerCards.xlsx).Path
    $cardsDoc = $xl.Workbooks.Open($cardInfoPath)
    $cards = $cardsDoc.WorkSheets["DominionPickerCards"]
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

# Make the default language file
GenerateCardsXml -Properties "ID","Name","Set","Type","Cost","Rules"

$lang | % { 
    GenerateCardsXml -Language $_ -Properties "ID","Name_$_","Rules_$_"
}