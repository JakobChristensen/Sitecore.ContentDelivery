Content Delivery Service for Sitecore CMS.

Requests to the Content Delivery service are prefixed with "/sitecore/get".

Parameters can either be passed as query string parameters or as form post values.

# Security
Pass username and password.
                                    
``` 
/sitecore/get/master?username=Admin&password=b
```

# Data Stores
By default the Content Delivery service has 3 data stores: web, master and core.

Get "master" data store
``` 
/sitecore/get/master
```

Returns:
```js
{
  "metadata": {
    "version": "1",
    "user": "extranet\\admin",
    "language": "en"
  },
  "type": "items",
  "name": "master",
  "icon16x16": "/sitecore/shell/themes/standard/Images/database_master.png",
  "icon32x32": "/sitecore/shell/themes/standard/Images/database_master.png",
  "languages": [
    {
      "name": "en",
      "displayName": "English : English",
      "cultureName": "en"
    }
  ],
  "root": {
    "id": "{11111111-1111-1111-1111-111111111111}",
    "name": "sitecore",
    "displayName": "sitecore",
    "database": "master",
    "dataStore": "master",
    "icon16x16": "/temp/IconCache/Applications/16x16/Document.png",
    "icon32x32": "/temp/IconCache/Applications/32x32/Document.png",
    "path": "/sitecore",
    "templateId": "{C6576836-910C-4A3D-BA03-C277DBD3B827}",
    "templateName": "Root",
    "childCount": 6
  }
}
```

# Items

Get all items in the master data store:
``` 
/sitecore/get/items/master
```

Get "Home" item:
``` 
/sitecore/get/item/master/sitecore/content/Home
``` 
or
``` 
/sitecore/get/item/master/{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}
```

Returns:
```js
{
  "metadata": {
    "version": "1",
    "user": "sitecore\\admin",
    "language": "en"
  },
  "id": "{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}",
  "name": "Home",
  "displayName": "Home",
  "database": "master",
  "dataStore": "master",
  "icon16x16": "/temp/IconCache/Network/16x16/home.png",
  "icon32x32": "/temp/IconCache/Network/32x32/home.png",
  "path": "/sitecore/content/Home",
  "templateId": "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}",
  "templateName": "Sample Item",
  "childCount": 3
}
```

Get items in the master data store where Title is "Welcome":
``` 
/sitecore/get/items/master?Title=Welcome
```

Returns:
```js
{
  "metadata": {
    "version": "1",
    "user": "sitecore\\admin",
    "language": "en"
  },
  "count": 1,
  "skip": 0,
  "take": 0,
  "items": [
    {
      "id": "{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}",
      "name": "Home",
      "displayName": "Home",
      "database": "master",
      "dataStore": "master",
      "icon16x16": "/temp/IconCache/Network/16x16/home.png",
      "icon32x32": "/temp/IconCache/Network/32x32/home.png",
      "path": "/sitecore/content/Home",
      "templateId": "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}",
      "templateName": "Sample Item",
      "childCount": 3
    }
  ]
}
```

Get items in the master data store where Title is not "Welcome":
``` 
/sitecore/get/items/master?Title[not]=Welcome
```

Get items in the master data store where Data is one of "Value A|Value B|Value C":
``` 
/sitecore/get/items/master?Data[in]=Value A|Value B|Value C
```

Get items in the master data store where Data is not one of "Value A|Value B|Value C":
``` 
/sitecore/get/items/master?Data[not in]=Value A|Value B|Value C
```

Get items in the master data store where Data has a value (not blank):
``` 
/sitecore/get/items/master?Data[has]=
```

Full text search (includes all fields):
``` 
/sitecore/get/items/master?query=Text to search for
```

Get item:
``` 
/sitecore/get/item/master/sitecore/content/Home
```

Get item with children in 2 levels:
``` 
/sitecore/get/item/master/sitecore/content/Home?children=2
```

Get first 10 items in the master data store where Title is "Welcome":
``` 
/sitecore/get/items/master?Title=Welcome&take=10
```

Get 10 items skipping first 100 items in the master data store where Title is "Welcome":
``` 
/sitecore/get/items/master?Title=Welcome&take=10&skip=100
```

## Operators

* = - where field equals value
* [not]= - where field not equals value
* [in]= - where field in list of value
* [not in]= - where field not in list of value
* [has]= - where field has a value

## Filters

* lang - language
* ver - version (only for get item)
* take - take first n items
* skip - skip first n items
* children - including children in n levels
* query - full text search


# Fields

Get Title, Text and __Icon fields:
``` 
/sitecore/get/item/master/sitecore/content/Home?fields=Title, Text, __Icon
``` 

Returns:
```js
{
  "metadata": {
    "version": "1",
    "user": "sitecore\\admin",
    "language": "en"
  },
  "id": "{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}",
  "name": "Home",
  "displayName": "Home",
  "database": "master",
  "dataStore": "master",
  "icon16x16": "/temp/IconCache/Network/16x16/home.png",
  "icon32x32": "/temp/IconCache/Network/32x32/home.png",
  "path": "/sitecore/content/Home",
  "templateId": "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}",
  "templateName": "Sample Item",
  "childCount": 3,
  "fields": {
    "Title": "Welcome",
    "Text": "...",
    "__Icon": "Network/16x16/home.png"
  }
}
``` 

