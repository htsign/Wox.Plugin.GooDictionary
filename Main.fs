namespace Wox.Plugin.GooDictionary

open System.Diagnostics
open System.Web

open AngleSharp.Html.Parser
open Wox.Plugin
open Wox.Plugin.GooDictionary.Net

type Main() =
    let Host = "https://dictionary.goo.ne.jp"
    let EspecialKeywords =
        [
            "%26", "%252526"
            "%2F", "%25252F"
        ]

    let mutable webClient : WebClientEx = Unchecked.defaultof<_>
    let htmlParser = HtmlParser()

    interface IPlugin with
        member _.Init context =
            webClient <- new WebClientEx(context.Proxy)

        member this.Query query =
            let query = EspecialKeywords |> List.fold (fun acc (x, y) -> String.replace x y acc) (HttpUtility.UrlEncode query.Search)
            let html = webClient.DownloadString $"{Host}/srch/en/{query}/m0u/"
            let document = htmlParser.ParseDocument html

            match webClient.LastAccessUri with

            // true if response is redirected to word page directly
            | Some url when (string url).[Host.Length ..].StartsWith "/word/" ->
                let parents = document.GetElementsByClassName "meanging"

                // remove all unnecessary elements
                parents
                |> Seq.collect (fun p -> p.QuerySelectorAll "script, div.examples")
                |> Seq.iter (fun element -> element.Remove())

                let trimAndMergeLines s =
                    String.splitWithChar '\n' s
                    |> List.map String.trim
                    |> List.filter (not << System.String.IsNullOrWhiteSpace)
                    |> String.concat " "

                let titles =
                    parents
                    |> Seq.collect (fun p -> p.GetElementsByClassName "basic_title")
                    |> Seq.map (fun el -> String.trim el.TextContent)
                    |> Seq.toArray
                let contents =
                    parents
                    |> Seq.collect (fun p -> p.GetElementsByClassName "content-box-ej")
                    |> Seq.map (fun el -> trimAndMergeLines el.TextContent)
                    |> Seq.toArray

                titles
                |> Array.mapi (fun i title -> Result(Title = title, SubTitle = contents.[i], Score = 2))
                |> ResizeArray
            
            | _ ->
                // list of meanings
                let listItems = document.QuerySelectorAll ".search-list .content_list > li"

                listItems
                |> Seq.map (fun listItem ->
                    let title = listItem.QuerySelector(".title").TextContent |> String.trim
                    let content = listItem.QuerySelector(".text").TextContent |> String.trim
                    let href = Host + listItem.QuerySelector("a[href]").GetAttribute("href")

                    Result(Title = title, SubTitle = content, Action = (fun _ -> Process.Start href |> ignore; true), Score = 1))
                |> ResizeArray
