using OpenTK.Graphics.OpenGL4; // Fornece acesso às funções do OpenGL 4
using OpenTK.Mathematics; // Fornece funcionalidades matemáticas, como vetores e matrizes
using OpenTK.Windowing.Common; // Fornece funcionalidades comuns, como eventos de janela
using OpenTK.Windowing.Desktop; // Fornece funcionalidades para criar e gerenciar janelas
using OpenTK.Windowing.GraphicsLibraryFramework; // Fornece acesso ao GLFW para manipulação de entrada

namespace Minecraft;

// Classe principal do jogo, que herda de GameWindow
public class RubyDung : GameWindow {
    public static readonly bool FULLSCREEN_MODE = false;

    private int width;
    private int height;

    private Shader shader; // Instância do shader que será usado para renderizar a geometria
    private Texture texture; // Instância da textura que será aplicada na geometria
    private Level level; // Instância da classe Level para gerenciar os blocos do mundo
    private LevelRenderer levelRenderer; // Instância da classe Level para gerenciar os blocos do mundo
    private Player player; // Instância da classe Player para gerenciar a câmera e a perspectiva

    private DrawGUI drawGUI;
    private Crosshair crosshair;    
    private Font font;

    private Raycast raycast;

    private string fpsString = "";

    // Construtor da classe RubyDung
    public RubyDung(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) {
        // Centraliza a janela ao iniciar
        CenterWindow();

        width = ClientSize.X;
        height = ClientSize.Y;
    }

    // Método chamado quando a janela é carregada
    protected override void OnLoad() {
        base.OnLoad();

        // Define a cor de fundo da tela (RGBA)
        GL.ClearColor(0.5f, 0.8f, 1.0f, 0.0f);

        // Cria uma instância do shader, carregando os arquivos de vertex e fragment shader
        shader = new Shader("src/shaders/texture_vertex.glsl", "src/shaders/texture_fragment.glsl");

        // Cria uma instância da textura, carregando a imagem do arquivo especificado
        texture = new Texture("src/textures/terrain.png");

        // Cria um nível com 256x64x256 blocos
        level = new Level(256, 64, 256);
        levelRenderer = new LevelRenderer(shader, level);
        levelRenderer.OnLoad();

        // wireframe
        // GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

        // Inicializa a instância do Player para gerenciar a câmera e a perspectiva
        player = new Player(level);
        player.OnLoad(this);

        // Habilita o teste de profundidade (Depth Test) para renderização 3D
        GL.Enable(EnableCap.DepthTest);

        // Habilita o culling de faces (Cull Face) para otimizar a renderização
        GL.Enable(EnableCap.CullFace);

        drawGUI = new DrawGUI(level);
        crosshair = new Crosshair();
        font = new Font();

        font.DrawShadow("0.0.2a", 2, -2, 0xFFFFFF);
        font.DrawShadow("666 fps", 2, -12, 0xFFFFFF);

        font.DrawShadow("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 2, -32, 0xFFFFFF);
        font.DrawShadow("abcdefghijklmnopqrstuvwxyz", 2, -42, 0xFFFFFF);
        font.DrawShadow("0123456789", 2, -52, 0xFFFFFF);
        font.DrawShadow("!@#$%&*()_+", 2, -62, 0xFFFFFF);
        font.DrawShadow("Eu gosto de abacate", 2, -72, 0xFFFFFF);

        raycast = new Raycast(level, levelRenderer, player, drawGUI);
    }

    // Método chamado a cada frame para atualizar a lógica do jogo
    protected override void OnUpdateFrame(FrameEventArgs args) {
        base.OnUpdateFrame(args);

        // Verifica se a tecla ESC foi pressionada para fechar a janela
        if(KeyboardState.IsKeyPressed(Keys.Escape)) {
            Close();
        }

        // Atualiza a lógica do jogador (movimentação da câmera, etc.)
        player.OnUpdateFrame(this);

        raycast.OnUpdateFrame(this);

        if(KeyboardState.IsKeyPressed(Keys.Enter)) {
            level.Save();
        }

        drawGUI.OnUpdateFrame(this);

        int frames = 0;

        fpsString = $"{frames} fps";
        
        // font.DrawShadow("0.0.2a", 2, -2, 16777215);
        // font.DrawShadow(fpsString, 2, -12, 16777215);

        // font.DrawShadow("0.0.2a", 2, -2, 0xFFFFFF);
        // font.DrawShadow(fpsString, 2, -12, 0xFFFFFF);
    }

    // Método chamado a cada frame para renderizar o jogo
    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);

        // Limpa o buffer de cor e o buffer de profundidade com a cor definida em OnLoad
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Ativa o shader para uso na renderização
        shader.OnRenderFrame();

        // Vincula a textura para uso na renderização
        texture.OnRenderFrame();

        // Renderiza o chunk (conjunto de tiles/blocos)
        levelRenderer.OnRenderFrame();

        // Cria a matriz de visualização (view) a partir da posição e orientação do jogador
        Matrix4 view = Matrix4.Identity;
        view *= player.LookAt();
        shader.SetMatrix4("view", view); // Passa a matriz de visualização para o shader

        // Cria a matriz de projeção em perspectiva a partir do tamanho da janela
        Matrix4 projection = Matrix4.Identity;
        projection *= player.CreatePerspectiveFieldOfView(width, height);
        shader.SetMatrix4("projection", projection); // Passa a matriz de projeção para o shader

        drawGUI.OnRenderFrame(width, height);
        crosshair.OnRenderFrame(width, height);
        font.OnRenderFrame(width, height);
        
        raycast.OnRenderFrame(width, height);

        // Troca os buffers para exibir o que foi renderizado
        SwapBuffers();
    }

    // Método chamado quando o tamanho da janela é alterado
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e) {
        base.OnFramebufferResize(e);

        // Ajusta o viewport para o novo tamanho da janela
        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

        width = ClientSize.X;
        height = ClientSize.Y;
    }

    protected override void OnUnload() {
        base.OnUnload();
        
        level.Save();
    }
}
