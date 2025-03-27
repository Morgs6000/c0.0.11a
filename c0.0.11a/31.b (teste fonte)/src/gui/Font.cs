using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace Minecraft;

public class Font {
    private Shader shader;
    private Texture texture;
    private Tesselator t;

    // Array que armazena a largura de cada caractere (0-255)
    private int[] charWidths = new int[256];

    public Font() {
        // Cria uma instância do shader, carregando os arquivos de vertex e fragment shader
        shader = new Shader("src/shaders/texture_vertex.glsl", "src/shaders/texture_fragment.glsl");
        
        // Cria uma instância da textura, carregando a imagem do arquivo especificado
        texture = new Texture("src/textures/default.gif");

        t = new Tesselator(shader);

        // Carrega a imagem da textura
        string path = "src/textures/default.gif";
        ImageResult img = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);

        int w = img.Width;  // Largura da imagem
        int h = img.Height; // Altura da imagem

        // Obtém os pixels da imagem
        // int[] rawPixels = new int[w * h];
        ReadOnlySpan<int> rawPixels = MemoryMarshal.Cast<byte, int>(img.Data);

        //*
        // Calcula a largura de cada caractere analisando os pixels
        for(int i = 0; i < 128; i++) {
            /*
            int xt = i % 16; // Posição X do caractere na textura (0-15)
            int yt = i / 16; // Posição Y do caractere na textura (0-7)
            int x = 0;       // Contador de largura do caractere

            // Verifica colunas até encontrar uma vazia ou chegar na largura máxima
            for(bool emptyColumn = false; x < 8 && !emptyColumn; x++) {
                int xPixel = xt * 8 + x; // Posição X do pixel atual

                emptyColumn = true;      // Assume que a coluna está vazia

                // Verifica todos os pixels na coluna vertical
                for(int y = 0; y < 8 && emptyColumn; ++y) {
                    int yPixel = (yt * 8 + y) * w; // Posição Y do pixel
                    int pixel = rawPixels[xPixel + yPixel] & 255; // Intensidade do pixel

                    if(pixel > 128) {
                        emptyColumn = false; // Encontrou pixel não-vazio
                    }
                }
            }

            // Define largura fixa para espaço
            if(i == 32) {
                x = 4;
            }
            
            charWidths[i] = x; // Armazena a largura calculada
            //*/

            //*
            // Largura padrão para caracteres não especificados
            charWidths[i] = 6;

            // Define largura fixa para espaço
            charWidths[32] = 4;

            // Caracteres especiais
            charWidths['!'] = 2;
            charWidths['"'] = 5; 
            charWidths['%'] = 7; 
            charWidths['&'] = 7; 
            charWidths[39] = 3;
            charWidths['('] = 5; 
            charWidths[')'] = 5; 
            charWidths['*'] = 8; 
            charWidths[','] = 2; 
            charWidths['.'] = 2;

            charWidths[':'] = 2; 
            charWidths[';'] = 2; 
            charWidths['<'] = 5; 
            charWidths['>'] = 5; 
            charWidths['@'] = 7;

            // Letras maiusculas
            charWidths['I'] = 2;

            // Letras minusculas
            charWidths['f'] = 5; 
            charWidths['i'] = 2; 
            charWidths['k'] = 5; 
            charWidths['l'] = 3; 
            charWidths['t'] = 4;
            //*/
        }
        //*/

        /*
        for (int i = 0; i < charWidths.Length; i++) {
            charWidths[i] = 8; // Valor padrão para maioria dos caracteres
        }
        //*/

