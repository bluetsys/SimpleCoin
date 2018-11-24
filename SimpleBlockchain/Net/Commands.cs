using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Net
{
    static class Commands
    {
        public const string ClientHello = "HELLO";
        public const string ClientAuthResponse = "AUTH_RESPONSE";
        public const string ClientPublicKeyResponse = "PUBLIC_KEY";
        public const string ClientSignatureResponse = "SIGNATURE";
        public const string ClientAcceptBlockRequest = "ACCEPT_BLOCK";
        public const string ClientAcceptTransactionRequest = "ACCEPT_TRANSACTION";
        public const string ClientGetUnitHashRequest = "GET_UNIT_HASH";
        public const string ClientGetUnitRequest = "GET_UNIT";
        public const string ClientQuitRequest = "QUIT";
        public const string ClientGetUnitCountRequest = "GET_UNIT_COUNT";

        public const string ServerAuthRequest = "REQUIRE_AUTH";
        public const string ServerAuthSuccessfulResponse = "AUTH_SUCCESSFUL";
        public const string ServerAuthFailureResponse = "AUTH_FAILURE";
        public const string ServerBusyResponse = "BUSY";
        public const string ServerProtocolViolationResponse = "INVALID_COMMAND";
        public const string ServerGetUnitResponse = "UNIT";
        public const string ServerGetUnitHashResponse = "UNIT_HASH";
        public const string ServerGetUnitCountResponse = "UNIT_COUNT";
    }
}
