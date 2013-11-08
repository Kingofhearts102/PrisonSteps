using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class implements one section of our prison ship
    /// </summary>
    public class PrisonModel
    {
        #region Fields

        enum DoorStates { Open, Opening, Closed, Closing };
        DoorStates state;
        /// <summary>
        /// The section (6) of the ship
        /// </summary>
        private int section;
        private float doorTranslation = 0;
        private float timeToRasieDoor = 2;
        private bool canMoveThrough = false;
        /// <summary>
        /// The name of the asset (FBX file) for this section
        /// </summary>
        private string asset;

        /// <summary>
        /// The game we are associated with
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The XNA model for this part of the ship
        /// </summary>
        private Model model;

        /// <summary>
        /// To make animation possible and easy, we save off the initial (bind) 
        /// transformation for all of the model bones. 
        /// </summary>
        private Matrix[] bindTransforms;

        /// <summary>
        /// The is the transformations for all model bones, potentially after we
        /// have made some change in the tranformation.
        /// </summary>
        private Matrix[] boneTransforms;

        /// <summary>
        /// A list of all of the door bones in the model.
        /// </summary>
        private List<int> doors = new List<int>();

        #endregion

        public bool CanMoveThrough
        {
            get { return canMoveThrough; }
        }


        #region Construction and Loading

        /// <summary>
        /// Constructor. Creates an object for a section.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="section"></param>
        public PrisonModel(PrisonGame game, int section)
        {
            state = DoorStates.Closed;
            this.game = game;
            this.section = section;
            this.asset = "AntonPhibes" + section.ToString();
        }

        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            // Load the second model
            model = content.Load<Model>(asset);

            // Save off all of hte bone information
            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            boneTransforms = new Matrix[boneCnt];

            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(boneTransforms);

            // Find all of the doors and save the index for the bone
            for (int b = 0; b < boneCnt; b++)
            {
                if (model.Bones[b].Name.StartsWith("DoorInner") || model.Bones[b].Name.StartsWith("DoorOuter"))
                {
                    doors.Add(b);
                }

            }

            // As supplied, all of the doors are closed. This code opens each door by 
            // translating it up 2 meters.
           // foreach (int d in doors)
           // {
                //boneTransforms[d] = Matrix.CreateTranslation(0, 0, 0) * bindTransforms[d];
           // }

         
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (state)
            {
                case DoorStates.Closed:
                    canMoveThrough = false;
                    break;
                case DoorStates.Open:
                    canMoveThrough = true;
                    break;
                    
                case DoorStates.Opening:
                    canMoveThrough = false;
                    doorTranslation += 200 *delta / timeToRasieDoor;

                    if (doorTranslation >= 200)
                    {
                        state = DoorStates.Open;
                        doorTranslation = 200;
                    }
                    foreach (int d in doors)
                    {                        
                        boneTransforms[d] = Matrix.CreateTranslation(0, doorTranslation, 0) * bindTransforms[d];
                    }
                    break;

                case DoorStates.Closing:
                    canMoveThrough = false;
                    doorTranslation -= 200 * delta / timeToRasieDoor;
                    if (doorTranslation <= 0)
                    {
                        state = DoorStates.Closed;
                        doorTranslation = 0;
                    }
                    foreach (int d in doors)
                    {
                        boneTransforms[d] = Matrix.CreateTranslation(0, doorTranslation, 0) * bindTransforms[d];
                    }
                    break;

            }



        }

        public void OpenDoor(int x)
        {
            if (state != DoorStates.Open) 
                state = DoorStates.Opening;
                //boneTransforms[d] = Matrix.CreateTranslation(0, 200, 0) * bindTransforms[d];
            

        }
        public void CloseDoor(int x)
        {
            if(state!= DoorStates.Closed)
                state = DoorStates.Closing;
                //boneTransforms[d] = Matrix.CreateTranslation(0, 0, 0) * bindTransforms[d];
            

        }

        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {

            DrawModel(graphics, model, Matrix.Identity);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            // Apply the bone transforms
            Matrix[] absoTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsFrom(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] *world);
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                    effect.Parameters["ScreenGooPosition"].SetValue(game.SlimeTime);
                }
                mesh.Draw();
            }
        }

        #endregion

    }
}
