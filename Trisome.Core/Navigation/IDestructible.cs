﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Trisome.Core.Navigation
{
    /// <summary>
    /// Interface for objects that require cleanup of resources prior to Disposal
    /// </summary>
    public interface IDestructible
    {
        /// <summary>
        /// This method allows cleanup of any resources used by your View/ViewModel 
        /// </summary>
        void Destroy();
    }
}
