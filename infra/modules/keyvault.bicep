@description('Key Vault name')
param name string

@description('Azure region')
param location string

@description('Tags applied to the vault')
param tags object

@description('Principal ID of the Container App managed identity')
@minLength(36)
param appPrincipalId string

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

// Built-in role definition: Key Vault Secrets User (read secret values only)
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource appSecretsAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appPrincipalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: appPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output vaultUri string = keyVault.properties.vaultUri
output vaultId string = keyVault.id