Get Title, Text and __Icon fields with __Icon formatted as an icon Url:
``` 
/sitecore/get/item/master/sitecore/content/Home?fields=Title, Text, __Icon[icon48x48]
``` 

Get all fields:
``` 
/sitecore/get/item/master/sitecore/content/Home?fields=*
``` 

Get all fields including system fields:
``` 
/sitecore/get/item/master/sitecore/content/Home?fields=*&systemfields=true
``` 

Get all fields including field info:
``` 
/sitecore/get/item/master/sitecore/content/Home?fields=Title&fieldinfo=true
``` 

```js
{
  "metadata": {
    "version": "1",
    "user": "sitecore\\admin",
    "language": "en"
  },
  "id": "{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}",
  "name": "Home",
  "displayName": "Home",
  "database": "master",
  "dataStore": "master",
  "icon16x16": "/temp/IconCache/Network/16x16/home.png",
  "icon32x32": "/temp/IconCache/Network/32x32/home.png",
  "path": "/sitecore/content/Home",
  "templateId": "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}",
  "templateName": "Sample Item",
  "childCount": 3,
  "fields": [
    {
      "id": "{75577384-3C97-45DA-A847-81B00500E250}",
      "name": "Title",
      "displayName": "Title",
      "value": "Welcome"
    }
  ]
}
``` 

## Formatters

* icon16x16 - icon url with size 16x16
* icon24x24 - icon url with size 24x24
* icon32x32 - icon url with size 32x32
* icon48x48 - icon url with size 48x48
* url - Url including host name
* img - image url

## Special fields

* id - item ID
* name - item name
* templateid - template ID
* templatename - template name
* path - item path

# Children

Get children of "Home" item:
``` 
/sitecore/get/children/master/sitecore/content/Home
```

Get children of "Home" item including 2 levels:
``` 
/sitecore/get/children/master/sitecore/content/Home?children=2
```

# Templates

Get all templates:
``` 
/sitecore/get/templates/master
```

Get "Sample Item" template:
``` 
/sitecore/get/template/master/sitecore/templates/Sample/Sample Item
```

Returns:
```js
{
  "metadata": {
    "version": "1",
    "user": "sitecore\\admin",
    "language": "en"
  },
  "id": "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}",
  "name": "Sample Item",
  "displayName": "Sample Item",
  "database": "master",
  "dataStore": "master",
  "icon16x16": "/temp/IconCache/Applications/16x16/document.png",
  "icon32x32": "/temp/IconCache/Applications/32x32/document.png",
  "path": "/sitecore/templates/Sample/Sample Item",
  "templateId": "{AB86861A-6030-46C5-B394-E8F99E8B87DB}",
  "templateName": "Template",
  "childCount": 2,
  "fields": [
    {
      "id": "{75577384-3C97-45DA-A847-81B00500E250}",
      "name": "Title",
      "displayName": "Title",
      "type": "text",
      "source": "",
      "sharing": "versioned",
      "section": "Data",
      "kind": "inherited"
    },
    {
      "id": "{A60ACD61-A6DB-4182-8329-C957982CEC74}",
      "name": "Text",
      "displayName": "Text",
      "type": "Rich Text",
      "source": "/sitecore/system/Settings/Html Editor Profiles/Rich Text Default",
      "sharing": "versioned",
      "section": "Data",
      "kind": "inherited"
    }
  ]
}
``` 

Get "Sample Item" template including system fields:
``` 
/sitecore/get/template/master/sitecore/templates/Sample/Sample Item?systemfields=true
```

# Bundling
Bundling is only supported when using form posts. 

Pass each Url as a form post value.

``` js
var data = {
    "request1": "/sitecore/get/children/master/sitecore/content/Home?children=2",
    "request2": "/sitecore/get/item/master/sitecore/content/Home?fields=*&fieldinfo=true"
};

$.post("/sitecore/get", data, function(result) {
        console.log(result);
    }
)
```

# Sitecore.WebServer
The Sitecore WebServer wraps the Sitecore Content Delivery service in a small website that can be run with IIS Express.

The web server does not include Sitecore, so Sitecore databases cannot be accessed. Instead the Content Delivery service 
loads Json files from the App_Data/DataStores/ directory. 

This means that you can use the web service without actually running Sitecore!

To start the website using IIS Express, use:
```
"c:\program files (x86)\iis express\iisexpress" /path:<absolute path to the directory where the website is located>
```

See also the `run.cmd` file.

To dump a database to a Json file for use with the Sitecore WebServer, make a request to:
``` 
/sitecore/get/dump/master
```

Save the output to a .json file in /App_Data/DataStores.

# CORS
To enable CORS, drop this in the web.config:

```xml
<configuration>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="Access-Control-Allow-Origin" value="http://www.domain.com" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
``` 

