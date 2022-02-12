namespace LitXml

[<AutoOpen>]
module DSL =

    let inline elem name = ElementBuilder(name)
    
    let inline attr name value = 
        Attr(fun tb ->
            tb.WriteAttributeString(name, string value)
        )
    
    let inline value (s: 'a) : Value =
        Value(fun tb ->
            tb.WriteString(string s)
        )

