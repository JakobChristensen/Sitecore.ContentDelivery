Content Delivery Service for Sitecore CMS

#Examples:

Get "master" data store
``` 
/cd/master
```

Get all templates:
``` 
/cd/master/templates
```

Get "Sample Item" template:
``` 
/cd/master/templates/sitecore/templates/Common/Sample Item
```

Get all items:
``` 
/cd/master/items
```

Get "Home" item:
``` 
/cd/master/items/sitecore/content/Home
```

Get children of "Home" item:
``` 
/cd/master/children/sitecore/content/Home
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


