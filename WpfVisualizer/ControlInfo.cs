using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfVisualizer
{
    [Serializable]
    public class ControlInfo
    {
        public string ControlType { get; set; }
        public Size Size { get; set; }
        public Point Position { get; set; }

        public string Text { get; set; }

        public int Depth { get; set; }

        public string Name { get; set; }

        public List<ControlInfo> Children { get; set; }

        public string TexturePath { get; set; }

        public ControlInfo()
        {
            Children = new List<ControlInfo>();
        }
    }
}
