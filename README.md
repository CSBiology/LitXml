# LitXml
Small FSharp based DSL for writing **lit**eral **xml**

# Usage
```fsharp
#r "nuget: LitXml"

open System.IO
open LitXml

let test =

        elem "MyXml" {
            attr "linksTo" "LebestWebsite.de"
            attr "isPartOf" "Dadabland"
            elem "FirstSegment" {
                attr "Importance" "10"
                value "This segment is very important"
                elem "avb"
            }
            elem "SecondSegment" {
                attr "Importance" "2"
                value "This segment is not very important"
            }
        }

ElementBuilder.WriteTo(Path.Combine(__SOURCE_DIRECTORY__,"test.xml"),test)
```

-> 

```xml
<?xml version="1.0" encoding="utf-8"?>
<MyXml linksTo="LebestWebsite.de" isPartOf="Dadabland">
	<FirstSegment Importance="10">This segment is very important<avb/>
	</FirstSegment>
	<SecondSegment Importance="2">This segment is not very important</SecondSegment>
</MyXml>
```
