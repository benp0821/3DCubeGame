using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
namespace _3DGame
{
    public class Cube
    {

        private Vector3 offset;
        private Vector3 size;

        Vector3 center;
        Vector3[] faces = new Vector3[6];

        VertexPositionColor[] vertices = new VertexPositionColor[8];
        short[] indices = new short[36];
        BoundingBox collision;
        public Color color;


        public Cube(Vector3 theOffset, Vector3 theSize, Color clr)
        {
            offset = theOffset;
            size = theSize;
            color = clr;
            SetUpIndices();
            SetUpVertices();
            collision = new BoundingBox(theOffset, theOffset + theSize);
            faces[0] = new Vector3(theSize.X/2, 0, 0);
            faces[1] = new Vector3(0, theSize.Y/2, 0);
            faces[2] = new Vector3(0, 0, theSize.Z/2);
            faces[3] = -faces[0];
            faces[4] = -faces[1];
            faces[5] = -faces[2];
            center = theOffset + theSize / 2;
        }

        private void SetUpVertices()
        {
            vertices[0].Position = new Vector3(0f, 0f, 0f) + offset;
            vertices[0].Color = color;

            vertices[1].Position = new Vector3(size.X, 0f, 0f) + offset;
            vertices[1].Color = color;

            vertices[2].Position = new Vector3(size.X, size.Y, 0f) + offset;
            vertices[2].Color = color;

            vertices[3].Position = new Vector3(0f, size.Y, 0f) + offset;
            vertices[3].Color = color;

            vertices[4].Position = new Vector3(0f, 0f, size.Z) + offset;
            vertices[4].Color = color;

            vertices[5].Position = new Vector3(0f, size.Y, size.Z) + offset;
            vertices[5].Color = color;

            vertices[6].Position = new Vector3(size.X, 0f, size.Z) + offset;
            vertices[6].Color = color;

            vertices[7].Position = new Vector3(size.X, size.Y, size.Z) + offset;
            vertices[7].Color = color;
        }


        private void SetUpIndices()
        {

            indices[0] = 0;
            indices[1] = 2;
            indices[2] = 1;

            indices[3] = 2;
            indices[4] = 0;
            indices[5] = 3;

            indices[6] = 4;
            indices[7] = 3;
            indices[8] = 0;

            indices[9] = 3;
            indices[10] = 4;
            indices[11] = 5;

            indices[12] = 6;
            indices[13] = 1;
            indices[14] = 2;

            indices[15] = 2;
            indices[16] = 7;
            indices[17] = 6;

            indices[15] = 2;
            indices[16] = 7;
            indices[17] = 6;

            indices[18] = 7;
            indices[19] = 5;
            indices[20] = 4;

            indices[21] = 7;
            indices[22] = 4;
            indices[23] = 6;

            indices[24] = 5;
            indices[25] = 7;
            indices[26] = 3;

            indices[27] = 7;
            indices[28] = 2;
            indices[29] = 3;

            indices[30] = 4;
            indices[31] = 0;
            indices[32] = 6;

            indices[33] = 6;
            indices[34] = 0;
            indices[35] = 1;
        }

        public VertexPositionColor[] getVertices()
        {
            return vertices;
        }

        public short[] getIndices()
        {
            return indices;
        }

        public BoundingBox getBoundingBox()
        {
            return collision;
        }

        public void setColors(Color c)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Color = c;
            }
        }

        public Vector3 checkFace(Vector3 bob)
        {
            float curDepth = Game1.reach;
            int curi = -1;
            for (int i = 0; i < faces.Length; i++)
            {
                float depth = Vector3.Distance(bob, faces[i] + center);
                if (depth.CompareTo(curDepth) < 0)
                {
                    curDepth = depth;
                    curi = i;
                }
            }
            return faces[curi]*2 + center;
        }


        public void debugDraw(GraphicsDevice device)
        {
            for (int i = 0; i < faces.Length; i++)
            {
                VertexPositionColor[] verts = new VertexPositionColor[2];
                verts[0] = new VertexPositionColor(center, Color.Red);
                verts[1] = new VertexPositionColor(faces[i]+center, Color.Red);
                device.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, verts, 0, 1);
            }
        }
    }
}
