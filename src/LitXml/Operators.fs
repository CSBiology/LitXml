namespace LitXml

open System.Xml
open Microsoft.FSharp.Quotations
open Expression

[<AutoOpen>]
module Operators = 
    
    /// Required value operator
    ///
    /// If expression does fail, returns a missing required value
    let inline (!!) (s : Expr<'a>) : Value =
        try 
            let value = eval<'a> s
            let f = (fun (sb : XmlWriter) -> 
                sb.WriteString(string value)
            )
            ok f            
        with
        | err -> MissingRequired([err.Message])

    /// Optional value operator
    ///
    /// If expression does fail, returns a missing optional value
    let inline (!?) (s : Expr<'a>) : Value =
        try 
            let value = eval<'a> s
            let f = (fun (sb : XmlWriter) -> 
                sb.WriteString(string value)
            )
            ok f            
        with
        | err -> MissingOptional([err.Message])