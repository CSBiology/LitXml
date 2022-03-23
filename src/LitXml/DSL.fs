namespace LitXml

open System.Xml
open Microsoft.FSharp.Quotations
open Expression

[<AutoOpen>]
type DSL =

    /// Create an xml element with given name
    static member inline elem name = ElementBuilder(name)
    
    /// Create an xml attriubte with given name and value
    static member inline attr name value : Attr = 
        ok (fun tb ->
            tb.WriteAttributeString(name, string value)
        )
    
    /// Create an xml value from the given value
    static member inline value (s : string) : Value =
        ok (fun tb -> 
            tb.WriteString(s)
        )    

    /// Create an xml value from the given value
    static member inline value<'T when 'T :> System.IFormattable> (v : 'T) : Value =
        ok (fun tb -> 
            tb.WriteString(v.ToString())
        ) 

    /// Create an ok xml value from the given value expression if it succedes. Else returns a missing required.
    static member inline value (s : Quotations.Expr<string>) : Value =
        try 
            let s = Expression.eval<string> s
            ok (fun tb -> 
                tb.WriteString(s)
            )              
        with
        | err -> MissingRequired([err.Message])

    /// Create an ok xml value from the given value expression if it succedes. Else returns a missing required.
    static member inline value (s : Quotations.Expr<'a> when 'a :> System.IFormattable) : Value =
        try 
            let v = Expression.eval<'a> s
            ok (fun tb -> 
                tb.WriteString(v.ToString())
            )            
        with
        | err -> MissingRequired([err.Message])

    /// Transforms any given xml element to an ok.
    static member opt (elem : Element) = 
        match elem with
        | Ok (f,messages) -> elem
        | MissingOptional (messages) -> Ok((fun tb -> ()),messages)
        | MissingRequired (messages) -> Ok((fun tb -> ()),messages)

    /// Transforms any given xml element expression to an ok.
    static member opt (elem : Expr<Element>) = 
        try 
            let elem = eval<Element> elem
            DSL.opt elem
        with
        | err -> 
            let f = (fun (sb : XmlWriter) -> ())
            Ok(f,[err.Message])
