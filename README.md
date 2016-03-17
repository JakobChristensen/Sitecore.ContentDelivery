Content Delivery Service for Sitecore CMS.

Requests to the Content Delivery service are prefixed with "/cd".

Parameters can either be passed as query string parameters or as form post fields.

# Security
Either pass username and password or pass an access token (not implemented yet).
                                    
``` 
/cd/master?username=Admin&password=b
```
or
``` 
/cd/master?token=test
```

# Data Stores
By default the Content Delivery service has 3 data stores: web, master and core.

Get "master" data store
``` 
/cd/master
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
  "smallIcon": "/sitecore/shell/themes/standard/Images/database_master.png",
  "largeIcon": "/sitecore/shell/themes/standard/Images/database_master.png",
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
    "smallIcon": "/temp/IconCache/Applications/16x16/Document.png",
    "largeIcon": "/temp/IconCache/Applications/32x32/Document.png",
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
/cd/master/items
```

Get "Home" item:
``` 
/cd/master/items/sitecore/content/Home
``` 
or
``` 
/cd/master/items/{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}
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
  "smallIcon": "/temp/IconCache/Network/16x16/home.png",
  "largeIcon": "/temp/IconCache/Network/32x32/home.png",
  "path": "/sitecore/content/Home",
  "templateId": "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}",
  "templateName": "Sample Item",
  "childCount": 3
}
```

Get items in the master data store where Title is "Welcome":
``` 
/cd/master/items?Title=Welcome
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
      "smallIcon": "/temp/IconCache/Network/16x16/home.png",
      "largeIcon": "/temp/IconCache/Network/32x32/home.png",
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
/cd/master/items?Title[not]=Welcome
```

Get items in the master data store where Data is one of "Value A|Value B|Value C":
``` 
/cd/master/items?Data[in]=Value A|Value B|Value C
```

Get items in the master data store where Data is not one of "Value A|Value B|Value C":
``` 
/cd/master/items?Data[not in]=Value A|Value B|Value C
```

Get items in the master data store where Data has a value (not blank):
``` 
/cd/master/items?Data[has]=
```

Full text search (includes all fields):
``` 
/cd/master/items?query=Text to search for
```

Get item with children in 2 levels:
``` 
/cd/master/items/sitecore/content/Home?levels=2
```

Get first 10 items in the master data store where Title is "Welcome":
``` 
/cd/master/items?Title=Welcome&take=10
```

Get 10 items skipping first 100 items in the master data store where Title is "Welcome":
``` 
/cd/master/items?Title=Welcome&take=10&skip=100
```

## Operators

* = - where field equals value
* [not]= - where field not equals value
* [in]= - where field in list of value
* [not in]= - where field not in list of value
* [has]= - where field has a value

## filters

* take - take first n items
* skip - skip first n items
* levels - including children in n levels
* query - full text search

# Fields

Get Title, Text and __Icon fields:
``` 
/cd/master/items/sitecore/content/Home?fields=Title, Text, __Icon
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
  "smallIcon": "/temp/IconCache/Network/16x16/home.png",
  "largeIcon": "/temp/IconCache/Network/32x32/home.png",
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
/cd/master/items/sitecore/content/Home?fields=Title, Text, __Icon[icon48x48]
``` 

Get all fields:
``` 
/cd/master/items/sitecore/content/Home?fields=*
``` 

Get all fields including system fields:
``` 
/cd/master/items/sitecore/content/Home?fields=*&systemfields=true
``` 

Get all fields including field info:
``` 
/cd/master/items/sitecore/content/Home?fields=Title&fieldinfo=true
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
  "smallIcon": "/temp/IconCache/Network/16x16/home.png",
  "largeIcon": "/temp/IconCache/Network/32x32/home.png",
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

# Children

Get children of "Home" item:
``` 
/cd/master/children/sitecore/content/Home
```

Get children of "Home" item including 2 levels:
``` 
/cd/master/children/sitecore/content/Home?level=2
```

# Templates

Get all templates:
``` 
/cd/master/templates
```

Get "Sample Item" template:
``` 
/cd/master/templates/sitecore/templates/Sample/Sample Item
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
  "smallIcon": "/temp/IconCache/Applications/16x16/document.png",
  "largeIcon": "/temp/IconCache/Applications/32x32/document.png",
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
/cd/master/templates/sitecore/templates/Sample/Sample Item?systemfields=true
```


# Bundling
Bundling is only supported when using form posts. 

Pass each Url as a form post value.

``` js
var data = {
    "request1": "/cd/master/children/sitecore/content/Home?level=2",
    "request2": "/cd/master/items/sitecore/content/Home?fields=*&fieldinfo=true"
};
```

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


