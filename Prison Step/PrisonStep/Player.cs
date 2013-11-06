using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class describes our player in the game. 
    /// </summary>
    public class Player
    {
        #region Fields

        /// <summary>
        /// List of regions for collision testing. The list of Vector2 objects
        /// is the list of triangles. Each triangle will have 3 vertices. 
        /// </summary>
        private Dictionary<string, List<Vector2>> regions = new Dictionary<string, List<Vector2>>();
        private Dictionary<string, Vector3> doorLocations = new Dictionary<string, Vector3>();
        /// <summary>
        /// Game that uses this player
        /// </summary>
        private PrisonGame game;

        //
        // Player location information.  We keep a x/z location (y stays zero)
        // and an orientation (which way we are looking).
        //

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        private Vector3 location = new Vector3(275, 0, 1053);

        /// <summary>
        /// The player orientation as a simple angle
        /// </summary>
        private float orientation = 1.6f;

        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        private Matrix transform;

        /// <summary>
        /// The rotation rate in radians per second when player is rotating
        /// </summary>
        private float panRate = 2;

        /// <summary>
        /// The player move rate in centimeters per second
        /// </summary>
        private float moveRate = 500;

        private string regionIn;

        #endregion

        public string RegionIn { get { return regionIn; } }

        public Player(PrisonGame game)
        {
            this.game = game;
            SetPlayerTransform();
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }


        public void LoadContent(ContentManager content)
        {
            Model model = content.Load<Model>("AntonPhibesCollision");
            Matrix[] M = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(M);

            foreach (ModelMesh mesh in model.Meshes)
            {
                // For accumulating the triangles for this mesh
                List<Vector2> triangles = new List<Vector2>();

                // Loop over the mesh parts
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // 
                    // Obtain the vertices for the mesh part
                    //

                    int numVertices = meshPart.VertexBuffer.VertexCount;
                    VertexPositionColorTexture[] verticesRaw = new VertexPositionColorTexture[numVertices];
                    meshPart.VertexBuffer.GetData<VertexPositionColorTexture>(verticesRaw);
                    //
                    // Obtain the indices for the mesh part
                    //

                    int numIndices = meshPart.IndexBuffer.IndexCount;
                    short[] indices = new short[numIndices];
                    meshPart.IndexBuffer.GetData<short>(indices);
                    //
                    // Build the list of triangles
                    //

                    for (int i = 0; i < meshPart.PrimitiveCount * 3; i++)
                    {
                        // The actual index is relative to a supplied start position
                        int index = i + meshPart.StartIndex;

                        // Transform the vertex into world coordinates
                        Vector3 v = Vector3.Transform(verticesRaw[indices[index] + meshPart.VertexOffset].Position, M[mesh.ParentBone.Index]);
                        triangles.Add(new Vector2(v.X, v.Z));
                    }

                }

                regions[mesh.Name] = triangles;
            }

            doorLocations["R_Door1"] = new Vector3(218,0, 1023);
            doorLocations["R_Door2"] = new Vector3(-11,0, -769);
            doorLocations["R_Door3"] = new Vector3(587,0, -999);
            doorLocations["R_Door4"] = new Vector3(787,0, -763);
            doorLocations["R_Door5"] = new Vector3(1187,0, -1218);


        }


        public void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // How much we will move the player
            float translation = 0;
            float rotation = 0;
            
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                rotation += panRate * delta;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                rotation -= panRate * delta;
            }

            

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                translation += moveRate * delta;
            }

            if (keyboardState.IsKeyDown(Keys.Down))
            {
                translation -= moveRate * delta;
            }

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            rotation += -gamePadState.ThumbSticks.Right.X * panRate * delta;
            translation += gamePadState.ThumbSticks.Right.Y * moveRate * delta;

            //
            // Update the orientation
            //

            orientation += rotation;

            //
            // Update the location
            //

            Vector3 translateVector = new Vector3((float)Math.Sin(orientation), 0, (float)Math.Cos(orientation));
            Vector3 direction = translateVector;
            translateVector *= translation;
           
            Vector3 newLocation = location + translateVector;
            regionIn = TestRegion(newLocation);
         
           // System.Diagnostics.Trace.WriteLine(regionIn);
            translateVector.Normalize();
            if (regionIn != "")
            {
                location = newLocation;
            }
            Console.WriteLine(regionIn);

            if (regionIn == "R_Door1")
            {
                Vector3 vec = (doorLocations[regionIn] - location);
                vec.Normalize();
                if (CalculateAngleToDoor(vec, direction) > .70)
                {
                    game.PrisonModels[0].OpenDoor();
                }
                else
                    game.PrisonModels[0].CloseDoor();
            }
            else
                game.PrisonModels[0].CloseDoor();

                    
                


            SetPlayerTransform();

            //
            // Make the camera follow the player
            //

            game.Camera.Eye = location + new Vector3(0, 180, 0);
            game.Camera.Center = game.Camera.Eye + transform.Backward + new Vector3(0, -0.1f, 0);
        }

        private float CalculateAngleToDoor(Vector3 vec1, Vector3 vec2)
        {
            return Vector3.Dot(vec1, vec2);
        }

               /// <summary>
        /// Test to see if we are in some region.
        /// </summary>
        /// <param name="v3">The region name or a blank string if not in a region.</param>
        /// <returns></returns>
        private string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            foreach (KeyValuePair<string, List<Vector2>> region in regions)
            {
                // For now we ignore the walls
                if (region.Key.StartsWith("W"))
                    continue;

                for (int i = 0; i < region.Value.Count; i += 3)
                {
                    float x1 = region.Value[i].X;
                    float x2 = region.Value[i + 1].X;
                    float x3 = region.Value[i + 2].X;
                    float y1 = region.Value[i].Y;
                    float y2 = region.Value[i + 1].Y;
                    float y3 = region.Value[i + 2].Y;

                    float d = 1.0f / ((x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3));
                    float l1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) * d;
                    if (l1 < 0)
                        continue;

                    float l2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) * d;
                    if (l2 < 0)
                        continue;

                    float l3 = 1 - l1 - l2;
                    if (l3 < 0)
                        continue;

                    return region.Key;
                }
            }

            return "";
        }



        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
        }



    }
}
