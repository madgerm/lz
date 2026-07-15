using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LzGame;

public sealed class AssetGame : Game
{
    private const int TileW = 64, TileH = 32, MapW = 70, MapH = 70, HudH = 142;
    private readonly GraphicsDeviceManager graphics;
    private readonly int[,] map = new int[MapW, MapH];
    private readonly List<Zombie> zombies = new();
    private SpriteBatch? batch;
    private Texture2D? pixel, diamond, grass, tree, crate, house, hero, zombie;
    private Vector2 player = new(8, 8), target = new(8, 8), camera;
    private MouseState oldMouse;
    private KeyboardState oldKeys;
    private bool menu, sound = true;
    private int menuIndex;
    private float health = 100f, hitCooldown;
    private static readonly string[] Menu = ["WEITER", "SOUND", "VOLLBILD", "BEENDEN"];

    public AssetGame()
    {
        graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1280, PreferredBackBufferHeight = 720 };
        IsMouseVisible = true;
        Window.Title = "LZ - Asset Build";
    }

    protected override void Initialize()
    {
        for (var y = 0; y < MapH; y++) for (var x = 0; x < MapW; x++)
        {
            var path = Math.Abs(x-y) <= 1 || x == 22 || y == 38;
            var forest = x < 4 || y < 4 || x > MapW-5 || y > MapH-5 || ((x*17+y*11)%23 < 4 && !path);
            map[x,y] = forest ? 2 : path ? 1 : 0;
        }
        zombies.AddRange([new(new(16,15)),new(new(26,29)),new(new(40,42)),new(new(52,20)),new(new(34,57))]);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        batch = new SpriteBatch(GraphicsDevice);
        pixel = new Texture2D(GraphicsDevice,1,1); pixel.SetData([Color.White]);
        diamond = MakeDiamond();
        grass = AssetData.LoadGrass(GraphicsDevice); tree = AssetData.LoadTree(GraphicsDevice);
        crate = AssetData.LoadCrate(GraphicsDevice); house = AssetData.LoadHouse(GraphicsDevice);
        hero = AssetData.LoadPlayer(GraphicsDevice); zombie = AssetData.LoadZombie(GraphicsDevice);
        camera = DesiredCamera();
    }

    protected override void Update(GameTime gameTime)
    {
        var k=Keyboard.GetState(); var m=Mouse.GetState(); var dt=(float)gameTime.ElapsedGameTime.TotalSeconds;
        if (Pressed(k,Keys.Escape)) menu=!menu;
        if (menu) UpdateMenu(k,m); else
        {
            if (m.LeftButton==ButtonState.Pressed && oldMouse.LeftButton==ButtonState.Released && m.Y<GraphicsDevice.Viewport.Height-HudH)
            {
                var t=ScreenToTile(m.Position);
                if(t.HasValue)
                {
                    var click=new Vector2(t.Value.X,t.Value.Y);
                    var z=zombies.Where(q=>q.Alive).OrderBy(q=>Vector2.DistanceSquared(q.Pos,click)).FirstOrDefault();
                    if(z is not null && Vector2.Distance(z.Pos,click)<1.2f && Vector2.Distance(player,z.Pos)<2.3f){z.Health-=40;if(z.Health<=0){z.Alive=false;z.Respawn=6;}}
                    else target=click;
                }
            }
            Move(dt); UpdateZombies(dt);
            camera=Vector2.Lerp(camera,DesiredCamera(),1f-MathF.Exp(-7f*dt));
        }
        hitCooldown=Math.Max(0,hitCooldown-dt); oldMouse=m; oldKeys=k; base.Update(gameTime);
    }

    private void UpdateMenu(KeyboardState k, MouseState m)
    {
        if(Pressed(k,Keys.Up)||Pressed(k,Keys.W)) menuIndex=(menuIndex+3)%4;
        if(Pressed(k,Keys.Down)||Pressed(k,Keys.S)) menuIndex=(menuIndex+1)%4;
        if(Pressed(k,Keys.Enter)||Pressed(k,Keys.Space)) Activate();
        if(m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released)
            for(var i=0;i<4;i++) if(new Rectangle(GraphicsDevice.Viewport.Width/2-180,GraphicsDevice.Viewport.Height/2-80+i*56,360,44).Contains(m.Position)){menuIndex=i;Activate();}
    }

    private void Activate()
    {
        if(menuIndex==0)menu=false; else if(menuIndex==1)sound=!sound; else if(menuIndex==2)graphics.ToggleFullScreen(); else Exit();
    }

    private void Move(float dt)
    {
        var d=target-player; if(d.LengthSquared()<.002f){player=target;return;}
        var s=Vector2.Normalize(d)*3.8f*dt; player=s.LengthSquared()>=d.LengthSquared()?target:player+s;
    }

    private void UpdateZombies(float dt)
    {
        foreach(var z in zombies)
        {
            if(!z.Alive){z.Respawn-=dt;if(z.Respawn<=0){z.Alive=true;z.Health=100;z.Pos=z.Spawn;}continue;}
            var d=Vector2.Distance(z.Pos,player);
            if(d<8&&d>.75f)z.Pos+=Vector2.Normalize(player-z.Pos)*1.25f*dt;
            if(d<.9f&&hitCooldown<=0){health=Math.Max(0,health-12);hitCooldown=.8f;if(health<=0){health=100;player=target=new(8,8);camera=DesiredCamera();}}
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(10,12,8)); if(batch is null||pixel is null||diamond is null)return;
        batch.Begin(samplerState:SamplerState.PointClamp,blendState:BlendState.AlphaBlend);
        DrawWorld(); DrawHud(); if(menu)DrawMenu(); batch.End(); base.Draw(gameTime);
    }

    private void DrawWorld()
    {
        var origin=BaseOrigin+camera;
        for(var sum=0;sum<MapW+MapH-1;sum++) for(var x=0;x<MapW;x++)
        {
            var y=sum-x;if(y<0||y>=MapH)continue;var p=Iso(x,y,origin);
            if(p.X<-180||p.X>GraphicsDevice.Viewport.Width+180||p.Y<-160||p.Y>GraphicsDevice.Viewport.Height-HudH+150)continue;
            var kind=map[x,y];
            if(kind==0&&grass is not null) batch!.Draw(grass,new Rectangle((int)p.X-6,(int)p.Y-20,76,50),Color.White);
            else batch!.Draw(diamond,p,kind==1?new Color(91,70,45):new Color(27,55,29));
            if(kind==2&&(x*7+y*3)%9==0&&tree is not null)batch.Draw(tree,new Rectangle((int)p.X-5,(int)p.Y-70,72,86),Color.White);
            if((x+y)%31==0&&crate is not null)batch.Draw(crate,new Rectangle((int)p.X+12,(int)p.Y-34,45,42),Color.White);
        }
        if(house is not null){var h=Iso(11,15,origin);batch!.Draw(house,new Rectangle((int)h.X-55,(int)h.Y-118,190,153),Color.White);}
        foreach(var z in zombies.Where(z=>z.Alive).OrderBy(z=>z.Pos.X+z.Pos.Y))
        {
            var p=Iso(z.Pos.X,z.Pos.Y,origin)+new Vector2(32,10); if(zombie is not null)batch!.Draw(zombie,new Rectangle((int)p.X-28,(int)p.Y-76,55,77),Color.White);
            Fill(p.X-22,p.Y-84,44,4,new Color(45,15,12));Fill(p.X-22,p.Y-84,44*z.Health/100f,4,new Color(160,25,20));
        }
        var hp=Iso(player.X,player.Y,origin)+new Vector2(32,10);if(hero is not null)batch!.Draw(hero,new Rectangle((int)hp.X-28,(int)hp.Y-78,56,78),Color.White);
    }

    private void DrawHud()
    {
        var w=GraphicsDevice.Viewport.Width;var y=GraphicsDevice.Viewport.Height-HudH;
        Fill(0,y,w,HudH,new Color(24,18,14));Fill(0,y,w,4,new Color(130,91,42));
        Orb(95,y+72,47,new Color(155,15,20),health/100f);Orb(w-95,y+72,47,new Color(20,50,160),1);
        Fill(w/2-250,y+22,500,92,new Color(55,39,25));for(var i=0;i<8;i++){Fill(w/2-225+i*57,y+42,44,44,new Color(22,18,15));}
    }

    private void DrawMenu()
    {
        var w=GraphicsDevice.Viewport.Width;var h=GraphicsDevice.Viewport.Height;Fill(0,0,w,h,new Color(0,0,0,185));
        Fill(w/2-220,h/2-155,440,330,new Color(42,29,20));
        for(var i=0;i<4;i++){var y=h/2-80+i*56;Fill(w/2-180,y,360,44,i==menuIndex?new Color(125,78,34):new Color(27,22,19));DrawWord(Menu[i]+(i==1?(sound?" AN":" AUS"):""),w/2-145,y+13,3);}
    }

    private void DrawWord(string text,float x,float y,int s){foreach(var c in text){if(c!=' ')Fill(x,y,4*s,6*s,new Color(225,192,125));x+=6*s;}}
    private void Orb(int cx,int cy,int r,Color c,float amount){for(var y=-r;y<=r;y++)for(var x=-r;x<=r;x++)if(x*x+y*y<=r*r)Fill(cx+x,cy+y,1,1,(y+r)/(2f*r)<1-amount?new Color(20,18,16):c);}
    private void Fill(float x,float y,float w,float h,Color c)=>batch!.Draw(pixel!,new Rectangle((int)x,(int)y,Math.Max(1,(int)w),Math.Max(1,(int)h)),c);
    private Texture2D MakeDiamond(){var t=new Texture2D(GraphicsDevice,TileW,TileH);var d=new Color[TileW*TileH];for(var y=0;y<TileH;y++)for(var x=0;x<TileW;x++)d[y*TileW+x]=Math.Abs(x-TileW/2f)/(TileW/2f)+Math.Abs(y-TileH/2f)/(TileH/2f)<=1?Color.White:Color.Transparent;t.SetData(d);return t;}
    private Vector2 DesiredCamera(){var focus=new Vector2(GraphicsDevice.Viewport.Width/2f,(GraphicsDevice.Viewport.Height-HudH)/2f);return focus-(Iso(player.X,player.Y,BaseOrigin)+new Vector2(32,16));}
    private Point? ScreenToTile(Point s){var o=BaseOrigin+camera;var sx=s.X-o.X-TileW/2f;var sy=s.Y-o.Y-TileH/2f;var x=(int)MathF.Floor(sy/TileH+sx/TileW+.5f);var y=(int)MathF.Floor(sy/TileH-sx/TileW+.5f);return x>=0&&y>=0&&x<MapW&&y<MapH?new Point(x,y):null;}
    private bool Pressed(KeyboardState k,Keys key)=>k.IsKeyDown(key)&&!oldKeys.IsKeyDown(key);
    private static Vector2 Iso(float x,float y,Vector2 o)=>o+new Vector2((x-y)*TileW/2f,(x+y)*TileH/2f);
    private Vector2 BaseOrigin=>new(GraphicsDevice.Viewport.Width/2f-TileW/2f,30);
    private sealed class Zombie{public Vector2 Pos,Spawn;public float Health=100,Respawn;public bool Alive=true;public Zombie(Vector2 p){Pos=Spawn=p;}}
}
