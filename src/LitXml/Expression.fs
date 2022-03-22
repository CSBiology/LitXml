namespace LitXml

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers

module Expression =

    let eval<'T> q = LeafExpressionConverter.EvaluateQuotation q :?> 'T
