﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Configs
{
    public interface IWalletManagerConfig
    {
        string WalletDirectoryPath { get; }
    }
}
