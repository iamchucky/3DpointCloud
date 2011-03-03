using System;
using System.Windows;


namespace Magic.Datastructures
{
    public interface IQuadObject
    {
        Rect Bounds { get; }
        event EventHandler BoundsChanged;
    }
}