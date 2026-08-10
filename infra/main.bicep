targetScope = 'resourceGroup'

@description('Environment name, used in resource naming')
param environmentName string = 'dev'

@description('Azure region for all resources')
param location string = resourceGroup().location

var appName = 'houseofnaksh'
var tags = {
  project: appName
  env: environmentName
  managedBy: 'bicep'
}

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

output logAnalyticsId string = logAnalytics.id
output logAnalyticsCustomerId string = logAnalytics.properties.customerId