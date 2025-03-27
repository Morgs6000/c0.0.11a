namespace Minecraft;

public class Bush : Tile {
    public Bush(int id) : base(id) {
        tex = 15;
    }

    public override void OnLoad(Tesselator t, Level level, int x, int y, int z) {
        int tex = GetTexture(15);

        // Define as coordenadas de textura iniciais (u0, v0) e finais (u1, v1)
        // u0 e v0 representam o canto inferior esquerdo da textura no atlas
        float u0 = (float)(tex % 16) / 16.0f; // Coordenada u inicial
        float v0 = (16.0f - 1.0f - tex / 16) / 16.0f; // Coordenada v inicial
        
        // u1 e v1 representam o canto superior direito da textura no atlas
        float u1 = u0 + (1.0f / 16.0f); // Coordenada u final
        float v1 = v0 + (1.0f / 16.0f); // Coordenada v final

        int rots = 2;

        t.Color(1.0f, 1.0f, 1.0f);

        for(int r = 0; r < rots; r++) {
            float xa = (float)(Math.Sin((double)r * Math.PI / (double)rots + 0.7853981633974483f) * 0.5f);
            float za = (float)(Math.Cos((double)r * Math.PI / (double)rots + 0.7853981633974483f) * 0.5f);

            float x0 = (float)x + 0.5f - xa;
            float y0 = (float)y + 0.0f;
            float z0 = (float)z + 0.5f - za;

            float x1 = (float)x + 0.5f + xa;
            float y1 = (float)y + 1.0f;
            float z1 = (float)z + 0.5f + za;

            t.VertexUV(x1, y0, z1, u1, v0);
            t.VertexUV(x0, y0, z0, u0, v0);
            t.VertexUV(x0, y1, z0, u0, v1);
            t.VertexUV(x1, y1, z1, u1, v1);

            t.VertexUV(x0, y0, z0, u0, v0);
            t.VertexUV(x1, y0, z1, u1, v0);
            t.VertexUV(x1, y1, z1, u1, v1);
            t.VertexUV(x0, y1, z0, u0, v1);
        }
    }

    public override bool IsSolid() {
        return false;
    }
}
