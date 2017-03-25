using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _3DGame
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GraphicsDevice device;

        BasicEffect effect;
        SpriteFont font;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        Random rand = new Random();

        List<Cube> cubeList = new List<Cube>();
        byte x = 50;
        byte y = 5;
        byte z = 50;

        byte mouseClickTime = 0;
        byte mouseRightClickTime = 0;
        byte maxClickTime = 30;
        byte maxRightClickTime = 30;
        public static float reach = 20;

        BoundingSphere cameraBox;

        Vector3 cameraPosition = new Vector3(0, 20, 0);
        float leftrightRot = -MathHelper.PiOver4 - 0.5f;
        float updownRot = -MathHelper.Pi / 10.0f;
        float rotationSpeed = 0.3f;
        float moveSpeed = 15.0f;
        MouseState originalMouseState;

        KeyboardState keyState;
        KeyboardState previousKeyState;

        Texture2D crosshair;
        Rectangle crosshairRect;

        Vector3 direction;

        Vector3 placeCubeSize = new Vector3(2.5f, 2.5f, 2.5f);

        private bool flying = false;
        private bool jump = false;
        private int jumpCounter = 15;
        private bool isGrounded;
        private int fallingSpeed = 1;

        bool? seizure = false;

        private bool showText = true;

        float elapsedTime;
        int totalFrames = 0;
        int fps = 0;

        List<String> menuOptions = new List<String>();
        int menuSelection = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.Viewport.Bounds.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.Viewport.Bounds.Height;
            graphics.IsFullScreen = true;
            graphics.PreferMultiSampling = true;
            IsMouseVisible = false;

            graphics.ApplyChanges();
            Window.Title = "3D Cube Demo";
            cameraBox = new BoundingSphere(cameraPosition, 2.5f);
            crosshairRect = new Rectangle(GraphicsDevice.Viewport.Width / 2 - 10, GraphicsDevice.Viewport.Height / 2 - 10, 20, 20);

            for (int i = 0; i < 8; i++)
            {
                menuOptions.Add("");
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            device = graphics.GraphicsDevice;

            effect = new BasicEffect(device);//Content.Load<Effect>("effects");
            effect.FogEnabled = true;
            effect.FogColor = Color.Black.ToVector3();
            effect.FogEnd = 300f;
            effect.FogStart = 25f;
            effect.VertexColorEnabled = true;

            crosshair = this.Content.Load<Texture2D>("crosshair");

            font = Content.Load<SpriteFont>("SpriteFont1");

            for (byte i = 0; i < x; i++)
            {
                for (byte j = 0; j < y; j++)
                {
                    for (byte k = 0; k < z; k++)
                    {
                        //cubeList.Add(new Cube((float)(i * 5), (float)(j * 5), (float)(k * -5), 5f, 5f, 5f, new Color(rand.Next(255), rand.Next(255), rand.Next(255), 125)));
                        //cubeList.Add(new Cube((float)(i * 5), (float)(j * 2.5), (float)(k * -5), 5f, 2.5f, 5f, new Color(rand.Next(255), rand.Next(255), rand.Next(255), 125)));
                        cubeList.Add(new Cube(new Vector3(i * 2.5f, j * 2.5f, k * -2.5f), new Vector3(2.5f, 2.5f, 2.5f), new Color(rand.Next(255), rand.Next(255), rand.Next(255), 125)));
                    }
                }
            }

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 1000.0f);
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();

            previousKeyState = Keyboard.GetState();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (elapsedTime >= 1000f)
            {
                fps = totalFrames;
                totalFrames = 0;
                elapsedTime = 0;
            }

            if (cameraPosition.Y < -100)
            {
                cameraPosition = new Vector3(0, 20, 0);
                cameraBox.Center = cameraPosition;
            }

            CheckMouse();

            ProcessInput(timeDifference);

            menuOptions[0] = (flying ? "Flying: Yes" : "Flying: No") + " (Hotkey: E)";
            menuOptions[1] = "Move Speed: " + moveSpeed.ToString();
            menuOptions[2] = "Rotation Speed: " + Math.Round(rotationSpeed, 2, MidpointRounding.AwayFromZero).ToString();
            menuOptions[3] = "Break Time: " + maxClickTime.ToString();
            menuOptions[4] = "Place Time: " + maxRightClickTime.ToString();
            menuOptions[5] = "Reach: " + reach.ToString();
            menuOptions[6] = (seizure == null) ? "Warning: Rapid Flashing" : ("Disco Mode: " + seizure.ToString());
            menuOptions[7] = "Cube Placement Size: " + placeCubeSize.ToString() + " (\"X/Y/Z\" + \"Arrow Keys\")";

            base.Update(gameTime);
        }

        private void CheckMouse()
        {
            MouseState mousePosition = Mouse.GetState();
            if (mousePosition.LeftButton == ButtonState.Released)
            {
                mouseClickTime = 0;
            }
            if (mousePosition.RightButton == ButtonState.Released)
            {
                mouseRightClickTime = maxRightClickTime;
            }
            mouseRightClickTime++;

            if (mousePosition.LeftButton == ButtonState.Pressed)
            {
                Vector3 nearPoint = cameraPosition;
                Vector3 farPoint = viewMatrix.Forward * 50;

                Viewport viewport = GraphicsDevice.Viewport;
                //nearPoint = viewport.Unproject(nearPoint, projectionMatrix, viewMatrix, Matrix.Identity);
                //farPoint = viewport.Unproject(farPoint, projectionMatrix, viewMatrix, Matrix.Identity);

                //Vector3 direction = farPoint - nearPoint;
                //direction.Normalize();
                Matrix camera = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
                direction = camera.Forward;
                direction.Normalize();

                Ray ray = new Ray(cameraPosition, direction);
                float curDepth = reach;
                int curi = -1;
                for (int i = 0; i < cubeList.Count; i++)
                {
                    float? depth = cubeList[i].getBoundingBox().Intersects(ray);
                    if (depth != null && depth < curDepth)
                    {
                        curDepth = depth.Value;
                        curi = i;
                    }
                }
                if (curi >= 0)
                    if (mouseClickTime > maxClickTime)
                    {
                        cubeList.RemoveAt(curi);
                        mouseClickTime = 0;
                    }
                    else
                    {
                        mouseClickTime++;
                        cubeList[curi].setColors(new Color(rand.Next(255), rand.Next(255), rand.Next(255), 128));
                    }
            }
            else if (mousePosition.RightButton == ButtonState.Pressed)
            {
                Vector3 nearPoint = cameraPosition;
                Vector3 farPoint = viewMatrix.Forward * 50;

                //Viewport viewport = GraphicsDevice.Viewport;
                //nearPoint = viewport.Unproject(nearPoint, projectionMatrix, viewMatrix, Matrix.Identity);
                //farPoint = viewport.Unproject(farPoint, projectionMatrix, viewMatrix, Matrix.Identity);

                //Vector3 direction = farPoint - nearPoint;
                //direction.Normalize();
                Matrix camera = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
                direction = camera.Forward;
                direction.Normalize();

                Ray ray = new Ray(cameraPosition, direction);
                float curDepth = reach;
                int curi = -1;
                for (int i = 0; i < cubeList.Count; i++)
                {
                    float? depth = cubeList[i].getBoundingBox().Intersects(ray);
                    if (depth != null && depth < curDepth)
                    {
                        curDepth = depth.Value;
                        curi = i;
                    }
                }
                if (curi >= 0)
                {
                    Vector3 point = cameraPosition + (direction * curDepth);
                    Vector3 v = cubeList[curi].checkFace(point);
                    {
                        if (mouseRightClickTime > maxRightClickTime)
                        {
                            Vector3 temp = new Vector3(placeCubeSize.X / 2, placeCubeSize.Y / 2, placeCubeSize.Z / 2);
                            Cube c = new Cube(new Vector3(v.X - temp.X, v.Y - temp.Y, v.Z - temp.Z), placeCubeSize, new Color(rand.Next(255), rand.Next(255), rand.Next(255)));
                            cubeList.Add(c);
                            if (c.getBoundingBox().Intersects(cameraBox))
                            {
                                cubeList.Remove(c);
                            }
                            mouseRightClickTime = 0;
                        }
                    }
                }
            }
        }


        private void ProcessInput(float amount)
        {
            MouseState currentMouseState = Mouse.GetState();
            if (currentMouseState != originalMouseState)
            {
                float xDifference = currentMouseState.X - originalMouseState.X;
                float yDifference = currentMouseState.Y - originalMouseState.Y;
                leftrightRot -= rotationSpeed * xDifference * amount;
                updownRot -= rotationSpeed * yDifference * amount;
                if (updownRot < -Math.PI / 2)
                {
                    updownRot = (float)-Math.PI / 2;
                }
                if (updownRot > Math.PI / 2)
                {
                    updownRot = (float)Math.PI / 2;
                }
                Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
                UpdateViewMatrix();
            }

            keyState = Keyboard.GetState();

            Vector3 moveVector = new Vector3(0, 0, 0);
            if (keyState.IsKeyDown(Keys.W))
            {
                moveVector += new Vector3(0, 0, -1);
                AddToCameraPosition(moveVector * amount);

                if (cameraCollision())
                {
                    AddToCameraPosition(-moveVector * amount);
                }
            }

            moveVector = new Vector3(0, 0, 0);
            if (keyState.IsKeyDown(Keys.S))
            {
                moveVector += new Vector3(0, 0, 1);
                AddToCameraPosition(moveVector * amount);

                if (cameraCollision())
                {
                    AddToCameraPosition(-moveVector * amount);
                }
            }

            moveVector = new Vector3(0, 0, 0);
            if (keyState.IsKeyDown(Keys.D))
            {
                moveVector += new Vector3(1, 0, 0);
                AddToCameraPosition(moveVector * amount);

                if (cameraCollision())
                {
                    AddToCameraPosition(-moveVector * amount);
                }
            }

            moveVector = new Vector3(0, 0, 0);
            if (keyState.IsKeyDown(Keys.A))
            {
                moveVector += new Vector3(-1, 0, 0);
                AddToCameraPosition(moveVector * amount);

                if (cameraCollision())
                {
                    AddToCameraPosition(-moveVector * amount);
                }
            }

            if (flying)
            {
                moveVector = new Vector3(0, 0, 0);
                if (keyState.IsKeyDown(Keys.Space))
                {
                    moveVector += new Vector3(0, 1, 0);
                    AddToCameraPosition(moveVector * amount);

                    if (cameraCollision())
                    {
                        AddToCameraPosition(-moveVector * amount);
                    }
                }

                moveVector = new Vector3(0, 0, 0);
                if (keyState.IsKeyDown(Keys.LeftShift))
                {

                    moveVector += new Vector3(0, -1, 0);
                    AddToCameraPosition(moveVector * amount);

                    if (cameraCollision())
                    {
                        AddToCameraPosition(-moveVector * amount);
                    }
                }
                fallingSpeed = 1;
            }

            moveVector = new Vector3(0, 0, 0);
            if (!flying && !jump)
            {
                moveVector += new Vector3(0, -1, 0);

                AddToCameraPosition(moveVector * amount);


                if (cameraCollision())
                {
                    AddToCameraPosition(-moveVector * amount);
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }
                if (keyState.IsKeyDown(Keys.Space) && isGrounded == true)
                {
                    jump = true;
                }

                if (isGrounded)
                {
                    fallingSpeed = 1;
                }
                else
                {
                    fallingSpeed++;
                }
            }
            else if (jump)
            {
                moveVector += new Vector3(0, 1, 0);
                AddToCameraPosition(moveVector * amount);

                if (cameraCollision())
                {
                    AddToCameraPosition(-moveVector * amount);
                }
                jumpCounter--;
                if (jumpCounter <= 0)
                {
                    jumpCounter = 15;
                    jump = false;
                }
            }

            if (showText)
            {
                if (keyState.IsKeyDown(Keys.Left) && previousKeyState.IsKeyUp(Keys.Left))
                {
                    if (menuSelection == 0)
                    {
                        flying = !flying;
                    }
                    if (menuSelection == 1)
                    {
                        if (moveSpeed > 1)
                            moveSpeed--;
                    }
                    if (menuSelection == 2)
                    {
                        if (rotationSpeed > .02f)
                            rotationSpeed = rotationSpeed - .01f;
                    }
                    if (menuSelection == 3)
                    {
                        if (maxClickTime > 1)
                            maxClickTime--;
                    }
                    if (menuSelection == 4)
                    {
                        if (maxRightClickTime > 1)
                            maxRightClickTime--;
                    }
                    if (menuSelection == 5)
                    {
                        if (reach > 3)
                            reach--;
                    }
                    if (menuSelection == 6)
                    {
                        if (seizure == null)
                        {
                            seizure = true;
                        }
                        else if (seizure == true)
                        {
                            seizure = false;
                        }
                        else
                        {
                            seizure = null;
                        }
                    }
                    if (menuSelection == 7)
                    {
                        if (keyState.IsKeyDown(Keys.X))
                        {
                            if (placeCubeSize.X > .5)
                            {
                                placeCubeSize -= new Vector3(.5f, 0, 0);
                            }
                        }
                        if (keyState.IsKeyDown(Keys.Y))
                        {
                            if (placeCubeSize.Y > .5)
                            {
                                placeCubeSize -= new Vector3(0, .5f, 0);
                            }
                        }
                        if (keyState.IsKeyDown(Keys.Z))
                        {
                            if (placeCubeSize.Z > .5)
                            {
                                placeCubeSize -= new Vector3(0, 0, .5f);
                            }
                        }

                    }
                }

                if (keyState.IsKeyDown(Keys.Right) && previousKeyState.IsKeyUp(Keys.Right))
                {
                    if (menuSelection == 0)
                    {
                        flying = !flying;
                    }
                    if (menuSelection == 1)
                    {
                        moveSpeed++;
                    }
                    if (menuSelection == 2)
                    {
                        if (rotationSpeed < 1.99f)
                            rotationSpeed = rotationSpeed + .01f;
                    }
                    if (menuSelection == 3)
                    {
                        maxClickTime++;
                    }
                    if (menuSelection == 4)
                    {
                        maxRightClickTime++;
                    }
                    if (menuSelection == 5)
                    {
                        reach++;
                    }
                    if (menuSelection == 6)
                    {
                        if (seizure == null)
                        {
                            seizure = true;
                        }
                        else if (seizure == true)
                        {
                            seizure = false;
                        }
                        else
                        {
                            seizure = null;
                        }
                    }
                    if (menuSelection == 7)
                    {
                        if (keyState.IsKeyDown(Keys.X))
                        {
                            placeCubeSize += new Vector3(.5f, 0, 0);
                        }


                        if (keyState.IsKeyDown(Keys.Y))
                        {
                            placeCubeSize += new Vector3(0, .5f, 0);
                        }


                        if (keyState.IsKeyDown(Keys.Z))
                        {
                            placeCubeSize += new Vector3(0, 0, .5f);
                        }
                    }
                }
            }

            if (keyState.IsKeyDown(Keys.R) && previousKeyState.IsKeyUp(Keys.R))
            {
                showText = !showText;
                menuSelection = 0;
            }

            //NAVIGATING THE MENU
            if (keyState.IsKeyDown(Keys.Down) && previousKeyState.IsKeyUp(Keys.Down))
            {
                menuSelection++;
                if (menuSelection > menuOptions.Count - 1)
                {
                    menuSelection = 0;
                }
            }
            if (keyState.IsKeyDown(Keys.Up) && previousKeyState.IsKeyUp(Keys.Up))
            {
                menuSelection--;
                if (menuSelection < 0)
                {
                    menuSelection = menuOptions.Count - 1;
                }
            }


            //FLYING HOTKEY
            if (keyState.IsKeyDown(Keys.E) && previousKeyState.IsKeyUp(Keys.E))
            {
                flying = !flying;
            }

            previousKeyState = keyState;
        }



        public bool cameraCollision()
        {
            for (int i = 0; i < cubeList.Count; i++)
            {
                bool collision = cameraBox.Intersects(cubeList[i].getBoundingBox());
                if (collision)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += (moveSpeed + fallingSpeed) * rotatedVector;
            cameraBox.Center = cameraPosition;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 cameraOriginalUpVector = new Vector3(0, 1, 0);
            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, cameraRotatedUpVector);


        }

        protected override void Draw(GameTime gameTime)
        {
            totalFrames++;
            device.Clear(Color.Black);

            device.RasterizerState = RasterizerState.CullClockwise;


            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            effect.Projection = projectionMatrix;
            effect.View = viewMatrix;
            effect.World = Matrix.Identity;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                for (int i = 0; i < cubeList.Count; i++)
                {
                    if (seizure == true)
                        cubeList[i].setColors(new Color(rand.Next(255), rand.Next(255), rand.Next(255)));
                    device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, cubeList[i].getVertices(), 0, cubeList[i].getVertices().Length, cubeList[i].getIndices(), 0, cubeList[i].getIndices().Length / 3);
                    //cubeList[i].debugDraw(GraphicsDevice);
                }
            }



            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            spriteBatch.DrawString(font, "Toggle Menu: R", new Vector2(GraphicsDevice.Viewport.Width - 170, 0), Color.White);
            spriteBatch.Draw(crosshair, crosshairRect, Color.White);
            spriteBatch.DrawString(font, Window.Title, Vector2.Zero, Color.White);

            if (showText)
            {
                int j = 0;
                for (int i = 0; i < menuOptions.Count; i++)
                {
                    j += 20;
                    spriteBatch.DrawString(font, menuOptions[i], new Vector2(0, j), Color.White);
                    if (menuOptions[menuSelection] == menuOptions[i])
                    {
                        spriteBatch.DrawString(font, menuOptions[menuSelection], new Vector2(1, j + 1), Color.White);
                        //spriteBatch.DrawString(font, menuOptions[menuSelection], new Vector2(-1, j - 1), Color.White);
                    }
                }



                spriteBatch.DrawString(font, (placeCubeSize != new Vector3(2.5f, 2.5f, 2.5f) ? "Warning: Changing this setting may cause strange bugs to occur" : ""), new Vector2(0, 180), Color.White);

                spriteBatch.DrawString(font, "Left Click: Break Blocks/Right Click: Place Blocks", new Vector2(0, GraphicsDevice.Viewport.Height - 50), Color.White);
                spriteBatch.DrawString(font, "WASD to move, shift to go down, space to go up, arrow keys to navigate menu", new Vector2(0, GraphicsDevice.Viewport.Height - 30), Color.White);
                //spriteBatch.DrawString(font, "FPS= " + fps, new Vector2(GraphicsDevice.Viewport.Width - 100, 40), Color.White);
                
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
