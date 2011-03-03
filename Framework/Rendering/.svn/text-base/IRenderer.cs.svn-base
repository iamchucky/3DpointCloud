using System;
namespace Magic.Rendering
{
    interface IRenderer
    {
        void AddRenderable(IRender renderable);
        event EventHandler AfterRendering;
        bool AngleMode { get; set; }
        event EventHandler BeforeRendering;
        void ClearBuffers();
        GLUtility.GLCamera CurrentCamera { get; }
        void OnFormShown();
        bool PanMode { get; set; }
        void Recenter();
        bool RulerMode { get; set; }
        System.Drawing.PointF ScreenToWorld(System.Drawing.PointF screenCoord);
        void ShowChase();
        void ShowFree();
        void ShowOrtho();
        void UpdateOffset(Magic.Common.RobotPose p);
        void UpdateOffset(float x, float y, float headingOff);
        void ZoomIn();
        void ZoomOut();

    }
}
