Install-Module Az
Import-Module Az
Login-AzAccount
Select-AzSubscription -SubscriptionName "Visual Studio Ultimate with MSDN"

$dataFactoryName = "DataFactoryScanDF"
$resourceGroupName = "DataFactoryRG"
# Set-AzDataFactoryV2LinkedService -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "AzureKeyVault1" -File "AzureKeyVaultLinkedService.json"
# Set-AzDataFactoryV2LinkedService -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "DataFactoryScanDBLinkedService" -File "LinkedService.json"

Set-AzDataFactoryV2Dataset -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "SourceDataset" -DefinitionFile "SourceDataset.json"
Set-AzDataFactoryV2Dataset -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "TargetDataset" -DefinitionFile "TargetDataset.json"

$DFPipeLine = Set-AzDataFactoryV2Pipeline -DataFactoryName $dataFactoryName -ResourceGroupName $resourceGroupName -Name "SourceToTargetPipeline" -DefinitionFile "SourceToTargetPipeline.json"
