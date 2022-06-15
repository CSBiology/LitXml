# LitXml
Small FSharp based DSL for writing **lit**eral **xml**

# Usage

- [Basic example](#basic-example)
- [Mixing literal and programmatic logic](#mixing-literal-and-programmatic-logic)
- [Optional and required values](#optional-and-required-values)
	- [Basics](#basics)
	- [Small examples](#small-examples)
	- [Nesting](#nesting)
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
## Optional and required values

### Basics

In some cases, whether a `value` actually exists is not known when writing the code, but only when running it. For how to handle these values, basically two approaches exist: 

- The value was `missing` and `required`
    In this case, not only the value but everything else should be missing from the final xml

- The value was `missing` and `optional`
    In this case, only the value should be missing from the final xml

For these cases the `!!` and `!?` operators were created. As input they receive an `Expr<'T>`. This `Expr<'T>` type can basically be used to `delay a computation`. This is important as we're interested in whether a value exists or not and we want to find that out in a controlled way, not crashing the program. Creating such a delayed expression can be done with the following syntax `<@ "expression" @>`. 

So you could write the following code:

```fsharp
open Microsoft.FSharp.Quotations

!! <@ (Some "value").Value @> // Will result in "Ok (value)"
!! <@ (None        ).Value @> // Will result in "MissingRequired"

!? <@ (Some "value").Value @> // Will result in "Ok (value)"
!? <@ (None        ).Value @> // Will result in "MissingOptional"
```

### Small examples

#### Optional value missing 
We use the `!?` operator here, so each subelement here is optional. The second subelement should return no value.
```fsharp
elem "myxml" {
        elem "ThisFieldWillWork" {
            !? <@ "abc" @>
        }
        elem "ThisFieldWillNotWork" {
            !? <@ None.Value @>
        }
        elem "ThisFieldWillWork" {
            !? <@ "abc" @>
        }
    }
```

-> 
```xml
<?xml version="1.0" encoding="utf-16"?>
<myxml>
  <ThisFieldWillWork>abc</ThisFieldWillWork>
  <ThisFieldWillWork>abc</ThisFieldWillWork>
</myxml>
```
Note that the `optional` `ThisFieldWillNotWork` block is missing, but the other blocks are still there.


#### Required value missing 
```fsharp
elem "myxml" {
        elem "ThisFieldWillWork" {
            !! <@ "abc" @>
        }
        elem "ThisFieldWillNotWork" {
            !! <@ None.Value @>
        }
        elem "ThisFieldWillWork" {
            !! <@ "abc" @>
        }
    }
```
-> 
```xml
```
Note that the whole xml is missing, as a `required` block was missing.

### Nesting

`MissingRequired` and `MissingOptional` elements behave consistently on each layer. 
- The layer above a `MissingOptional` will only become a `MissingOptional` itself, if there are no other non missing elements.
- The layer above a `MissingRequired` will always become a `MissingRequired` itself. 



```fsharp
elem "myxml" {
    elem "ThisFieldWillWork" {
        !! <@ "abc" @>
    }
    elem "RequiredBelow" {
        elem "ThisFieldWillNotWork" {
            !! <@ None.Value @>
        }
    }
}
```
-> 
```xml
```
The `MissingRequired` "ThisFieldWillNotWork" lead to the "RequiredBelow" element being also `MissingRequired` so the whole xml get's lost

#### Using opt

But what if we want a subelement to be required for an element, but this element being optional for the element above?
For this we use the `opt` keyword. Which transforms a `MissingRequired` to a `MissingOptional`:

```fsharp
elem "myxml" {
        elem "ThisFieldWillWork" {
            !! <@ "abc" @>
        }
        opt (elem "RequiredBelow" {
            elem "ThisFieldWillNotWork" {
                !! <@ None.Value @>
            }
        })
    }
```
-> 
```xml
<?xml version="1.0" encoding="utf-16"?>
<myxml>
  <ThisFieldWillWork>abc</ThisFieldWillWork>
</myxml>
```
