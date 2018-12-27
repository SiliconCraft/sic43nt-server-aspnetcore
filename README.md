# SIC43NT Tag authenticity verification using rolling code

This project provides an example ASP.NET Core project for SIC43NT rolling code authentication web application. SIC43NT tag provider can apply this concept to allow SIC43NT tag holders to verify 
their tags as well as communicate to them regarding to the tag status.

## Getting Started

### Installing on Microsoft Azure Web App 

#### Prerequisites

* SIC43NT Tag
* Android NFC Phone with [SIC43NT Writer](https://play.google.com/store/apps/details?id=com.sic.app.sic43nt.writer) App
* [Microsoft Azure Account](https://azure.microsoft.com/) 
* [Azure Command Line / Azure CLI](https://docs.microsoft.com/en-us/cli/azure) from [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/cloud-shell/overview) in Azure Portal or locally [install](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) on your macOS, Linux or Window machine.

#### Step 1 : Create a resource group

Create a new resource group to contain this new sample web app by using Azure command line.
The following example creates a new resource group with name "sic43nt-sample-rg" and the location is in "West Europe". 

```
az group create --name sic43nt-sample-rg --location "West Europe"
```

#### Step 2 : Create a new Free App Service Plan
Create a new Free App Service Plan by using Azure command line. The following example creates a new Free App service plan with name "sic43nt_samplePlan" in resource group "sic43nt-sample-rg".

```
az appservice plan create --name sic43nt_samplePlan --resource-group sic43nt-sample-rg --sku FREE
```

#### Step 3 : Create a Web App 
Create a new Web App by using Azure command line. The following example creates a new Web App. Please replace '<app_name>' with a globally unique app name (valid characters are 'a-z', '0-9', and '-'). 

```
az webapp create --resource-group sic43nt-sample-rg --plan sic43nt_samplePlan --name <app_name>
```

#### Step 4 : Deploy the sample app using Git
Deploy source code from GitHub to Azure Web App using Azure command line. The following example deploy source code from https://github.com/SiliconCraft/sic43nt-server-aspnetcore.git in master branch to a Web App name '<app_name>' in resource group "sic43nt-sample-rg".
```
az webapp deployment source config --repo-url https://github.com/SiliconCraft/sic43nt-server-aspnetcore.git --branch master  --name <app_name> --resource-group sic43nt-sample-rg
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

## Basic Concept of SIC43NT 

SIC43NT Tag provide 4 disticnt NDEF contents coded in Hexadecimal string which can be pass to web service directly. The contents including
1. **UID** or **Unique ID** **:** 7-bytes UID of this Tag (i.e. "39493000012345")
1. **Tamper Flag:** 1-byte content reflect status of tamper pin. If tamper pin is connected to the GND, the result is "00". Otherwise Tamper Flag will be "AA" by factory setting value. 
1. **Time-Stamp:** 4-bytes randomly increasing value (each step of increasing is 1 to 255). This content always increasing each time the tag has been read.
1. **Rolling Code:** 4-bytes of stream cipher ([Mickey V1](http://www.ecrypt.eu.org/stream/ciphers/mickey/mickey.pdf)) with input from Time-stamp value as IV.

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

