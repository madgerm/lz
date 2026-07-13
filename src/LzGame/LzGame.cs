using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LzGame;

public sealed class LzGame : Game
{
    private const int TileWidth = 64;
    private const int TileHeight = 32;
    private const int MapWidth = 60;
    private const int MapHeight = 60;
    private const int HudHeight = 142;

    private readonly GraphicsDeviceManager graphics;
    private readonly TileKind[,] map = new TileKind[MapWidth, MapHeight];
    private readonly List<Enemy> enemies = new();

    private SpriteBatch? spriteBatch;
    private Texture2D? pixel;
    private Texture2D? diamond;
    private Texture2D? diamondHover;
    private SoundEffect? clickSound;
    private SoundEffect? hitSound;

    private MouseState previousMouse;
    private KeyboardState previousKeyboard;
    private Vector2 playerTile = new(7, 7);
    private Vector2 targetTile = new(7, 7);
    private Vector2 cameraOffset;
    private Point? hoverTile;
    private bool menuOpen;
    private bool soundEnabled = true;
    private int menuIndex;
    private float playerHealth = 100f;
    private float damageCooldown;

    private static readonly string[] MenuItems = ["WEITER", "SOUND", "VOLLBILD", "BEENDEN"];

    private static readonly Dictionary<char, string[]> Font = new()
    {
        ['A'] = ["01110","10001","10001","11111","10001","10001","10001"],
        ['B'] = ["11110","10001","10001","11110","10001","10001","11110"],
        ['D'] = ["11110","10001","10001","10001","10001","10001","11110"],
        ['E'] = ["11111","10000","10000","11110","10000","10000","11111"],
        ['F'] = ["11111","10000","10000","11110","10000","10000","10000"],
        ['G'] = ["01110","10001","10000","10111","10001","10001","01110"],
        ['H'] = ["10001","10001","10001","11111","10001","10001","10001"],
        ['I'] = ["11111","00100","00100","00100","00100","00100","11111"],
        ['L'] = ["10000","10000","10000","10000","10000","10000","11111"],
        ['M'] = ["10001","11011","10101","10101","10001","10001","10001"],
        ['N'] = ["10001","11001","10101","10011","10001","10001","10001"],
        ['O'] = ["01110","10001","10001","10001","10001","10001","01110"],
        ['P'] = ["11110","10001","10001","11110","10000","10000","10000"],
        ['R'] = ["11110","10001","10001","11110","10100","10010","10001"],
        ['S'] = ["01111","10000","10000","01110","00001","00001","11110"],
        ['T'] = ["11111","00100","00100","00100","00100","00100","00100"],
        ['U'] = ["10001","10001","10001","10001","10001","10001","01110"],
        ['V'] = ["10001","10001","10001","10001","10001","01010","00100"],
        ['W'] = ["10001","10001","10001","10101","10101","11011","10001"],
        ['Z'] = ["11111","00001","00010","00100","01000","10000","11111"],
        ['0'] = ["01110","10001","10011","10101","11001","10001","01110"],
        ['1'] = ["00100","01100","00100","00100","00100","00100","01110"],
        ['2'] = ["01110","10001","00001","00010","00100","01000","11111"],
        ['3'] = ["11110","00001","00001","01110","00001","00001","11110"],
        ['4'] = ["00010","00110","01010","10010","11111","00010","00010"],
        ['5'] = ["11111","10000","10000","11110","00001","00001","11110"],
        ['6'] = ["01110","10000","10000","11110","10001","10001","01110"],
        ['7'] = ["11111","00001","00010","00100","01000","01000","01000"],
        ['8'] = ["01110","10001","10001","01110","10001","10001","01110"],
        ['9'] = ["01110","10001","10001","01111","00001","00001","01110"],
        [':'] = ["00000","00100","00100","00000","00100","00100","00000"],
        ['/'] = ["00001","00010","00010","00100","01000","01000","10000"],
        [' '] = ["00000","00000","00000","00000","00000","00000","00000"]
    };

    public LzGame()
    {
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            SynchronizeWithVerticalRetrace = true
        };

