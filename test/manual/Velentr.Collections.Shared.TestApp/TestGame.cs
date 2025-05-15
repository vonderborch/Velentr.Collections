using System.Diagnostics;
using System.Net.Mime;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Velentr.INDIVIDUAL_SUPPORT.TestApp;

public class TestGame : PerformanceMonitoredGame
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public TestGame() : base(
        "Velentr.Collections",
        FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion,
#if FNA
        "FNA",
#elif MONOGAME
        "Monogame",
#else
        framework: "Unknown",
#endif
        "font",
        Color.Black,
        new Vector2(5, 5)
    )
    {
        this._graphics = new GraphicsDeviceManager(this);
        this.Content.RootDirectory = "Content";
        this.IsMouseVisible = true;
    }

    private void UpdateResolution(int width, int height, bool fullscreen, GraphicsDeviceManager graphics)
    {
        graphics.IsFullScreen = fullscreen;
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        UpdateResolution(1280, 768, false, this._graphics);

        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        this._spriteBatch = new SpriteBatch(this.GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        this.GraphicsDevice.Clear(Color.CornflowerBlue);
        this._spriteBatch.Begin();
        RenderMetrics(gameTime, this._spriteBatch);

        // TODO: Add your drawing code here

        this._spriteBatch.End();
        base.Draw(gameTime);
    }
}
