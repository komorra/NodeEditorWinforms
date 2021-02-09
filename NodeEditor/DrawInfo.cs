using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace NodeEditor
{
    /// <summary>
    /// Class used to provide custom drawing elements to node editor, such connection styles, node graphics etc.
    /// </summary>
    public class DrawInfo
    {
        private static Pen boldPen;

        /// <summary>
        /// Gets pen used to draw connections between nodes.
        /// </summary>
        /// <param name="dataType">Type indicating data type passing through connection</param>
        /// <param name="isConnecting">If user is currently dragging connection between nodes</param>
        /// <returns>Pen to draw line</returns>
        public virtual Pen GetConnectionStyle(Type dataType, bool isConnecting)
        {
            if(isConnecting)
            {
                return boldPen ?? (boldPen = new Pen(Brushes.Black, 3));
            }
            return Pens.Black;
        }
    }
}
