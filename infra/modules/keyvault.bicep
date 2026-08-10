@description('Key Vault name')
param name string

@description('Azure region')
param location string

@description('Tags applied to the vault')
param tags object

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    publicNetworkAccess: 'Enabled'
  }
}

output vaultUri string = keyVault.properties.vaultUri
output vaultId string = keyVault.id
