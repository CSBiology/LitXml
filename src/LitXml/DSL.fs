namespace LitXml

open System.Xml
open Microsoft.FSharp.Quotations
open Expression

[<AutoOpen>]
type DSL =

    static member inline elem name = ElementBuilder(name)
    
    static member inline attr name value : Attr = 
        ok (fun tb ->
            tb.WriteAttributeString(name, string value)
        )
    
    static member inline value (s: 'a when 'a :> System.IEquatable<'a>) : Value =
        ok (fun tb -> 
            tb.WriteString(string s)
        )

    static member inline value (s : Expr<'a> when 'a :> System.IEquatable<'a>) : Value =
        try 
            let value = eval<'a> s
            let f = (fun (sb : XmlWriter) -> 
                sb.WriteString(string value)
            )
            ok f            
        with
        | err -> MissingRequired([err.Message])
        
        
    static member opt (elem : Element) = 
        match elem with
        | Ok (f,messages) -> elem
        | MissingOptional (messages) -> Ok((fun tb -> ()),messages)
        | MissingRequired (messages) -> Ok((fun tb -> ()),messages)

    static member opt (elem : Expr<Element>) = 
        try 
            let elem = eval<Element> elem
            match elem with
            | Ok (f,messages) -> elem
            | MissingOptional (messages) -> Ok((fun tb -> ()),messages)
            | MissingRequired (messages) -> Ok((fun tb -> ()),messages)
        with
        | err -> 
            let f = (fun (sb : XmlWriter) -> 
                ()
            )
            Ok(f,[err.Message])

