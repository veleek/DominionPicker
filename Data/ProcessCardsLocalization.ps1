$cardLookup = @{}

$fileName = "cardlist"
Get-Content ".\$($fileName).csv" -Encoding:String | Out-File ".\$($fileName)_UTF8.csv" -Encoding:Unicode

$engCards = [xml](gc ..\Assets\DominionPickerCards.xml)
$engCards.ArrayOfCard.Card | % { $cardLookup[$_.Name] = [int]($_.ID) }

$allLocCards = Import-Csv ".\$($fileName)_UTF8.csv"
$allLocCards[9]
#return

$locCards = $allLocCards | ? { ![String]::IsNullOrEmpty($_.'Card-cz') }

$locCards | Sort-Object { $cardLookup[$_.Card] } | % {
    $id = $cardLookup[$_.Card]
    #"  <Card ID=`"$id`" Name=`"$($_.'Card-de')`" OriName=`"$($_.Card)`" />"
    "  <Card ID=`"$id`" Name=`"$($_.'Card-cz')`" />"
} | clip




