﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine
{
    /// <summary>
    /// A game object to be drawn in a 3D space.
    /// </summary>
    public class GameObject
    {
        /// <summary>
        /// Used so the shader knows what kind of vertices to expect.
        /// </summary>
        public VertexDeclaration texturedVertexDeclaration;

        /// <summary>
        /// The actual verticies (and thier respective texture coordinates)
        /// </summary>
        public VertexPositionTexture[] vertices;

        /// <summary>
        /// The effect for the shader to use (I simply use an effect file I found online)
        /// </summary>
        public Effect myEffect;

        /// <summary>
        /// The position this game object is currently located.  This position is where
        /// the top left of the sprite is to be drawn.
        /// </summary>
        public virtual Vector3 position3D
        {
            get { return _position3D; }
            set 
            {
                //Vector3 distanceMoved = value - _position3D;
                _position3D = value;

                //for (int i = 0; i < vertices.GetLength(0); i++)
                //{
                //    vertices[i].Position += distanceMoved;
                //}
            }
        }
        protected Vector3 _position3D;

        public void makeHorizontal()
        {
            for (int i = 0; i < vertices.GetLength(0); i++)
            {
                float y = vertices[i].Position.Y;
                float z = vertices[i].Position.Z;

                vertices[i].Position = new Vector3(vertices[i].Position.X, z, -y);
            }
           
            //vertices[0].Position = new Vector3(0, newHeight, 0);
            //vertices[1].Position = new Vector3(newWidth, 0, 0);
            //vertices[2].Position = position3D;
            //vertices[3].Position = new Vector3(newWidth, 0, 0);
            //vertices[4].Position = new Vector3(0, newHeight, 0);
            //vertices[5].Position = new Vector3(newWidth, newHeight, 0);
        }

        /// <summary>
        /// The rectangle that encompasses the desire section of the sprite sheet that
        /// this object will currently use.  If it is set to the size of the sprite sheet
        /// this will operate as a normal texture.
        /// </summary>
        //public virtual Rectangle spriteSheetRectangle
        //{
        //    get { return _spriteSheetRectangle; }
        //    set { _spriteSheetRectangle = value; }
        //}
        //protected Rectangle _spriteSheetRectangle;

        public void setWidth(float newWidth)
        {
            //vertices[0].Position = new Vector3(0, newHeight, 0);
            vertices[1].Position = new Vector3(newWidth, vertices[1].Position.Y, vertices[1].Position.Z);
            //vertices[2].Position = position3D;
            vertices[3].Position = new Vector3(newWidth, vertices[1].Position.Y, vertices[1].Position.Z);
            //vertices[4].Position = new Vector3(0, newHeight, 0);
            vertices[5].Position = new Vector3(newWidth, vertices[1].Position.Y, vertices[1].Position.Z);
        }

        public void setHeight(float newHeight)
        {
            vertices[0].Position = new Vector3(0, newHeight, 0);
            //vertices[1].Position = new Vector3(vertices[1].Position.X, newHeight, vertices[1].Position.Z);
            //vertices[2].Position = position3D;
            //vertices[3].Position = new Vector3(vertices[1].Position.X,newHeight, vertices[1].Position.Z);
            vertices[4].Position = new Vector3(0, newHeight, 0);
            vertices[5].Position = new Vector3(vertices[1].Position.X, newHeight, vertices[1].Position.Z);
        }

        public int spriteSheetStepX
        {
            get
            {
                return _spriteSheetStepX;
            }
            set
            {
                _spriteSheetStepX = value;
                initalizeTextureCoords();
            }
        }
        protected int _spriteSheetStepX;

        public int spriteSheetStepY 
        {
            get
            {
                return _spriteSheetStepY;
            }
            set
            {
                _spriteSheetStepY = value;
                initalizeTextureCoords();
            }
        }
        protected int _spriteSheetStepY;

        public int pixelsPerSpriteSheetStepX { get; set; }
        public int pixelsPerSpriteSheetStepY { get; set; }

        private void initalizeTextureCoords()
        {
            float desiredTop = pixelsPerSpriteSheetStepY * _spriteSheetStepY / (float)spriteSheet.Height;
            float desiredBottom = pixelsPerSpriteSheetStepY * (_spriteSheetStepY + 1) / (float)spriteSheet.Height;
            float desiredLeft = pixelsPerSpriteSheetStepX * _spriteSheetStepX / (float)spriteSheet.Width;
            float desiredRight = pixelsPerSpriteSheetStepX * (_spriteSheetStepX + 1) / (float)spriteSheet.Width;

            vertices[0].TextureCoordinate.X = desiredLeft;
            vertices[0].TextureCoordinate.Y = desiredTop;

            vertices[1].TextureCoordinate.X = desiredRight;
            vertices[1].TextureCoordinate.Y = desiredBottom;

            vertices[2].TextureCoordinate.X = desiredLeft;
            vertices[2].TextureCoordinate.Y = desiredBottom;

            vertices[3].TextureCoordinate.X = desiredRight;
            vertices[3].TextureCoordinate.Y = desiredBottom;

            vertices[4].TextureCoordinate.X = desiredLeft;
            vertices[4].TextureCoordinate.Y = desiredTop;

            vertices[5].TextureCoordinate.X = desiredRight;
            vertices[5].TextureCoordinate.Y = desiredTop;
        }

        /// <summary>
        /// The physical texture this object will use.  This can be a sprite or a
        /// spritesheet.
        /// </summary>
        public Texture2D spriteSheet { get; set; }

        /// <summary>
        /// The center point of the sprite texture used.  This is really calculated using
        /// the provided sprite sheet rectangle.
        /// </summary>
        public Vector3 center3D;

        /// <summary>
        /// The speed and direction in which the sprite is to be currently moving
        /// </summary>
        public Vector3 velocity3D;

        /// <summary>
        /// The current scaling for this GameObject.
        /// </summary>
        public virtual Vector3 scale3D
        {
            get { return _scale3D; }
            set 
            {
                _scale3D = value;
                //float newWidth = pixelsPerSpriteSheetStepX * value.X;
                //float newHeight = pixelsPerSpriteSheetStepY * value.Y;

                //vertices[0].Position = position3D + new Vector3(0, newHeight, 0);
                //vertices[1].Position = position3D + new Vector3(newWidth, 0, 0);
                //vertices[2].Position = position3D;
                //vertices[3].Position = position3D + new Vector3(newWidth, 0, 0);
                //vertices[4].Position = position3D + new Vector3(0, newHeight, 0);
                //vertices[5].Position = position3D + new Vector3(newWidth, newHeight, 0);
            }
        }
        protected Vector3 _scale3D;

        public Vector3 rotation3D
        {
            get
            {
                return _rotation3D;
            }
            set
            {
                _rotation3D = value;
            }
        }
        protected Vector3 _rotation3D;

        /// <summary>
        /// A boolean value that dictates wether this sprite is to be drawn or not.
        /// </summary>
        public bool alive { get; set; }

        public virtual void draw(GraphicsDevice device, Matrix viewMatrix, Matrix projectionMatrix)
        {
            Matrix worldMatrix = Matrix.Identity;

            worldMatrix *= Matrix.CreateScale(scale3D);
            //worldMatrix *= Matrix.CreateFromAxisAngle(new Vector3(0.5f, 0f, 0f) * scale3D / 2, rotation3D.X);
            //worldMatrix *= Matrix.CreateFromAxisAngle(new Vector3(0f, 0.5f, 0f) * scale3D / 2, rotation3D.Y);
            //worldMatrix *= Matrix.CreateFromAxisAngle(new Vector3(0f, 0f, 0.5f) * scale3D / 2, rotation3D.Z);
            //worldMatrix.Translation -= scale3D / 2;
            worldMatrix *= Matrix.CreateFromYawPitchRoll(rotation3D.Y, rotation3D.X, rotation3D.Z);
            //worldMatrix.Translation += scale3D / 2;

            worldMatrix.Translation = position3D;
            myEffect.CurrentTechnique = myEffect.Techniques["Textured"];
            myEffect.Parameters["xWorld"].SetValue(worldMatrix);
            myEffect.Parameters["xView"].SetValue(viewMatrix);
            myEffect.Parameters["xProjection"].SetValue(projectionMatrix);
            myEffect.Parameters["xTexture"].SetValue(spriteSheet);
            myEffect.Begin();
            foreach (EffectPass pass in myEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                device.VertexDeclaration = texturedVertexDeclaration;
                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);

                pass.End();
            }
            myEffect.End();
        }

        public GameObject(Texture2D loadedTex, Effect effectToUse, GraphicsDevice device)
        {
            _spriteSheetStepX = 1;
            _spriteSheetStepY = 1;
            pixelsPerSpriteSheetStepX = 1;
            pixelsPerSpriteSheetStepY = 1;
            vertices = new VertexPositionTexture[6];
            spriteSheet = loadedTex;
            //spriteSheetRectangle = spriteRect;
            myEffect = effectToUse;
            position3D = Vector3.Zero;
            center3D = Vector3.Zero;
            velocity3D = Vector3.Zero;
            scale3D = Vector3.One;
            rotation3D = Vector3.Zero;
            texturedVertexDeclaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

            vertices[0].Position = new Vector3(0, 1, 0);
            vertices[1].Position = new Vector3(1, 0, 0);
            vertices[2].Position = Vector3.Zero;
            vertices[3].Position = new Vector3(1, 0, 0);
            vertices[4].Position = new Vector3(0, 1, 0);
            vertices[5].Position = new Vector3(1, 1, 0);
        }

        /// <summary>
        /// Obtains the center position of this object in 3D space.
        /// </summary>
        /// <returns>A Vector3 representing the position.</returns>
        public Vector3 getCenterPosition()
        {
            return new Vector3(position3D.X + (center3D.X * scale3D.X),
                               position3D.Y + (center3D.Y * scale3D.Y),
                               position3D.Z + (center3D.Z * scale3D.Z));
        }
    }
}
