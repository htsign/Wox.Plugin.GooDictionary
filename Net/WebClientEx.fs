namespace Wox.Plugin.GooDictionary.Net

open System.Net
open System.Text

open Wox.Plugin

type WebClientEx(proxy : IHttpProxy) as this =
    inherit WebClient()

    let mutable lastAccessUri : System.Uri option = None

    do
        this.Encoding <- Encoding.UTF8

        if proxy.Enabled then
            this.Proxy <- WebProxy(proxy.Server, proxy.Port)
            this.Proxy.Credentials <- NetworkCredential(proxy.UserName, proxy.Password)

    member this.LastAccessUri
        with get () : System.Uri option = lastAccessUri
        and private set(value) = lastAccessUri <- value

    override this.GetWebResponse request =
        let response = base.GetWebResponse request
        this.LastAccessUri <- Some response.ResponseUri
        response
