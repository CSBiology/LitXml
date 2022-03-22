namespace LitXml

open System.Xml

open Microsoft.FSharp.Quotations
open Expression

type ElementBuilder(name: string) =

    static member Empty = ok (fun _sb -> ())

    // -- Computation Expression methods --> 

    member inline this.Zero() = ok (fun _sb -> ())

    member _.Name = name

    member this.SignMessages (messages : Message list) : Message list =
        messages
        |> List.map (sprintf "In Element \"%s\": %s" this.Name)

    member inline _.Yield(n: 'a when 'a :> System.IEquatable<'a>) = 
        ok (fun tb ->
            tb.WriteString(string n)
        )

    member inline _.Yield(n: XmlPart) = n
        
    member inline _.Yield(b: ElementBuilder) =
        ok (fun tb ->
            tb.WriteStartElement(b.Name)
            tb.WriteEndElement()
        )

    member inline _.Yield(b: Expr<XmlPart>) =
        try 
            eval<XmlPart> b                   
        with
        | err -> MissingRequired([err.Message])

    member inline this.YieldFrom(ns: seq<XmlPart>) =   
        ns
        |> Seq.fold (fun state we ->
            this.Combine(state,we)

        ) ElementBuilder.Empty

    member inline this.YieldFrom(bs: seq<ElementBuilder>) = 
        ok (fun tb ->
            bs
            |> Seq.iter (fun b ->
                tb.WriteStartElement(b.Name)
                tb.WriteEndElement()
            )           
        )

    member inline this.For(vs : seq<'T>, f : 'T -> XmlPart) =
        vs
        |> Seq.map f
        |> this.YieldFrom

    member inline this.For(vs : seq<'T>, f : 'T -> ElementBuilder) =
        vs
        |> Seq.map f
        |> this.YieldFrom


    member inline this.Run(children: XmlPart) =
        match children with
        | Ok (f,messages) ->
            Ok ((fun tb ->
                tb.WriteStartElement(this.Name)
                f tb
                tb.WriteEndElement()
            ),this.SignMessages messages)
        | MissingOptional messages -> MissingOptional (this.SignMessages messages)
        | MissingRequired messages -> MissingRequired (this.SignMessages messages)

    member this.Combine(wx1: XmlPart, wx2: XmlPart) : XmlPart=
        match wx1,wx2 with
        // If both contain content, combine the content
        | Ok (f1,messages1), Ok (f2,messages2) ->
            Ok (fun tb ->
                f1 tb
                f2 tb
            ,List.append messages1 messages2)

        // If any of the two is missing and was required, return a missing required
        | _, MissingRequired messages2 ->
            MissingRequired (List.append wx1.Messages messages2)

        | MissingRequired messages1, _ ->
            MissingRequired (List.append messages1 wx2.Messages)

        // If only one of the two is missing and was optional, take the content of the functioning one
        | Ok (f1,messages1), MissingOptional messages2 ->
            Ok (fun tb ->
                f1 tb
            ,List.append messages1 messages2)

        | MissingOptional messages1, Ok (f2,messages2) ->
            Ok (fun tb ->
                f2 tb
            ,List.append messages1 messages2)

        // If both are missing and were optional, return a missing optional
        | MissingOptional messages1, MissingOptional messages2 ->
            MissingOptional (List.append messages1 messages2)
        
    member inline _.Delay(n: unit -> Element) = n()


    // -- Writers --> 

    static member writeToWriter (writer : XmlWriter) (element : Element) = 
        element.WriteTo(writer)

    static member writeToStream (stream : System.IO.MemoryStream) (element : Element) = 
        element.WriteTo(stream)
        
    static member writeToStreamWith (settings : XmlWriterSettings) (stream : System.IO.MemoryStream) (element : Element) = 
        element.WriteTo(stream, settings)

    static member writeToPath (path : string) (element : Element) = 
        element.WriteTo(path)

    static member writeToPathWith (settings : XmlWriterSettings) (path : string) (element : Element) = 
        element.WriteTo(path, settings)

    static member writeToString (element : Element) =
        element.WriteToString()

    static member writeToStringWith (settings : XmlWriterSettings) (element : Element) =
        element.WriteToString(settings)
