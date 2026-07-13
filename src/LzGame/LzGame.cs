using Microsoft.Xna.Framework;
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
    private SpriteBatch? spriteBatch;
    private Texture2D? pixel;
    private Texture2D? diamond;
    private Texture2D? diamondHover;
    private MouseState previousMouse;

    private readonly TileKind[,] map = new TileKind[MapWidth, MapHeight];
    private Vector2 playerTile = new(7, 7);
    private Vector2 targetTile = new(7, 7);
    private Vector2 cameraOffset;
    private Point? hoverTile;

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
        CenterCameraImmediately();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        pixel = CreatePixel(GraphicsDevice);
        diamond = CreateDiamond(GraphicsDevice, TileWidth, TileHeight, new Color(255, 255, 255, 235), new Color(20, 28, 18, 80));
        diamondHover = CreateDiamond(GraphicsDevice, TileWidth, TileHeight, new Color(255, 230, 120, 90), new Color(255, 220, 90, 160));
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
            return;
        }

        var mouse = Mouse.GetState();
        hoverTile = ScreenToTile(mouse.Position);

        if (mouse.LeftButton == ButtonState.Pressed &&
            previousMouse.LeftButton == ButtonState.Released &&
            mouse.Y < GraphicsDevice.Viewport.Height - HudHeight)
        {
            var clicked = ScreenToTile(mouse.Position);
            if (clicked.HasValue)
            {
                targetTile = new Vector2(clicked.Value.X, clicked.Value.Y);
            }
        }

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        MovePlayer(dt);
        UpdateCamera(dt);

        previousMouse = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(12, 18, 10));

        if (spriteBatch is null || pixel is null || diamond is null || diamondHover is null)
        {
            return;
        }

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
        DrawWorld(spriteBatch, pixel, diamond, diamondHover);
        DrawHud(spriteBatch, pixel);
        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void GenerateMap()
    {
        for (var y = 0; y < MapHeight; y++)
        {
            for (var x = 0; x < MapWidth; x++)
            {
                var edge = x == 0 || y == 0 || x == MapWidth - 1 || y == MapHeight - 1;
                var path = Math.Abs(x - y) <= 1 || y == 20 || x == 10 || y == 42 || x == 38;
                var clearing = (x is > 5 and < 14 && y is > 11 and < 19) ||
                               (x is > 31 and < 43 && y is > 35 and < 46);
                var forest = x + y > 78 || x < 4 || y < 4 ||
                             ((x * 13 + y * 7) % 29 < 4 && !path && !clearing);

                map[x, y] = edge ? TileKind.Fence : forest ? TileKind.Forest : clearing ? TileKind.Sand : path ? TileKind.Path : TileKind.Grass;
            }
        }

        for (var y = 13; y < 18; y++)
        {
            for (var x = 7; x < 12; x++)
            {
                map[x, y] = TileKind.Floor;
            }
        }
    }

    private void MovePlayer(float dt)
    {
        var delta = targetTile - playerTile;
        if (delta.LengthSquared() < 0.001f)
        {
            playerTile = targetTile;
            return;
        }

        const float speed = 3.8f;
        var step = Vector2.Normalize(delta) * speed * dt;
        playerTile = step.LengthSquared() >= delta.LengthSquared() ? targetTile : playerTile + step;
    }

    private void CenterCameraImmediately()
    {
        cameraOffset = GetDesiredCameraOffset();
    }

    private void UpdateCamera(float dt)
    {
        var desired = GetDesiredCameraOffset();
        var smoothing = 1f - MathF.Exp(-7f * dt);
        cameraOffset = Vector2.Lerp(cameraOffset, desired, smoothing);
    }

    private Vector2 GetDesiredCameraOffset()
    {
        var visibleWorldHeight = Math.Max(1, GraphicsDevice.Viewport.Height - HudHeight);
        var focusPoint = new Vector2(GraphicsDevice.Viewport.Width / 2f, visibleWorldHeight / 2f + 24f);
        var playerScreenWithoutCamera = TileToScreen(playerTile.X, playerTile.Y, BaseWorldOrigin) + new Vector2(TileWidth / 2f, TileHeight / 2f);
        return focusPoint - playerScreenWithoutCamera;
    }

    private void DrawWorld(SpriteBatch batch, Texture2D one, Texture2D tile, Texture2D hover)
    {
        var origin = WorldOrigin;

        for (var sum = 0; sum <= MapWidth + MapHeight - 2; sum++)
        {
            for (var x = 0; x < MapWidth; x++)
            {
                var y = sum - x;
                if (y < 0 || y >= MapHeight) continue;

                var pos = TileToScreen(x, y, origin);
                if (pos.X < -180 || pos.X > GraphicsDevice.Viewport.Width + 180 ||
                    pos.Y < -180 || pos.Y > GraphicsDevice.Viewport.Height - HudHeight + 180)
                {
                    continue;
                }

                DrawTile(batch, one, tile, pos, map[x, y]);

                if ((x + y) % 11 == 0 && map[x, y] == TileKind.Grass)
                {
                    DrawShrub(batch, one, pos + new Vector2(18, 8));
                }

                if ((x * 7 + y * 3) % 17 == 0 && map[x, y] == TileKind.Forest)
                {
                    DrawTree(batch, one, pos + new Vector2(32, -16));
                }

                if ((x + y) % 9 == 0 && map[x, y] == TileKind.Fence)
                {
                    Fill(batch, one, pos.X + 18, pos.Y + 8, 28, 10, new Color(91, 55, 30));
                    Fill(batch, one, pos.X + 20, pos.Y + 3, 5, 18, new Color(66, 37, 22));
                    Fill(batch, one, pos.X + 40, pos.Y + 3, 5, 18, new Color(66, 37, 22));
                }
            }
        }

        if (hoverTile.HasValue)
        {
            var hoverPos = TileToScreen(hoverTile.Value.X, hoverTile.Value.Y, origin);
            batch.Draw(hover, hoverPos, Color.White);
        }

        DrawHouse(batch, one, TileToScreen(8, 13, origin));
        DrawActor(batch, one, TileToScreen(playerTile.X, playerTile.Y, origin) + new Vector2(32, 10));
    }

    private void DrawTile(SpriteBatch batch, Texture2D one, Texture2D tile, Vector2 position, TileKind kind)
    {
        var color = kind switch
        {
            TileKind.Path => new Color(107, 82, 55),
            TileKind.Sand => new Color(197, 179, 132),
            TileKind.Floor => new Color(120, 102, 86),
            TileKind.Forest => new Color(28, 70, 32),
            TileKind.Fence => new Color(70, 60, 34),
            _ => new Color(54, 104, 45)
        };

        batch.Draw(tile, position, color);

        if (kind == TileKind.Grass || kind == TileKind.Forest)
        {
            Fill(batch, one, position.X + 24, position.Y + 12, 3, 6, new Color(34, 145, 44));
            Fill(batch, one, position.X + 36, position.Y + 9, 3, 8, new Color(46, 160, 53));
            Fill(batch, one, position.X + 31, position.Y + 17, 2, 5, new Color(23, 124, 35));
        }
    }

    private void DrawHouse(SpriteBatch batch, Texture2D one, Vector2 p)
    {
        Fill(batch, one, p.X + 10, p.Y - 58, 118, 62, new Color(90, 85, 80));
        Fill(batch, one, p.X + 18, p.Y - 82, 92, 26, new Color(45, 45, 46));
        Fill(batch, one, p.X + 34, p.Y - 54, 22, 48, new Color(112, 70, 34));
        Fill(batch, one, p.X + 65, p.Y - 45, 35, 22, new Color(26, 30, 29));
        Fill(batch, one, p.X + 125, p.Y - 42, 34, 56, new Color(96, 58, 33));
        Fill(batch, one, p.X + 132, p.Y - 70, 20, 30, new Color(85, 49, 29));
    }

    private void DrawTree(SpriteBatch batch, Texture2D one, Vector2 p)
    {
        Fill(batch, one, p.X - 3, p.Y + 9, 6, 28, new Color(66, 43, 24));
        Fill(batch, one, p.X - 18, p.Y - 18, 36, 28, new Color(13, 65, 29));
        Fill(batch, one, p.X - 13, p.Y - 35, 26, 24, new Color(15, 75, 32));
        Fill(batch, one, p.X - 23, p.Y - 2, 46, 22, new Color(10, 51, 24));
    }

    private void DrawShrub(SpriteBatch batch, Texture2D one, Vector2 p)
    {
        Fill(batch, one, p.X, p.Y, 16, 7, new Color(21, 132, 43));
        Fill(batch, one, p.X + 5, p.Y - 5, 12, 7, new Color(38, 155, 49));
    }

    private void DrawActor(SpriteBatch batch, Texture2D one, Vector2 p)
    {
        Fill(batch, one, p.X - 8, p.Y + 14, 16, 6, new Color(12, 11, 10, 120));
        Fill(batch, one, p.X - 4, p.Y - 16, 8, 22, new Color(32, 39, 58));
        Fill(batch, one, p.X - 5, p.Y - 25, 10, 9, new Color(171, 136, 104));
        Fill(batch, one, p.X - 9, p.Y - 7, 4, 14, new Color(22, 28, 44));
        Fill(batch, one, p.X + 5, p.Y - 7, 4, 14, new Color(22, 28, 44));
        Fill(batch, one, p.X - 6, p.Y + 5, 5, 12, new Color(24, 24, 28));
        Fill(batch, one, p.X + 1, p.Y + 5, 5, 12, new Color(24, 24, 28));
    }

    private void DrawHud(SpriteBatch batch, Texture2D one)
    {
        var h = GraphicsDevice.Viewport.Height;
        var w = GraphicsDevice.Viewport.Width;
        var y = h - HudHeight;

        Fill(batch, one, 0, y, w, HudHeight, new Color(31, 23, 18));
        Fill(batch, one, 0, y, w, 4, new Color(118, 88, 45));
        Fill(batch, one, w / 2 - 260, y + 16, 520, 106, new Color(49, 39, 34));
        Fill(batch, one, w / 2 - 248, y + 28, 496, 82, new Color(21, 18, 17));

        DrawOrb(batch, one, 96, y + 72, 46, new Color(122, 18, 24), new Color(225, 40, 35));
        DrawOrb(batch, one, w - 96, y + 72, 46, new Color(22, 43, 117), new Color(50, 98, 220));

        for (var i = 0; i < 8; i++)
        {
            var x = w / 2 - 228 + i * 58;
            Fill(batch, one, x, y + 42, 46, 46, new Color(73, 58, 44));
            Fill(batch, one, x + 4, y + 46, 38, 38, new Color(30, 25, 22));
            Fill(batch, one, x + 17, y + 59, 12, 12, new Color(114, 87, 48));
        }

        Fill(batch, one, w / 2 - 85, y + 100, 170, 10, new Color(95, 70, 38));
    }

    private void DrawOrb(SpriteBatch batch, Texture2D one, int cx, int cy, int radius, Color dark, Color light)
    {
        for (var yy = -radius; yy <= radius; yy++)
        {
            for (var xx = -radius; xx <= radius; xx++)
            {
                var d = xx * xx + yy * yy;
                if (d > radius * radius) continue;
                var shade = 1f - MathHelper.Clamp(MathF.Sqrt(d) / radius, 0f, 1f);
                var color = Color.Lerp(dark, light, shade * 0.9f);
                Fill(batch, one, cx + xx, cy + yy, 1, 1, color);
            }
        }

        Fill(batch, one, cx - radius, cy - radius, radius * 2, 3, new Color(130, 100, 60));
        Fill(batch, one, cx - radius, cy + radius - 3, radius * 2, 3, new Color(20, 15, 12));
    }

    private Point? ScreenToTile(Point screen)
    {
        var origin = WorldOrigin;
        var sx = screen.X - origin.X - TileWidth / 2f;
        var sy = screen.Y - origin.Y - TileHeight / 2f;

        var tx = sy / TileHeight + sx / TileWidth;
        var ty = sy / TileHeight - sx / TileWidth;
        var ix = (int)MathF.Floor(tx + 0.5f);
        var iy = (int)MathF.Floor(ty + 0.5f);

        return ix >= 0 && iy >= 0 && ix < MapWidth && iy < MapHeight ? new Point(ix, iy) : null;
    }

    private static Vector2 TileToScreen(float x, float y, Vector2 origin)
    {
        return origin + new Vector2((x - y) * TileWidth / 2f, (x + y) * TileHeight / 2f);
    }

    private Vector2 BaseWorldOrigin => new(GraphicsDevice.Viewport.Width / 2f - TileWidth / 2f, 38);
    private Vector2 WorldOrigin => BaseWorldOrigin + cameraOffset;

    private static void Fill(SpriteBatch batch, Texture2D texture, float x, float y, float w, float h, Color color)
    {
        batch.Draw(texture, new Rectangle((int)x, (int)y, Math.Max(1, (int)w), Math.Max(1, (int)h)), color);
    }

    private static Texture2D CreatePixel(GraphicsDevice device)
    {
        var tex = new Texture2D(device, 1, 1);
        tex.SetData(new[] { Color.White });
        return tex;
    }

    private static Texture2D CreateDiamond(GraphicsDevice device, int width, int height, Color fill, Color border)
    {
        var data = new Color[width * height];
        var cx = width / 2f;
        var cy = height / 2f;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var nx = MathF.Abs(x - cx) / cx;
                var ny = MathF.Abs(y - cy) / cy;
                var d = nx + ny;
                data[y * width + x] = d <= 1f ? (d > 0.88f ? border : fill) : Color.Transparent;
            }
        }

        var tex = new Texture2D(device, width, height);
        tex.SetData(data);
        return tex;
    }

    private enum TileKind
    {
        Grass,
        Forest,
        Path,
        Sand,
        Floor,
        Fence
    }
}
