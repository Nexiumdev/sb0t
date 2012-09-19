﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iconnect
{
    /// <summary>Server Callback</summary>
    public interface IHostApp
    {
        /// <summary>User pool</summary>
        IPool Users { get; }
        /// <summary>Folder to save items for this extension</summary>
        String DataPath { get; }
        /// <summary>Chatroom statistics</summary>
        IStats Stats { get; }
        /// <summary>Chatroom properties</summary>
        IRoom Room { get; }
        /// <summary>Hashlink encoder / decoder</summary>
        IHashlink Hashlinks { get; }
        /// <summary>Compression utility</summary>
        ICompression Compression { get; }
    }
}