        /*
        // Calcula a largura de cada caractere analisando os pixels
        for(int i = 0; i < 128; i++) {
            int xt = i % 16; // Posição X do caractere na textura (0-15)
            int yt = i / 16; // Posição Y do caractere na textura (0-7)
            int x = 0;       // Contador de largura do caractere

            // Verifica colunas até encontrar uma vazia ou chegar na largura máxima
            for(bool emptyColumn = false; x < 8 && !emptyColumn; x++) {
                int xPixel = xt * 8 + x; // Posição X do pixel atual
                emptyColumn = true;      // Assume que a coluna está vazia

                // Verifica todos os pixels na coluna vertical
                for(int y = 0; y < 8 && emptyColumn; ++y) {
                    int yPixel = (yt * 8 + y) * w; // Posição Y do pixel
                    int pixel = rawPixels[xPixel + yPixel] & 255; // Intensidade do pixel
                    if(pixel > 128) {
                        emptyColumn = false; // Encontrou pixel não-vazio
                    }
                }
            }

            // Define largura fixa para espaço
            if(i == 32) {
                x = 4;
            }

            charWidths[i] = x; // Armazena a largura calculada
        }
        //*/
    }

    public void OnRenderFrame(int width, int height) { 
        int screenWidth = width * 240 / height;
        int screenHeight = height * 240 / height;

        // Ativa o shader para uso na renderização
        shader.OnRenderFrame();

        // Vincula a textura para uso na renderização
        texture.OnRenderFrame();

        t.OnRenderFrame();

        // Cria a matriz de visualização (view) a partir da posição e orientação do jogador
        Matrix4 view = Matrix4.Identity;
        //view *= Matrix4.CreateScale(8.0f);
        view *= Matrix4.CreateTranslation(0.0f, (float)(screenHeight - 8.0f), 0.0f);
        view *= Matrix4.CreateTranslation(0.0f, 0.0f, -200.0f);
        shader.SetMatrix4("view", view); // Passa a matriz de visualização para o shader

        // Cria a matriz de projeção em perspectiva a partir do tamanho da janela
        Matrix4 projection = Matrix4.Identity;
        projection *= CreateOrthographicOffCenter(screenWidth, screenHeight);
        shader.SetMatrix4("projection", projection); // Passa a matriz de projeção para o shader
    }

    // Desenha texto com sombra
    public void DrawShadow(string str, int x, int y, int color) {
        t.Init();

        // Desenha texto principal
        Draw(str, x, y, color);

        // Desenha versão escurecida deslocada como sombra
        Draw(str, x + 1, y - 1, color, true);

        t.OnLoad(); // Renderiza os vértices
    }

    // Desenha texto simples
    public void Draw(string str, int x, int y, int color) {
        Draw(str, x, y, color, false);
    }

    // Método principal de desenho de texto
    public void Draw(string str, int x, int y, int color, bool darken) {
        char[] chars = str.ToCharArray();

        // Escurece a cor se necessário (para sombras)
        if(darken) {
            // color = (color & 16579836) >> 2;
            color = (color & 0xFCFCFC) >> 2;
        }

        // t.Init();

        t.Color(color);

        int xo = 0; // Offset horizontal acumulado

        for(int i = 0; i < chars.Length; i++) {
            int ix;
            int iy;

            // Trata códigos de cor (formato &x onde x é hexadecimal)
            if(chars[i] == '&') {
                ix = "0123456789abcdef".IndexOf(chars[i + 1]);
                iy = (ix & 8) * 8; // Bit de intensidade

                // Calcula componentes RGB
                int r = ((ix & 4) >> 2) * 191 + iy;
                int g = ((ix & 2) >> 1) * 191 + iy;
                int b = (ix & 1) * 191 + iy;
                color = r << 16 | g << 8 | b;

                i += 2; // Pula o código de cor

                // Aplica escurecimento se necessário
                if(darken) {
                    // color = (color & 16579836) >> 2;
                    color = (color & 0xFCFCFC) >> 2;
                }

                t.Color(color); // Atualiza cor no renderizador
            }

            float x0 = (float)(x + xo);
            float y0 = (float)y;

            float x1 = (float)(x + xo + 8);
            float y1 = (float)(y + 8);

            // Calcula posição do caractere na textura
            // ix = chars[i] % 16 * 8;
            // iy = chars[i] / 16 * 8;

            // float u0 = (float)ix / 128.0f;
            // float v0 = (float)iy / 128.0f;

            // float u1 = (float)(ix + 8) / 128.0f;
            // float v1 = (float)(iy + 8) / 128.0f;

            float u0 = (float)(chars[i] % 16) / 16.0f;
            float v0 = (16.0f - 1.0f - chars[i] / 16) / 16.0f;
            
            float u1 = u0 + (1.0f / 16.0f);
            float v1 = v0 + (1.0f / 16.0f);

            // Renderiza quad texturizado para o caractere
            t.VertexUV(x0, y0, 0.0f, u0, v0);
            t.VertexUV(x1, y0, 0.0f, u1, v0);
            t.VertexUV(x1, y1, 0.0f, u1, v1);
            t.VertexUV(x0, y1, 0.0f, u0, v1);

            xo += charWidths[chars[i]]; // Avança pelo tamanho do caractere
        }

        // t.OnLoad(); // Renderiza os vértices
    }

    // Método para criar uma matriz de projeção em perspectiva
    public Matrix4 CreateOrthographicOffCenter(int width, int height) {
        float left = 0.0f;
        float right = (float)width;
        float bottom = 0.0f;
        float top = (float)height;

        // Define a distância do plano de corte próximo (depthNear)
        float depthNear = 100.0f;

        // Define a distância do plano de corte distante (depthFar)
        float depthFar = 300.0f;

        return Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, depthNear, depthFar);
    }
}
