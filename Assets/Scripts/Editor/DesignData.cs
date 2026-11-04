using System;
namespace ClashRoyaleUI.Editor
{
    [Serializable] public class DesignData { public DesignScreen[] screens; }
    [Serializable] public class DesignScreen { public string id, name, key; public DesignNode[] nodes; }
    [Serializable] public class DesignNode
    {
        public string id, parent, name, type, text, align, valign, asset;
        public float x, y, w, h, rotation, opacity = 1, stroke, radius, size;
        public DesignPaint[] fills, strokes;
        public DesignFont font;
        public DesignLineHeight lineHeight;
    }
    [Serializable] public class DesignFont { public string family, style; }
    [Serializable] public class DesignLineHeight { public string unit; public float value; }
    [Serializable] public class DesignPaint { public string type, imageHash, scaleMode; public float opacity=1; public float[] color, matrix, imageTransform; public DesignStop[] stops; }
    [Serializable] public class DesignStop { public float p; public float[] c; }
}
