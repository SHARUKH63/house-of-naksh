targetScope = 'resourceGroup'

// ---------------------------------------------------------------------------
// Parameters
// ---------------------------------------------------------------------------

@description('Environment name, used in resource naming')
param environmentName string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

// ---------------------------------------------------------------------------
// Variables
// ---------------------------------------------------------------------------

var appName = 'houseofnaksh'

var tags = {
  project: appName
  env: environmentName
  managedBy: 'bicep'
}

// ---------------------------------------------------------------------------
// Log Analytics
// ---------------------------------------------------------------------------

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: 'log-${appName}-${environmentName}'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ---------------------------------------------------------------------------
// Key Vault
//
// NOTE: RBAC role assignments are intentionally NOT managed here. See
// infra/README.md — keeping them out prevents the deployment pipeline from
// being able to escalate its own privileges.
// ---------------------------------------------------------------------------

module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault'
  params: {
    name: 'kv-${appName}-${environmentName}'
    location: location
    tags: tags
  }
}

module appInsights 'modules/appinsights.bicep' = {
  name: 'appinsights'
  params: {
    name: 'appi-${appName}-${environmentName}'
    location: location
    tags: tags
    workspaceId: logAnalytics.id
  }
}

// ---------------------------------------------------------------------------
// Outputs
// ---------------------------------------------------------------------------

output logAnalyticsId string = logAnalytics.id
output logAnalyticsCustomerId string = logAnalytics.properties.customerId
output keyVaultUri string = keyVault.outputs.vaultUri
output keyVaultId string = keyVault.outputs.vaultId
output appInsightsConnectionString string = appInsights.outputs.connectionString
