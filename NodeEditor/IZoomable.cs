using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NodeEditor
{
    public interface IZoomable
    {
        /// <summary>
        /// Indicates current zoom (scale), higher value means bigger elements
        /// </summary>
        float Zoom { get; set; }
    }
}
