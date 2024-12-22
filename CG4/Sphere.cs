using OpenTK.Mathematics;

namespace PhongLightingExample
{
   public class Sphere
    {
        public float[] Vertices;
        public uint[] Indices;

        public Sphere(int sectorCount, int stackCount, float radius)
        {
            List<float> verticesList = new List<float>();
            List<uint> indicesList = new List<uint>();
            int i, j;
            float x, y, z, xy;
            float nx, ny, nz;
            

            float sectorStep = 2 * MathHelper.Pi / sectorCount;
            float stackStep = MathHelper.Pi / stackCount;
            float sectorAngle, stackAngle;

            for (i = 0; i <= stackCount; ++i)
            {
                stackAngle = MathHelper.PiOver2 - i * stackStep;
                xy = radius * MathF.Cos(stackAngle);
                z = radius * MathF.Sin(stackAngle);
                for (j = 0; j <= sectorCount; ++j)
                {
                    sectorAngle = j * sectorStep;
                    x = xy * MathF.Cos(sectorAngle);
                    y = xy * MathF.Sin(sectorAngle);
                    verticesList.Add(x);
                    verticesList.Add(y);
                    verticesList.Add(z);

                    nx = x / radius;
                    ny = y / radius;
                    nz = z / radius;
                    verticesList.Add(nx);
                    verticesList.Add(ny);
                    verticesList.Add(nz);
                }
            }

            uint k1, k2;
            for (i = 0; i < stackCount; ++i)
            {
                k1 = (uint)(i * (sectorCount + 1));
                k2 = (uint)(k1 + sectorCount + 1);

                for (j = 0; j < sectorCount; ++j, ++k1, ++k2)
                {
                    if (i != 0)
                    {
                        indicesList.Add(k1);
                        indicesList.Add(k2);
                        indicesList.Add(k1 + 1);
                    }

                    if (i != (stackCount - 1))
                    {
                        indicesList.Add(k1 + 1);
                        indicesList.Add(k2);
                        indicesList.Add(k2 + 1);
                    }
                }
            }
            Vertices = verticesList.ToArray();
            Indices = indicesList.ToArray();
        }
    }
}