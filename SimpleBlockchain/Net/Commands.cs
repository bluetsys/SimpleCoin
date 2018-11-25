using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Net
{
    static class Commands
    {
        public const string ClientAcceptTransactionRequest = "TRANSACTION";
        public const string ClientAcceptBlockRequest = "BLOCK";

        public const string ServerOkResponse = "OK";
        public const string ServerFailureResponse = "FAILURE";
    }
}
