namespace LitXml

open System.Xml

/// 
type Message = string

/// Xml node with instructions about xml elements, values and attributes to be written by an xml writer.
[<AutoOpen>]
type XmlPart =
    /// Node could be computed and contains writer instructions.
    | Ok of (XmlWriter -> unit) * Message list
    /// Node could not be computed, but was optional.
    | MissingOptional of Message list
    /// Node could not be computed, but was required.
    | MissingRequired of Message list

    static member ok (f : XmlWriter -> unit) : XmlPart = XmlPart.Ok (f,[])

    /// Write xml node content to a xml writer.
    member this.Invoke(xml : XmlWriter) = 
        
        match this with 
        | Ok (f,errs) -> f xml
        | MissingOptional errs -> 
            printfn "No function to invoke, Missings:"
            errs 
            |> List.iter (printfn "\t %s")
        | MissingRequired errs -> 
            printfn "No function to invoke, Missings:"
            errs 
            |> List.iter (printfn "\t %s")

    /// Get messages
    member this.Messages =

        match this with 
        | Ok (f,errs) -> errs
        | MissingOptional errs -> errs
        | MissingRequired errs -> errs

    /// Write xml node content to a xml writer and close it.
    member this.WriteTo(writer : XmlWriter) = 
        match this with
        | Ok (f,messages) ->
            f writer
            writer.Flush()
            writer.Close()
        | _ -> ()
    
    /// Write xml node content to a memory stream using a xml writer.
    ///
    /// Writer can be tuned by supplying settings
    member this.WriteTo(stream : System.IO.MemoryStream, ?Settings : XmlWriterSettings) = 
        let settings = Option.defaultValue (XmlWriterSettings()) Settings
        match this with
        | Ok (f,messages) ->
            let writer = XmlWriter.Create(stream,settings)
            this.WriteTo(writer)
        | _ -> ()

    /// Write xml node content to a file path using a xml writer.
    ///
    /// Writer can be tuned by supplying settings
    member this.WriteTo(path : string, ?Settings : XmlWriterSettings) = 
        let settings = Option.defaultValue (XmlWriterSettings()) Settings
        match this with
        | Ok (f,messages) ->
            let writer = XmlWriter.Create(path, settings)
            this.WriteTo(writer)
        | _ -> ()

    /// Write xml node content to a string using a xml writer.
    ///
    /// Writer can be tuned by supplying settings
    member this.WriteToString(?Settings : XmlWriterSettings) = 
        let settings = Option.defaultValue (XmlWriterSettings()) Settings
        match this with
        | Ok (f,messages) ->
            let tb = System.Text.StringBuilder()
            let writer = XmlWriter.Create(tb,settings)
            this.WriteTo(writer)
            tb.ToString()
        | _ -> ""

/// Xml Element
type Element = XmlPart

/// Xml Value
type Value = XmlPart

/// Xml Attribute
type Attr = XmlPart