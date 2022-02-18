# LitXml
Small FSharp based DSL for writing **lit**eral **xml**

# Usage

## Basic example
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
## Mixing literal and programmatic logic

```fsharp
let standardAttributes = 
    List.init 3 (fun i ->
        attr $"Key{i}" $"Value{i}"
    )

let collectionTest = 
    elem "MyXml" {
        // You can use yield! to add multiple subelements (inclduing attributes) to an element
        yield! standardAttributes 
        elem "Numbers" {
            // You can use for loops to programmatically add multiple subelements (inclduing attributes) to an element
            for i in [1 .. 10] do 
                yield elem "Number" {
                    value i
                }
        }
    }
```

->

```xml
<?xml version="1.0" encoding="utf-8"?>
<MyXml Key0="Value0" Key1="Value1" Key2="Value2">
	<Numbers>
		<Number>1</Number>
		<Number>2</Number>
		<Number>3</Number>
		<Number>4</Number>
		<Number>5</Number>
		<Number>6</Number>
		<Number>7</Number>
		<Number>8</Number>
		<Number>9</Number>
		<Number>10</Number>
	</Numbers>
</MyXml>
```
