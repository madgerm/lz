using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LzGame;

internal static class AssetData
{
    internal static Texture2D LoadGrass(GraphicsDevice d)
    {
        var c=new Canvas(d,108,71); c.Diamond(54,35,50,29,new Color(58,91,25));
        for(var i=0;i<80;i++){var x=14+(i*37)%80;var y=20+(i*19)%28;c.Rect(x,y,1,4,new Color(103+(i%3)*20,125+(i%4)*15,29));}
        c.Line(13,35,54,63,new Color(41,55,19));c.Line(54,63,95,35,new Color(41,55,19));return c.Texture;
    }

    internal static Texture2D LoadTree(GraphicsDevice d)
    {
        var c=new Canvas(d,78,86);c.Rect(35,55,8,27,new Color(72,43,20));
        c.Triangle(39,5,8,65,new Color(20,62,20));c.Triangle(39,18,5,69,new Color(23,78,23));c.Triangle(39,31,9,72,new Color(18,68,20));
        c.Rect(29,38,4,5,new Color(58,102,28));c.Rect(48,30,5,5,new Color(73,116,31));return c.Texture;
    }

    internal static Texture2D LoadCrate(GraphicsDevice d)
    {
        var c=new Canvas(d,67,63);c.Diamond(33,42,27,14,new Color(72,43,18));c.Rect(12,14,42,30,new Color(116,68,28));
        c.Line(12,14,54,44,new Color(55,31,15));c.Line(54,14,12,44,new Color(55,31,15));c.Rect(10,12,46,4,new Color(61,36,17));c.Rect(10,42,46,4,new Color(61,36,17));return c.Texture;
    }

    internal static Texture2D LoadHouse(GraphicsDevice d)
    {
        var c=new Canvas(d,165,133);c.Rect(28,55,108,65,new Color(83,72,58));c.Rect(41,75,23,45,new Color(92,50,23));c.Rect(87,72,28,25,new Color(20,24,21));
        c.Triangle(82,7,18,60,new Color(91,48,18));c.Line(21,60,82,7,new Color(47,29,17));c.Line(82,7,145,60,new Color(47,29,17));
        for(var i=0;i<8;i++)c.Line(31+i*14,52,75+i*7,14,new Color(129,73,27));c.Rect(118,18,18,40,new Color(64,58,49));
        c.Rect(18,118,128,6,new Color(36,48,25));return c.Texture;
    }

    internal static Texture2D LoadPlayer(GraphicsDevice d)
    {
        var c=new Canvas(d,76,105);c.Ellipse(38,24,10,11,new Color(178,139,92));c.Rect(29,35,18,35,new Color(53,48,42));
        c.Rect(25,39,5,29,new Color(78,63,42));c.Rect(47,39,5,29,new Color(78,63,42));c.Rect(29,69,7,28,new Color(38,34,31));c.Rect(41,69,7,28,new Color(38,34,31));
        c.Rect(24,36,28,7,new Color(108,73,32));c.Line(51,48,64,78,new Color(170,170,155));c.Rect(23,54,8,18,new Color(77,52,24));return c.Texture;
    }

    internal static Texture2D LoadZombie(GraphicsDevice d)
    {
        var c=new Canvas(d,75,105);c.Ellipse(37,24,11,12,new Color(107,126,73));c.Rect(28,36,20,35,new Color(58,69,42));
        c.Rect(22,40,7,32,new Color(84,103,59));c.Rect(48,40,7,32,new Color(84,103,59));c.Rect(28,70,8,28,new Color(42,46,36));c.Rect(41,70,8,28,new Color(42,46,36));
        c.Rect(31,21,3,3,new Color(180,20,15));c.Rect(42,21,3,3,new Color(180,20,15));c.Rect(29,48,19,8,new Color(80,36,29));return c.Texture;
    }

    private sealed class Canvas
    {
        private readonly GraphicsDevice d; private readonly Color[] p; private readonly int w,h;
        internal Texture2D Texture{get;}
        internal Canvas(GraphicsDevice d,int w,int h){this.d=d;this.w=w;this.h=h;p=new Color[w*h];Texture=new Texture2D(d,w,h);}
        private void Set(int x,int y,Color c){if(x>=0&&y>=0&&x<w&&y<h)p[y*w+x]=c;}
        internal void Rect(int x,int y,int ww,int hh,Color c){for(var yy=y;yy<y+hh;yy++)for(var xx=x;xx<x+ww;xx++)Set(xx,yy,c);Flush();}
        internal void Ellipse(int cx,int cy,int rx,int ry,Color c){for(var y=-ry;y<=ry;y++)for(var x=-rx;x<=rx;x++)if(x*x/(float)(rx*rx)+y*y/(float)(ry*ry)<=1)Set(cx+x,cy+y,c);Flush();}
        internal void Diamond(int cx,int cy,int rx,int ry,Color c){for(var y=-ry;y<=ry;y++)for(var x=-rx;x<=rx;x++)if(Math.Abs(x)/(float)rx+Math.Abs(y)/(float)ry<=1)Set(cx+x,cy+y,c);Flush();}
        internal void Triangle(int cx,int top,int left,int bottom,Color c){for(var y=top;y<=bottom;y++){var t=(y-top)/(float)Math.Max(1,bottom-top);var r=(int)(t*(cx-left));for(var x=cx-r;x<=cx+r;x++)Set(x,y,c);}Flush();}
        internal void Line(int x0,int y0,int x1,int y1,Color c){var dx=Math.Abs(x1-x0);var sx=x0<x1?1:-1;var dy=-Math.Abs(y1-y0);var sy=y0<y1?1:-1;var e=dx+dy;while(true){Set(x0,y0,c);if(x0==x1&&y0==y1)break;var e2=2*e;if(e2>=dy){e+=dy;x0+=sx;}if(e2<=dx){e+=dx;y0+=sy;}}Flush();}
        private void Flush()=>Texture.SetData(p);
    }
}
