using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Rybie.Sprites;

namespace Rybie
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private int _screenWidth;
        private int _screenHeight;

        #region Fish

        private List<Fish> _fishes;
        private float _fishSpeedMultiplier;
        private Texture2D _fishTexture;
        private float _fishSpawnDelay;
        private double _fishTimer;
        private SpriteFont _fishFont;

        #endregion

        #region Player

        private float _shipSpeed;
        private SpriteBase _ship;
        private int _lives;
        private float _score;

        #endregion

        #region Net

        private Texture2D _netTexture;
        private float _netSpeed;
        private List<Net> _nets;

        #endregion

        #region UI

        private SpriteFont _uiFont;
        private SpriteFont _stateFont;
        private Texture2D _heart;
        private Texture2D _splashScreen;
        private bool _gameStarted;
        private bool _paused;
        private int _fishesToCatch;
        private const string ToCatch = "Do zlapania: ";
        private const string Chances = "Szanse: ";

        #endregion

        private KeyboardState _previousState;

        private Random _random;

        private int _level;
        private bool _levelTransition;
        private float _levelTransitionTimer;
        private float _levelTransitionDelay;

        private Texture2D _bubbleTexture;
        private List<BackgroundBubble> _bubbles;
        private int _bubbleAmount;

        public Game1()
        {
            //Minimal resolution of the game is 1024x576
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferHeight = _screenHeight = 576, PreferredBackBufferWidth = _screenWidth = 1024
            };

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnClientSizeChanged;

            Content.RootDirectory = "Content";
        }

        private void OnClientSizeChanged(object sender, EventArgs e)
        {

            if (GraphicsDevice.Viewport.Height < 576)
            {
                _graphics.PreferredBackBufferHeight = _screenHeight = 576;
                _graphics.ApplyChanges();
            }
            else
            {
                _screenHeight = GraphicsDevice.Viewport.Height;
            }

            if (GraphicsDevice.Viewport.Width < 1024)
            {
                _graphics.PreferredBackBufferWidth = _screenWidth = 1024;
                _graphics.ApplyChanges();
            }
            else
            {
                _screenWidth = GraphicsDevice.Viewport.Width;
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _screenHeight = GraphicsDevice.Viewport.Height;
            _screenWidth = GraphicsDevice.Viewport.Width;

            _shipSpeed = 500f;

            _netSpeed = 500f;
            _nets = new List<Net>();

            _fishSpeedMultiplier = 0.1f;
            _fishes = new List<Fish>();
            _fishSpawnDelay = 3f;
            _fishTimer = 0;

            _levelTransitionDelay = 1f;

            _bubbleAmount = 50;

            _previousState = Keyboard.GetState();

            _random = new Random();

            this.IsMouseVisible = false;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _fishTexture = Content.Load<Texture2D>("ryba");

            _ship = new SpriteBase(GraphicsDevice, @"Content/okrecik.png", 0.8f);

            _netTexture = Content.Load<Texture2D>("siata");
            _bubbleTexture = Content.Load<Texture2D>("Bubble");

            _uiFont = Content.Load<SpriteFont>("Lives");
            _stateFont = Content.Load<SpriteFont>("State");
            _fishFont = Content.Load<SpriteFont>("FishFont");

            _splashScreen = Content.Load<Texture2D>("start-splash");
            _heart = Content.Load<Texture2D>("serducho");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_levelTransition)
            {
                if (_levelTransitionTimer > _levelTransitionDelay)
                {
                    _levelTransition = false;
                    _levelTransitionTimer = 0f;
                }

                _levelTransitionTimer += elapsedTime;
            }

            KeyboardHandler();

            if (!_paused && !_levelTransition && _gameStarted)
            {

                if (_gameStarted && _fishTimer > _fishSpawnDelay / Math.Sqrt(_level))
                {
                    SpawnFish();
                    _fishTimer = 0;
                }

                _fishTimer += elapsedTime;

                foreach (var backgroundBubble in _bubbles)
                {
                    backgroundBubble.Update(elapsedTime);
                    if (backgroundBubble.X < 0)
                    {
                        backgroundBubble.X = _screenWidth;
                    }
                }

                UpdateFish(elapsedTime);

                foreach (var net in _nets.ToList())
                {
                    net.Update(elapsedTime);
                    if (net.X > _screenWidth)
                    {
                        _nets.Remove(net);
                    }

                    if (net.Dead)
                    {
                        _nets.Remove(net);
                    }
                }

                _ship.Update(elapsedTime);
            }

            base.Update(gameTime);
        }

        private void UpdateFish(float elapsedTime)
        {
            foreach (var fish in _fishes.ToList())
            {
                fish.Update(elapsedTime);
                if (fish.X < -10)
                {
                    _fishes.Remove(fish);
                    if (fish.IsCorrect) _lives -= 1;
                }

                CheckCollision(fish);

                if (fish.Dead)
                {
                    _fishes.Remove(fish);
                }
            }
        }

        private void CheckCollision(Fish fish)
        {
            if (_nets.Any(c => c.RectangleCollision(fish)))
            {
                if (fish.IsCorrect)
                {
                    _score += 100 + (int)(fish.X / 2);
                    _fishesToCatch--;
                    if (_fishesToCatch == 0)
                    {
                        LevelUp();
                    }
                }
                else
                {
                    _score -= 100;
                    _lives--;
                }
            }
        }

        private void LevelUp()
        {
            _levelTransition = true;
            _level++;
            _lives++;
            _fishesToCatch = _level + 3;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightSeaGreen);
            _spriteBatch.Begin();
            if ((!_levelTransition || !_paused) && _gameStarted)
            {
                _ship.Draw(_spriteBatch);

                foreach (var backgroundBubble in _bubbles)
                {
                    backgroundBubble.Draw(_spriteBatch);
                }

                foreach (var fish in _fishes)
                {
                    fish.Draw(_spriteBatch);
                }

                foreach (var net in _nets)
                {
                    net.Draw(_spriteBatch);
                }

                DrawUI();
            }

            if (!_gameStarted)
            {
                DrawSplashScreen();
            }

            if (_paused)
            {
                var pause = "Pauza";
                var pauseSize = _stateFont.MeasureString(pause);
                _spriteBatch.DrawString(_stateFont, pause, new Vector2((_screenWidth / 2) - pauseSize.X / 2,
                    (_screenHeight / 2) - pauseSize.Y / 2), Color.White);
            }

            if (_levelTransition)
            {
                DrawLevelTransition();
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawLevelTransition()
        {
            _spriteBatch.Draw(_splashScreen, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);

            string level = $"Poziom {_level}";
            const string getReady = "Przygotuj sie";

            var levelSize = _stateFont.MeasureString(level);
            var getReadySize = _stateFont.MeasureString(getReady);

            _spriteBatch.DrawString(_stateFont, level, new Vector2(_screenWidth / 2 - levelSize.X / 2, _screenHeight / 3), Color.DarkSeaGreen, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
            _spriteBatch.DrawString(_stateFont, getReady, new Vector2(_screenWidth / 2 - getReadySize.X / 2, _screenHeight / 2 - (getReadySize.Y / 3) + 50), Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 1f);
        }

        private void DrawSplashScreen()
        {
            _spriteBatch.Draw(_splashScreen, new Rectangle(0, 0, _screenWidth, _screenHeight), Color.White);

            const string title = "Underwater Attack";
            const string pressSpace = "Wcisnij spacje zaby zaczac";

            var titleSize = _stateFont.MeasureString(title);
            var pressSize = _stateFont.MeasureString(pressSpace);

            var titleScale = _screenWidth * 0.8f / titleSize.X;
            var pressScale = _screenWidth * 0.8f / pressSize.X;

            _spriteBatch.DrawString(_stateFont, title, new Vector2(_screenWidth / 2 - (titleSize.X * titleScale) / 2, _screenHeight / 3), Color.DarkSeaGreen, 0f, new Vector2(0, 0), titleScale, SpriteEffects.None, 1f);
            _spriteBatch.DrawString(_stateFont, pressSpace, new Vector2(_screenWidth / 2 - (pressSize.X / 2 * pressScale), (_screenHeight / 2 - (pressSize.Y / 3 * pressScale)) + 50), Color.White, 0f, new Vector2(0, 0), pressScale, SpriteEffects.None, 1f);
        }

        private void DrawUI()
        {
            var livesX = 750;
            var toCatchX = 50;

            _spriteBatch.DrawString(_uiFont, Chances, new Vector2(livesX, 25), Color.White);
            _spriteBatch.DrawString(_uiFont, $"Punkty: {_score}", new Vector2(50, 25), Color.Green);
            _spriteBatch.DrawString(_uiFont, ToCatch, new Vector2(toCatchX, 60), Color.White);

            var toCatchSize = _uiFont.MeasureString(Chances);
            var chancesSize = _uiFont.MeasureString(Chances);
            var heartLine = 0;
            var heartOffset = 0;
            var fishLine = 0;
            var fishOffset = 0;

            for (var i = 1; i <= _lives; i++)
            {
                if (chancesSize.X + _heart.Width / 2 * (i + 1 - heartOffset) + livesX > _screenWidth)
                {
                    heartOffset = i - 1;
                    heartLine++;
                }

                var heartX = chancesSize.X + livesX + _heart.Width / 2 * (i - heartOffset);
                var heartY = 25 + chancesSize.Y / 2 + heartLine * chancesSize.Y;

                _spriteBatch.Draw(_heart, new Vector2(heartX, heartY), null, Color.White, 0f, new Vector2(_heart.Width / 2, _heart.Height / 2), 0.5f, SpriteEffects.None, 1f);
            }

            for (var i = 1; i <= _fishesToCatch; i++)
            {
                if (toCatchSize.X + toCatchX + _fishTexture.Width / 2 + _fishTexture.Width / 2 * (i + 1 - fishOffset) > 500)
                {
                    fishOffset = i - 1;
                    fishLine++;
                }

                var fishX = toCatchSize.X + toCatchX + _fishTexture.Width / 2 + _fishTexture.Width / 2 * (i - fishOffset);
                var fishY = 60 + toCatchSize.Y / 2 + fishLine * toCatchSize.Y;

                _spriteBatch.Draw(_fishTexture, new Vector2(fishX, fishY), null, Color.White, 0f, new Vector2(_fishTexture.Width / 2, _fishTexture.Height / 2), 0.5f, SpriteEffects.None, 1f);
            }
        }

        private void KeyboardHandler()
        {
            var state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            if ((!_previousState.IsKeyDown(Keys.Pause) && !_previousState.IsKeyDown(Keys.P)) && (state.IsKeyDown(Keys.Pause) || state.IsKeyDown(Keys.P)))
            {
                var keys = _previousState.GetPressedKeys();
                _paused = !_paused;
            }

            if (!_previousState.IsKeyDown(Keys.Space) && state.IsKeyDown(Keys.Space) && _gameStarted)
            {
                var net = new Net(_netTexture, 1f)
                {
                    X = _ship.X + (_ship.Texture.Width / 2 * _ship.Scale), Y = _ship.Y, dX = _netSpeed
                };

                _nets.Add(net);
            }

            if (!_gameStarted)
            {
                if (state.IsKeyDown(Keys.Space))
                {
                    StartGame();
                    _gameStarted = true;
                }
                return;
            }

            if ((state.IsKeyDown(Keys.Up) || state.IsKeyDown(Keys.W)) && _ship.Y > 75 + _ship.Texture.Height)
            {
                _ship.dY = _shipSpeed * -1;
            }
            else if ((state.IsKeyDown(Keys.Down) || state.IsKeyDown(Keys.S)) &&
                    _ship.Y < _screenHeight - _ship.Texture.Height)
            {
                _ship.dY = _shipSpeed * 1;
            }
            else
            {
                _ship.dY = 0;
            }

            _previousState = state;
        }

        private void SpawnFish()
        {
            var spawnPosition = _random.Next(75, _screenHeight - 100);
            var scale = _random.Next(5,13) / 10f;

            var isCorrect = _random.Next(1, 100) <= 50;
            var equation = GenerateEquation(isCorrect);

            //var equation = GenerateEquation();

            var fish = new Fish(_fishTexture, scale, _random, _fishFont, equation, isCorrect) {X = _screenWidth + _fishTexture.Width / 2, Y = spawnPosition};

            fish.dX = (float) ((0 - fish.X) * (_fishSpeedMultiplier * Math.Sqrt(_level)));

            _fishes.Add(fish);
        }

        private string GenerateEquation(bool isCorrect)
        {
            var sign = (OperationSign)(_level % 4);
            int a;
            int b;
            switch (sign)
            {
                case OperationSign.Addition:
                    a = _random.Next(1, 100);
                    b = _random.Next(1, 100);
                    return isCorrect ? $"{a} + {b} = {a + b}" : $"{a} + {b} = {(a + b) + _random.Next(1,15)}";
                case OperationSign.Substraction:
                    do
                    {
                        a = _random.Next(1, 100);
                        b = _random.Next(1, 100);
                    } while (a < b);
                    return isCorrect ? $"{a} - {b} = {a - b}" : $"{a} - {b} = {(a - b) + _random.Next(1,15)}";
                case OperationSign.Multipliction:
                    a = _random.Next(1, 10);
                    b = _random.Next(1, 10);
                    return isCorrect ? $"{a} * {b} = {a * b}" : $"{a} * {b} = {(a * b) + _random.Next(1,15)}";
                case OperationSign.Division:
                    do
                    {
                        a = _random.Next(1, 100);
                        b = _random.Next(1, 100);
                    } while (a % b != 0);
                    return isCorrect ? $"{a} / {b} = {a / b}" : $"{a} / {b} = {(a / b) + _random.Next(1,15)}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartGame()
        {
            _bubbles = GenerateBackgroundBubbles();
            _ship.Y = _screenHeight / 2;
            _ship.X = (_ship.Texture.Width / 2) + 10;
            _level = 1;
            _lives = 5;
            _fishesToCatch = 3 + _level;
            _score = 0;
        }

        private List<BackgroundBubble> GenerateBackgroundBubbles()
        {
            var bubbles = new List<BackgroundBubble>();

            for (var i = 0; i < _bubbleAmount; i++)
            {
                var scale = _random.Next(3, 10) / 10f;
                var bubbleX = _random.Next(_screenWidth);
                var bubbleY = _random.Next(100, _screenHeight);
                var bubble = new BackgroundBubble(_bubbleTexture, scale) {X = bubbleX, Y = bubbleY, dX = (-80f) * (float)Math.Pow(scale * 2,2)};
                bubbles.Add(bubble);
            }

            return bubbles;
        }

    }
}
