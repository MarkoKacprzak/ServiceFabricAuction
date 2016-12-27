namespace SFAuction.JsonRpc
{
    public enum JsonRpcError : int
    {
        Parse = -32700,            // Parse error; Invalid JSON was received by the server.
        InvalidRequest = -32600,   // Invalid Request; The JSON sent is not a valid Request object.
        MethodNotFound = -32601,   // Method not found; The method does not exist / is not available.
        InvalidParameters = -32602,// Invalid params; Invalid method parameter(s).
        Internal = -32603,         // error; Internal JSON-RPC error.
        ReservedLow = -32099,      // -32000 to -32099; Server error; Reserved for implementation-defined server-errors.
        ReserverHigh = -32000
    }
}