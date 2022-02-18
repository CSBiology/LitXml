namespace LitXml

open System.Xml

type Element(f : XmlWriter -> unit) =

    member this.Invoke(xml : XmlWriter) = f xml

type Value(f) =

    inherit Element(f)

type Attr(f) =

    inherit Element(f)

// https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmlwriter?view=net-6.0
type ElementBuilder(name: string) =

    static member Empty = Element(fun _sb -> ())

    member inline this.Zero() = Element(fun _sb -> ())

    member _.Name = name

    member inline _.Yield(n: #Element) = n

    member inline _.YieldFrom(ns: seq<#Element>) = 
        Element(fun tb ->
            Seq.iter (fun (n: #Element) -> n.Invoke(tb)) ns
        )

    member inline _.YieldFrom(bs: seq<ElementBuilder>) = 
        Element(fun tb ->
            bs
            |> Seq.iter (fun b ->
                tb.WriteStartElement(b.Name)
                tb.WriteEndElement()
            )           
        )

    member inline _.Yield(b: ElementBuilder) =
        Element(fun tb ->
            tb.WriteStartElement(b.Name)
            tb.WriteEndElement()
        )

    member inline this.Run(children: #Element) : Element =
        Element(fun tb ->
            tb.WriteStartElement(this.Name)
            children.Invoke(tb)
            tb.WriteEndElement()
        )

    member inline _.Combine(x1: #Element, x2: #Element) =
        Element(fun sb -> 
            x1.Invoke(sb)
            x2.Invoke(sb)
        )

    member inline _.Delay(n: unit -> Element) = n()

    member inline _.For(ns: 'T seq, ex: 'T -> Element) = 
        Element(fun tb ->
            Seq.iter (fun n -> (ex n).Invoke(tb)) ns
        )

    static member WriteTo(stream : System.IO.MemoryStream, element : Element) = 
        let writer = XmlWriter.Create(stream)
        element.Invoke(writer) |> ignore
        writer.Flush()
        writer.Close()

    static member WriteTo(path : string, element : Element) = 
        let writer = XmlWriter.Create(path)
        element.Invoke(writer) |> ignore
        writer.Flush()
        writer.Close()

    static member WriteToString(element : Element) = 
        let tb = System.Text.StringBuilder()
        let writer = XmlWriter.Create(tb)
        element.Invoke(writer) |> ignore
        writer.Flush()
        writer.Close()
        tb.ToString()