        IsMouseVisible = true;
        Window.Title = "LZ - Isometric Linux Prototype";
    }

    protected override void Initialize()
    {
        GenerateMap();
        enemies.AddRange([
            new Enemy(new Vector2(15, 14)),
            new Enemy(new Vector2(23, 25)),
            new Enemy(new Vector2(37, 39)),
            new Enemy(new Vector2(47, 20)),
            new Enemy(new Vector2(31, 52))
        ]);
        CenterCameraImmediately();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        pixel = CreatePixel(GraphicsDevice);
        diamond = CreateDiamond(GraphicsDevice, TileWidth, TileHeight, new Color(255, 255, 255, 235), new Color(20, 28, 18, 80));
        diamondHover = CreateDiamond(GraphicsDevice, TileWidth, TileHeight, new Color(255, 230, 120, 90), new Color(255, 220, 90, 160));
        clickSound = CreateTone(520f, 0.07f, 0.18f);
        hitSound = CreateTone(105f, 0.12f, 0.32f);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Pressed(keyboard, Keys.Escape))
        {
            menuOpen = !menuOpen;
            Play(clickSound);
        }

        if (menuOpen)
        {
            UpdateMenu(keyboard, mouse);
        }
        else
        {
            hoverTile = ScreenToTile(mouse.Position);
            HandleWorldClick(mouse);
            MovePlayer(dt);
            UpdateEnemies(dt);
            UpdateCamera(dt);
        }

        damageCooldown = Math.Max(0f, damageCooldown - dt);
        previousMouse = mouse;
        previousKeyboard = keyboard;
        base.Update(gameTime);
    }

    private void UpdateMenu(KeyboardState keyboard, MouseState mouse)
    {
        if (Pressed(keyboard, Keys.Up) || Pressed(keyboard, Keys.W))
        {
            menuIndex = (menuIndex + MenuItems.Length - 1) % MenuItems.Length;
            Play(clickSound);
        }
        if (Pressed(keyboard, Keys.Down) || Pressed(keyboard, Keys.S))
        {
            menuIndex = (menuIndex + 1) % MenuItems.Length;
            Play(clickSound);
        }
        if (Pressed(keyboard, Keys.Enter) || Pressed(keyboard, Keys.Space))
        {
            ActivateMenuItem();
        }

        if (mouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
        {
            var w = GraphicsDevice.Viewport.Width;
            var h = GraphicsDevice.Viewport.Height;
            for (var i = 0; i < MenuItems.Length; i++)
            {
                var rect = new Rectangle(w / 2 - 190, h / 2 - 80 + i * 58, 380, 46);
                if (rect.Contains(mouse.Position))
                {
                    menuIndex = i;
                    ActivateMenuItem();
                    break;
                }
            }
        }
    }

    private void ActivateMenuItem()
    {
        Play(clickSound);
        switch (menuIndex)
        {
            case 0:
                menuOpen = false;
                break;
            case 1:
                soundEnabled = !soundEnabled;
                break;
            case 2:
                graphics.ToggleFullScreen();
                break;
            case 3:
                Exit();
                break;
        }
    }

    private void HandleWorldClick(MouseState mouse)
    {
        if (mouse.LeftButton != ButtonState.Pressed || previousMouse.LeftButton != ButtonState.Released ||
            mouse.Y >= GraphicsDevice.Viewport.Height - HudHeight)
        {
            return;
        }

        var clicked = ScreenToTile(mouse.Position);
        if (!clicked.HasValue) return;

        var clickTile = new Vector2(clicked.Value.X, clicked.Value.Y);
        var enemy = enemies.Where(e => e.Alive).OrderBy(e => Vector2.DistanceSquared(e.Position, clickTile)).FirstOrDefault();
        if (enemy is not null && Vector2.Distance(enemy.Position, clickTile) < 1.1f && Vector2.Distance(playerTile, enemy.Position) < 2.2f)
        {
            enemy.Health -= 35f;
            Play(hitSound);
            if (enemy.Health <= 0f)
            {
                enemy.Alive = false;
                enemy.RespawnTimer = 7f;
            }
            return;
        }

        targetTile = clickTile;
        Play(clickSound);
    }

    private void UpdateEnemies(float dt)
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.Alive)
            {
                enemy.RespawnTimer -= dt;
                if (enemy.RespawnTimer <= 0f)
                {
                    enemy.Alive = true;
                    enemy.Health = 100f;
                    enemy.Position = enemy.Spawn;
                }
                continue;
            }

            var distance = Vector2.Distance(enemy.Position, playerTile);
            if (distance < 8f && distance > 0.75f)
            {
                var direction = Vector2.Normalize(playerTile - enemy.Position);
                enemy.Position += direction * 1.45f * dt;
            }
            else if (distance >= 8f)
            {
                enemy.WanderTimer -= dt;
                if (enemy.WanderTimer <= 0f)
                {
                    enemy.WanderTimer = 1.5f + (enemy.Spawn.X % 3f);
                    var phase = enemy.WanderTimer + enemy.Position.X * 0.4f + enemy.Position.Y;
                    enemy.WanderDirection = Vector2.Normalize(new Vector2(MathF.Cos(phase), MathF.Sin(phase)));
                }
                enemy.Position += enemy.WanderDirection * 0.45f * dt;
                enemy.Position = Vector2.Clamp(enemy.Position, enemy.Spawn - new Vector2(3f), enemy.Spawn + new Vector2(3f));
            }

            if (distance < 0.85f && damageCooldown <= 0f)
            {
                playerHealth = Math.Max(0f, playerHealth - 12f);
                damageCooldown = 0.8f;
                Play(hitSound);
                if (playerHealth <= 0f)
                {
                    playerHealth = 100f;
                    playerTile = new Vector2(7, 7);
                    targetTile = playerTile;
                    CenterCameraImmediately();
                }
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(12, 18, 10));
        if (spriteBatch is null || pixel is null || diamond is null || diamondHover is null) return;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        DrawWorld(spriteBatch, pixel, diamond, diamondHover);
        DrawHud(spriteBatch, pixel);
        if (menuOpen) DrawMenu(spriteBatch, pixel);
        spriteBatch.End();
        base.Draw(gameTime);
    }

    private void GenerateMap()
    {
        for (var y = 0; y < MapHeight; y++)
        for (var x = 0; x < MapWidth; x++)
        {
            var edge = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1;
            var path = Math.Abs(x - y) <= 1 || y == 20 || x == 10 || y == 42 || x == 38;
            var clearing = (x is > 5 and < 14 && y is > 11 and < 19) || (x is > 31 and < 43 && y is > 35 and < 46);
            var forest = x + y > 78 || x < 4 || y < 4 || ((x * 13 + y * 7) % 29 < 4 && !path && !clearing);
            map[x, y] = edge ? TileKind.Fence : forest ? TileKind.Forest : clearing ? TileKind.Sand : path ? TileKind.Path : TileKind.Grass;
        }

        for (var y = 13; y < 18; y++)
        for (var x = 7; x < 12; x++) map[x, y] = TileKind.Floor;
    }

    private void MovePlayer(float dt)
    {
        var delta = targetTile - playerTile;
        if (delta.LengthSquared() < 0.001f) { playerTile = targetTile; return; }
        var step = Vector2.Normalize(delta) * 3.8f * dt;
        playerTile = step.LengthSquared() >= delta.LengthSquared() ? targetTile : playerTile + step;
    }

    private void CenterCameraImmediately() => cameraOffset = GetDesiredCameraOffset();

    private void UpdateCamera(float dt)
    {
        var smoothing = 1f - MathF.Exp(-7f * dt);
        cameraOffset = Vector2.Lerp(cameraOffset, GetDesiredCameraOffset(), smoothing);
    }

    private Vector2 GetDesiredCameraOffset()
    {
        var visibleWorldHeight = Math.Max(1, GraphicsDevice.Viewport.Height - HudHeight);
        var focus = new Vector2(GraphicsDevice.Viewport.Width / 2f, visibleWorldHeight / 2f + 24f);
        var playerScreen = TileToScreen(playerTile.X, playerTile.Y, BaseWorldOrigin) + new Vector2(TileWidth / 2f, TileHeight / 2f);
        return focus - playerScreen;
    }

    private void DrawWorld(SpriteBatch batch, Texture2D one, Texture2D tile, Texture2D hover)
    {
        var origin = WorldOrigin;
        for (var sum = 0; sum <= MapWidth + MapHeight - 2; sum++)
        for (var x = 0; x < MapWidth; x++)
        {
            var y = sum - x;
            if (y < 0 || y >= MapHeight) continue;
            var pos = TileToScreen(x, y, origin);
            if (pos.X < -180 || pos.X > GraphicsDevice.Viewport.Width + 180 || pos.Y < -180 || pos.Y > GraphicsDevice.Viewport.Height - HudHeight + 180) continue;

            DrawTile(batch, one, tile, pos, map[x, y]);
            if ((x + y) % 11 == 0 && map[x, y] == TileKind.Grass) DrawShrub(batch, one, pos + new Vector2(18, 8));
            if ((x * 7 + y * 3) % 17 == 0 && map[x, y] == TileKind.Forest) DrawTree(batch, one, pos + new Vector2(32, -16));
            if ((x + y) % 9 == 0 && map[x, y] == TileKind.Fence)
            {
                Fill(batch, one, pos.X + 18, pos.Y + 8, 28, 10, new Color(91, 55, 30));
                Fill(batch, one, pos.X + 20, pos.Y + 3, 5, 18, new Color(66, 37, 22));
                Fill(batch, one, pos.X + 40, pos.Y + 3, 5, 18, new Color(66, 37, 22));
            }
        }

        if (hoverTile.HasValue) batch.Draw(hover, TileToScreen(hoverTile.Value.X, hoverTile.Value.Y, origin), Color.White);
        DrawHouse(batch, one, TileToScreen(8, 13, origin));

        foreach (var enemy in enemies.Where(e => e.Alive).OrderBy(e => e.Position.X + e.Position.Y))
            DrawEnemy(batch, one, TileToScreen(enemy.Position.X, enemy.Position.Y, origin) + new Vector2(32, 10), enemy.Health);

        DrawActor(batch, one, TileToScreen(playerTile.X, playerTile.Y, origin) + new Vector2(32, 10));
    }

    private static void DrawTile(SpriteBatch batch, Texture2D one, Texture2D tile, Vector2 position, TileKind kind)
    {
        var color = kind switch
        {
            TileKind.Path => new Color(107, 82, 55), TileKind.Sand => new Color(197, 179, 132),
            TileKind.Floor => new Color(120, 102, 86), TileKind.Forest => new Color(28, 70, 32),
            TileKind.Fence => new Color(70, 60, 34), _ => new Color(54, 104, 45)
        };
        batch.Draw(tile, position, color);
        if (kind is TileKind.Grass or TileKind.Forest)
        {
            Fill(batch, one, position.X + 24, position.Y + 12, 3, 6, new Color(34, 145, 44));
            Fill(batch, one, position.X + 36, position.Y + 9, 3, 8, new Color(46, 160, 53));
        }
    }

    private static void DrawHouse(SpriteBatch b, Texture2D p, Vector2 v)
    {
        Fill(b,p,v.X+10,v.Y-58,118,62,new Color(90,85,80)); Fill(b,p,v.X+18,v.Y-82,92,26,new Color(45,45,46));
        Fill(b,p,v.X+34,v.Y-54,22,48,new Color(112,70,34)); Fill(b,p,v.X+65,v.Y-45,35,22,new Color(26,30,29));
        Fill(b,p,v.X+125,v.Y-42,34,56,new Color(96,58,33));
    }

    private static void DrawTree(SpriteBatch b, Texture2D p, Vector2 v)
    {
        Fill(b,p,v.X-3,v.Y+9,6,28,new Color(66,43,24)); Fill(b,p,v.X-18,v.Y-18,36,28,new Color(13,65,29));
        Fill(b,p,v.X-13,v.Y-35,26,24,new Color(15,75,32)); Fill(b,p,v.X-23,v.Y-2,46,22,new Color(10,51,24));
    }

    private static void DrawShrub(SpriteBatch b, Texture2D p, Vector2 v)
    {
        Fill(b,p,v.X,v.Y,16,7,new Color(21,132,43)); Fill(b,p,v.X+5,v.Y-5,12,7,new Color(38,155,49));
    }

    private static void DrawActor(SpriteBatch b, Texture2D p, Vector2 v)
    {
        Fill(b,p,v.X-8,v.Y+14,16,6,new Color(12,11,10,120)); Fill(b,p,v.X-4,v.Y-16,8,22,new Color(32,39,58));
        Fill(b,p,v.X-5,v.Y-25,10,9,new Color(171,136,104)); Fill(b,p,v.X-9,v.Y-7,4,14,new Color(22,28,44));
        Fill(b,p,v.X+5,v.Y-7,4,14,new Color(22,28,44)); Fill(b,p,v.X-6,v.Y+5,5,12,new Color(24,24,28));
        Fill(b,p,v.X+1,v.Y+5,5,12,new Color(24,24,28));
    }

    private static void DrawEnemy(SpriteBatch b, Texture2D p, Vector2 v, float health)
    {
        Fill(b,p,v.X-9,v.Y+14,18,6,new Color(8,8,8,120)); Fill(b,p,v.X-5,v.Y-18,10,23,new Color(54,80,45));
        Fill(b,p,v.X-6,v.Y-27,12,10,new Color(103,137,79)); Fill(b,p,v.X-11,v.Y-8,5,16,new Color(69,104,57));
        Fill(b,p,v.X+6,v.Y-8,5,16,new Color(69,104,57)); Fill(b,p,v.X-7,v.Y+4,6,13,new Color(43,52,39));
        Fill(b,p,v.X+1,v.Y+4,6,13,new Color(43,52,39));
        Fill(b,p,v.X-16,v.Y-36,32,4,new Color(35,20,17)); Fill(b,p,v.X-16,v.Y-36,32*MathHelper.Clamp(health/100f,0f,1f),4,new Color(140,28,24));
    }

    private void DrawHud(SpriteBatch batch, Texture2D one)
    {
        var h = GraphicsDevice.Viewport.Height; var w = GraphicsDevice.Viewport.Width; var y = h - HudHeight;
        Fill(batch,one,0,y,w,HudHeight,new Color(31,23,18)); Fill(batch,one,0,y,w,4,new Color(118,88,45));
        Fill(batch,one,w/2-260,y+16,520,106,new Color(49,39,34)); Fill(batch,one,w/2-248,y+28,496,82,new Color(21,18,17));
        DrawOrb(batch,one,96,y+72,46,new Color(122,18,24),new Color(225,40,35),playerHealth/100f);
        DrawOrb(batch,one,w-96,y+72,46,new Color(22,43,117),new Color(50,98,220),1f);
        for (var i=0;i<8;i++){var x=w/2-228+i*58;Fill(batch,one,x,y+42,46,46,new Color(73,58,44));Fill(batch,one,x+4,y+46,38,38,new Color(30,25,22));}
        DrawText(batch,one,$"LEBEN {(int)playerHealth}/100",w/2-104,y+104,2,new Color(213,190,140));
    }

    private static void DrawOrb(SpriteBatch b, Texture2D p, int cx, int cy, int radius, Color dark, Color light, float fill)
    {
        for(var yy=-radius;yy<=radius;yy++) for(var xx=-radius;xx<=radius;xx++)
        {
            var d=xx*xx+yy*yy; if(d>radius*radius)continue;
            var normalized=(yy+radius)/(radius*2f); var empty=normalized < 1f-MathHelper.Clamp(fill,0f,1f);
            var shade=1f-MathHelper.Clamp(MathF.Sqrt(d)/radius,0f,1f);
            Fill(b,p,cx+xx,cy+yy,1,1,empty?new Color(25,22,20):Color.Lerp(dark,light,shade*.9f));
        }
    }

    private void DrawMenu(SpriteBatch b, Texture2D p)
    {
        var w=GraphicsDevice.Viewport.Width; var h=GraphicsDevice.Viewport.Height;
        Fill(b,p,0,0,w,h,new Color(0,0,0,175)); Fill(b,p,w/2-235,h/2-175,470,360,new Color(29,22,18));
        Fill(b,p,w/2-225,h/2-165,450,340,new Color(53,40,30)); DrawText(b,p,"PAUSE",w/2-75,h/2-145,5,new Color(220,186,112));
        for(var i=0;i<MenuItems.Length;i++)
        {
            var y=h/2-80+i*58; var selected=i==menuIndex;
            Fill(b,p,w/2-190,y,380,46,selected?new Color(112,75,38):new Color(35,29,25));
            var label=MenuItems[i];
            if(i==1)label+=" "+(soundEnabled?"AN":"AUS");
            if(i==2)label+=" "+(graphics.IsFullScreen?"AN":"AUS");
            DrawText(b,p,label,w/2-160,y+11,3,selected?new Color(255,222,150):new Color(190,170,135));
        }
    }

    private static void DrawText(SpriteBatch b, Texture2D p, string text, float x, float y, int scale, Color color)
    {
        var cursor=x;
        foreach(var c in text.ToUpperInvariant())
        {
            if(!Font.TryGetValue(c,out var glyph)){cursor+=6*scale;continue;}
            for(var gy=0;gy<7;gy++) for(var gx=0;gx<5;gx++) if(glyph[gy][gx]=='1') Fill(b,p,cursor+gx*scale,y+gy*scale,scale,scale,color);
            cursor+=6*scale;
        }
    }

    private Point? ScreenToTile(Point screen)
    {
        var sx=screen.X-WorldOrigin.X-TileWidth/2f; var sy=screen.Y-WorldOrigin.Y-TileHeight/2f;
        var ix=(int)MathF.Floor(sy/TileHeight+sx/TileWidth+.5f); var iy=(int)MathF.Floor(sy/TileHeight-sx/TileWidth+.5f);
        return ix>=0&&iy>=0&&ix<MapWidth&&iy<MapHeight?new Point(ix,iy):null;
    }

    private bool Pressed(KeyboardState state, Keys key) => state.IsKeyDown(key) && !previousKeyboard.IsKeyDown(key);
    private void Play(SoundEffect? sound){if(soundEnabled)sound?.Play();}

    private SoundEffect CreateTone(float frequency, float seconds, float volume)
    {
        const int sampleRate=22050; var count=(int)(sampleRate*seconds); var data=new byte[count*2];
        for(var i=0;i<count;i++)
        {
            var fade=1f-i/(float)count; var sample=(short)(MathF.Sin(2f*MathF.PI*frequency*i/sampleRate)*short.MaxValue*volume*fade);
            data[i*2]=(byte)(sample&255); data[i*2+1]=(byte)((sample>>8)&255);
        }
        return new SoundEffect(data,sampleRate,AudioChannels.Mono);
    }

    private static Vector2 TileToScreen(float x,float y,Vector2 origin)=>origin+new Vector2((x-y)*TileWidth/2f,(x+y)*TileHeight/2f);
    private Vector2 BaseWorldOrigin=>new(GraphicsDevice.Viewport.Width/2f-TileWidth/2f,38);
    private Vector2 WorldOrigin=>BaseWorldOrigin+cameraOffset;
    private static void Fill(SpriteBatch b,Texture2D t,float x,float y,float w,float h,Color c)=>b.Draw(t,new Rectangle((int)x,(int)y,Math.Max(1,(int)w),Math.Max(1,(int)h)),c);

    private static Texture2D CreatePixel(GraphicsDevice d){var t=new Texture2D(d,1,1);t.SetData([Color.White]);return t;}
    private static Texture2D CreateDiamond(GraphicsDevice d,int width,int height,Color fill,Color border)
    {
        var data=new Color[width*height];var cx=width/2f;var cy=height/2f;
        for(var y=0;y<height;y++)for(var x=0;x<width;x++){var dist=MathF.Abs(x-cx)/cx+MathF.Abs(y-cy)/cy;data[y*width+x]=dist<=1f?(dist>.88f?border:fill):Color.Transparent;}
        var t=new Texture2D(d,width,height);t.SetData(data);return t;
    }

    private sealed class Enemy(Vector2 spawn)
    {
        public Vector2 Spawn { get; } = spawn;
        public Vector2 Position { get; set; } = spawn;
        public Vector2 WanderDirection { get; set; } = Vector2.UnitX;
        public float WanderTimer { get; set; }
        public float Health { get; set; } = 100f;
        public float RespawnTimer { get; set; }
        public bool Alive { get; set; } = true;
    }

    private enum TileKind { Grass, Forest, Path, Sand, Floor, Fence }
}
