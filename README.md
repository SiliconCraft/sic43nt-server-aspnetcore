# SIC43NT Tag authenticity verification using rolling code

This project provides an example ASP.NET Core project for SIC43NT rolling code authentication web application. SIC43NT tag provider can apply this concept to allow SIC43NT tag holders to verify their tags as well as communicate to them regarding the tag status.

Table of Content
* [Basic Concept of SIC43NT](#Basic-Concept-of-SIC43NT)
* [Getting Started](#Getting-Started)
  * [Installing on Microsoft Azure Web App](#Installing-on-Microsoft-Azure-Web-App)  
  * [Installing on Google Cloud Platform](#Installing-on-Google-Cloud-Platform)
* [Usage](##Usage)

## Basic Concept of SIC43NT 

SIC43NT Tag provide 4 distinct NDEF contents coded in Hexadecimal string which can be passed to web service directly. The contents includes
1. **UID** or **Unique ID** **:** 7-bytes UID of this Tag (i.e. "39493000012345")
1. **Tamper Flag:** 1-byte content reflect status of tamper pin. If tamper pin is connected to the GND, the result is "00". Otherwise Tamper Flag will be "AA" by factory setting value. 
1. **Time-Stamp:** 4-bytes randomly increasing value (each step of increasing is 1 to 255). This content always increasing each time the tag has been read.
1. **Rolling Code:** 4-bytes of stream cipher ([Mickey V1](http://www.ecrypt.eu.org/stream/ciphers/mickey/mickey.pdf)) with input from Time-stamp value as IV.

## Getting Started

### Installing on Microsoft Azure Web App 

#### Prerequisites

* SIC43NT Tag
* Mobile Application for Encoding Data to SIC43NT
   * [SIC Tag encoder](https://play.google.com/store/apps/details?id=th.co.sic.nfc_tag_encoder) App on Google Play
   * [SIC Tag encoder](https://apps.apple.com/th/app/nfc-tag-encoder/id6740401948) App on Apple Store 
* [Microsoft Azure Account](https://azure.microsoft.com/) 
* [Azure Command Line / Azure CLI](https://docs.microsoft.com/en-us/cli/azure) from [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/cloud-shell/overview) in Azure Portal or locally [install](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) on your macOS, Linux or Window machine.

#### Step 1 : Create a resource group

Create a new resource group to contain this new sample web app by using Azure command line.
The following example <your-resource-group-name> creates a new resource group name and <your-server-location> is the location (i.e. "West Europe").

```
az group create --name <your-resource-group-name> --location <your-server-location>
```

#### Step 2 : Create a new Free App Service Plan
Create a new Free App Service Plan by using Azure command line. The following example <your-service-plan-name> creates a new Free App service plan name in your resource group from step 1.

```
az appservice plan create --name <your-service-plan-name> --resource-group <your-resource-group-name> --sku FREE
```

#### Step 3 : Create a Web App 
Create a new Web App by using Azure command line. The following example creates a new Web App. Please replace '<app_name>' with a globally unique app name (valid characters are 'a-z', '0-9', and '-'). 

```
az webapp create --resource-group <your-resource-group-name> --plan <your-service-plan-name> --name <app_name>
```

#### Step 4 : Deploy the sample app using Git
Deploy source code from GitHub to Azure Web App using Azure command line. The following example deploy source code from https://github.com/SiliconCraft/sic43nt-server-aspnetcore.git in master branch to a Web App name '<app_name>' in resource group <your-resource-group-name>.
```
az webapp deployment source config --repo-url https://github.com/SiliconCraft/sic43nt-server-aspnetcore.git --branch master  --name <app_name> --resource-group <your-resource-group-name> --manual-integration
```

#### Step 5 : Customize SIC43NT Tag
Use SIC43NT Writer App on Android NFC Phone to customize SIC43NT Tag as the explanation below.
* RLC MODE Tab (In case of default tag, this RLC mode can leave with default factory value)
  * **Rolling Code Mode**
    * Rolling Code keeps changing.
  * **Rolling Code Key**
    * FFFFFF + Tag UID (i.e. The Key of the Tag with UID = "39493000012345" is "FFFFFF39493000012345".)

* NDEF MESSAGE Tab
  * **MIME:** URL/URI
  * **Prefix:** https://
  * **NDEF Message:** <app_name>.azurewebsites.net/?d=
  * **Dynamic Data**
    * **UID:** Checked
    * **Tdata:** Checked
    * **Rolling Code:** Checked

After completely customize SIC43NT Tag with the setting above, each time you tap the SIC43NT tag to NFC Phone (iPhone, Android or any NDEF support device), the web page will display a table of Tamper Flag, Time Stamp value and Rolling Code value which keep changing. Especially for the rolling code value, it will be a match between "From Tag" and "From Server" column. This mean that server-side applicationm (which calculate rolling code based on same Rolling Code Key) can check the authenicity of SIC43NT Tag.


### Installing on Google Cloud Platform

#### Prerequisites

* SIC43NT Tag
* Android NFC Phone with [SIC43NT Writer](https://play.google.com/store/apps/details?id=com.sic.app.sic43nt.writer) App
* [Google Cloud Console Account](https://console.cloud.google.com/) 
* Google Cloud App Engine

#### Step 1 : Creating and managing projects
 
Google Cloud projects form the basis for creating and using all Google Cloud services. 
To create and manage Google Cloud project using Google Cloud Console, please follow the step in [Creating and managing projects](https://cloud.google.com/resource-manager/docs/creating-managing-projects#creating_a_project)

#### Step 2 : Creating App Engine 

1. Click "Go to the [Dashboard page](https://console.cloud.google.com/home?_ga=2.205273744.1217504798.1597994442-2063354042.1597316606)" in the Cloud console
2. select your project name on top left corner.
3. click Activate Cloud Shell on top right corner to use browser shell command.
4. update all component and continue update press "Y" (Optional)

```
sudo apt-get update && sudo apt-get --only-upgrade install google-cloud-sdk-bigtable-emulator google-cloud-sdk-datastore-emulator google-cloud-sdk-cbt google-cloud-sdk-pubsub-emulator google-cloud-sdk-app-engine-python-extras google-cloud-sdk-minikube google-cloud-sdk-app-engine-python kubectl google-cloud-sdk-kpt google-cloud-sdk google-cloud-sdk-app-engine-go google-cloud-sdk-firestore-emulator google-cloud-sdk-app-engine-grpc google-cloud-sdk-cloud-build-local google-cloud-sdk-datalab google-cloud-sdk-anthos-auth google-cloud-sdk-kind google-cloud-sdk-spanner-emulator google-cloud-sdk-skaffold google-cloud-sdk-app-engine-java
```

#### Step 3 : Clone the git sample app using Google Cloud Shell

1. Clone source code from GitHub to Google Cloud Platform using Google Cloud Shell. The following example deploy source code from the master branch of https://github.com/SiliconCraft/sic43nt-server-aspnetcore.git
```
git clone https://github.com/SiliconCraft/sic43nt-server-aspnetcore.git
```

2. Move the directory into the project folder
```
cd sic43nt-server-aspnetcore/SIC43NT_Webserver
```

3. run the ASP.NET Core app on port 8080
```
dotnet run --urls=http://localhost:8080
```

4. view the app in your cloud shell toolbar, click "Web preview" and select "Preview on port 8080.".

5. publish the ASP.NET Core app by the following command
```
dotnet publish -c Release
```

6. Move the directory into publish folder
```
cd bin/Release/netcoreapp2.1/publish/
```

7. Create app.yaml for App engine flexible.
```
vim app.yaml
```
write this code to app.yaml file and save
```
runtime: aspnetcore
env: flex
```


#### Step 4 : Deploy and run

Deploy app on App Engine by the following command

```
gcloud app deploy --version v1
```

view the live app by the following command

```
gcloud app browse
```

then you will see the url link on the console, Go to this link to view your app.

For more information, please find [Quickstart for ASP.NET Core](https://codelabs.developers.google.com/codelabs/cloud-app-engine-aspnetcore/#0)

#### Step 5 : Customize SIC43NT Tag
Use SIC43NT Writer App on Android NFC Phone to customize SIC43NT Tag as the explanation below.
* RLC MODE Tab (In case of default tag, this RLC mode can leave with default factory value)
  * **Rolling Code Mode**
    * Rolling Code keeps changing.
  * **Rolling Code Key**
    * FFFFFF + Tag UID (i.e. The Key of the Tag with UID = "39493000012345" is "FFFFFF39493000012345".)

* NDEF MESSAGE Tab
  * **MIME:** URL/URI
  * **Prefix:** https://
  * **NDEF Message:** <PROJECT_ID>.<REGION_ID>.r.appspot.com/?d=
  * **Dynamic Data**
    * **UID:** Checked
    * **Tdata:** Checked
    * **Rolling Code:** Checked

After completely customize SIC43NT Tag with the setting above, each time you tap the SIC43NT tag to NFC Phone (iPhone, Android or any NDEF support device), the web page will display a table of Tamper Flag, Time Stamp value and Rolling Code value which keep changing. Especially for the rolling code value, it will be a match between "From Tag" and "From Server" column. This mean that server-side application (which calculate rolling code based on same Rolling Code Key) can check the authenticity of SIC43NT Tag.

## Usage

In general use-case of SIC43NT to perform authenicity verification, there are 2 cruial criterias to confirm, the consistence of rolling code and the increasing of time-stamp value.

### 1: Verify consistence of rolling code

To verify consistence of rolling code, you can copy class **KeyStream** ([KeyStream.cs](https://github.com/SiliconCraft/sic43nt-server-aspnetcore/blob/master/SIC43NT_Webserver/Utilities/KeyStream/KeyStream.cs)) and class **Encrypt** ([Encrypt.cs](https://github.com/SiliconCraft/sic43nt-server-aspnetcore/blob/master/SIC43NT_Webserver/Utilities/KeyStream/Encrypt.cs)) to your own ASP.net project. These classes are utility for rolling code calculation. 

The method *stream* of KeyStream calculate rolling code. It requires *80 bits-Key* (input as a 20-characters hexadecimal string) and *32 bits Time Stamp* or *32 bits iv* (input as a 8-characters hexadecimal string).

### 2: Verify increasing of time-stamp value

To verify increasing of time-stamp value, you need data storage to keep latest time-stamp value of each tag (each UID). Please note that this repository does not provide the example to store latest time-stamp in server at the moment. However the logic is quite simple, any web page request with time-stamp value less or equal the latest successful rolling code verification time-stamp value should be consider as a reuse of URL which shold be reject.

### Example usage

The class in this example which calling this method is **IndexModel** in [Index.cshtml.cs](https://github.com/SiliconCraft/sic43nt-server-aspnetcore/blob/master/SIC43NT_Webserver/Pages/Index.cshtml.cs) which is an example showing how to verify consistence of rolling code. However, it does not contain a verify increasing of time-stamp value due to this example does not contain data storage. The example to verify both consistence of rolling code and increasing of time-stamp value are shown below.


```C#
public void OnGet(string d)
{
    //...
    uid = d.Substring(0, 14);               
    flagTamperTag = d.Substring(14, 2);
    timeStampTag_str = d.Substring(16, 8);
    timeStampTag_uint = UInt32.Parse(timeStampTag_str, System.Globalization.NumberStyles.HexNumber);
    rollingCodeTag = d.Substring(24, 8);
    uid_key = get_key(uid);    // by default, key value is "FFFFFF" + uid
    rollingCodeServer = KeyStream.stream(uid_key, timeStampTag_str, 4);
    //...
    If (timeStampTag_uint > latest_timestamp(uid))
    {
        // Latest know Time stamp of this UID
        If (rollingCodeServer == rollingCodeTag)  // Compare rollingCodeServer against rollingCodeTag.
        {
            //Authentic SIC43NT Tag
        }
        else 
        {
            //Invalid Web Request
        }
    } 
    else
    {
        //Outdated time-stamp request
    }
    //...
}
```

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details
