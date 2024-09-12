@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Set environment values for your storage account
rem set subscription_id=[YOUR_SUBSCRIPTION_ID]
rem set azure_storage_account=[YOUR_AZURE_STORAGE_ACCOUNT]
rem set azure_storage_key=[YOUR_AZURE_STORAGE_KEY]


echo Creating container...
call az storage container create --account-name !azure_storage_account! --subscription !subscription_id! --name margies --public-access blob --auth-mode key --account-key !azure_storage_key! --output none

echo Uploading files...
call az storage blob upload-batch -d margies -s data --account-name !azure_storage_account! --auth-mode key --account-key !azure_storage_key!  --output none

echo SubscriptionID: !subscription_id!
echo StorageAccount: !azure_storage_account!
echo StorageKey: !azure_storage_key!