module Config

type Config = {
    connectionString : string
    edsUrl : string
    webAuthUrl : string
    edsUserName : string
    edsPassword : string
    configSettingExample : string
    environment: string
    blobStorageConnectionString: string
    sink: string
}