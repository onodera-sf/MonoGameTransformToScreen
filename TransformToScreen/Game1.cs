using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TransformToScreen
{
	/// <summary>
	/// ゲームメインクラス
	/// </summary>
	public class Game1 : Game
	{
    /// <summary>
    /// グラフィックデバイス管理クラス
    /// </summary>
    private readonly GraphicsDeviceManager _graphics = null;

    /// <summary>
    /// スプライトのバッチ化クラス
    /// </summary>
    private SpriteBatch _spriteBatch = null;

    /// <summary>
    /// スプライトでテキストを描画するためのフォント
    /// </summary>
    private SpriteFont _font = null;

    /// <summary>
    /// モデル
    /// </summary>
    private Model _model = null;

    /// <summary>
    /// 位置の配列
    /// </summary>
    private Vector3[] _positions = new Vector3[3];

    /// <summary>
    /// カメラの水平回転角度
    /// </summary>
    private float _theta = 0.0f;

    /// <summary>
    /// ビューマトリックス
    /// </summary>
    private Matrix _view = Matrix.Identity;

    /// <summary>
    /// プロジェクションマトリックス
    /// </summary>
    private Matrix _projection = Matrix.Identity;


    /// <summary>
    /// GameMain コンストラクタ
    /// </summary>
    public Game1()
    {
      // グラフィックデバイス管理クラスの作成
      _graphics = new GraphicsDeviceManager(this);

      // ゲームコンテンツのルートディレクトリを設定
      Content.RootDirectory = "Content";

      // マウスカーソルを表示
      IsMouseVisible = true;
    }

    /// <summary>
    /// ゲームが始まる前の初期化処理を行うメソッド
    /// グラフィック以外のデータの読み込み、コンポーネントの初期化を行う
    /// </summary>
    protected override void Initialize()
    {
      // 位置をランダムに設定
      Random rand = new Random();
      for (int i = 0; i < _positions.Length; i++)
      {
        _positions[i] =
            new Vector3((float)(rand.NextDouble() - 0.5) * 10.0f,
                        0.0f,
                        (float)(rand.NextDouble() - 0.5) * 10.0f);
      }

      // ビューマトリックス
      _view = Matrix.CreateLookAt(new Vector3(0.0f, 10.0f, 20.0f),
                                      Vector3.Zero,
                                      Vector3.Up);

      // プロジェクションマトリックス
      _projection = Matrix.CreatePerspectiveFieldOfView(
                  MathHelper.ToRadians(45.0f),
                  (float)GraphicsDevice.Viewport.Width /
                      (float)GraphicsDevice.Viewport.Height,
                  1.0f,
                  100.0f
              );

      // コンポーネントの初期化などを行います
      base.Initialize();
    }

    /// <summary>
    /// ゲームが始まるときに一回だけ呼ばれ
    /// すべてのゲームコンテンツを読み込みます
    /// </summary>
    protected override void LoadContent()
    {
      // テクスチャーを描画するためのスプライトバッチクラスを作成します
      _spriteBatch = new SpriteBatch(GraphicsDevice);

      // フォントをコンテンツパイプラインから読み込む
      _font = Content.Load<SpriteFont>("Font");

      // モデルを作成
      _model = Content.Load<Model>("Model");

      // ライトとプロジェクションはあらかじめ設定しておく
      foreach (ModelMesh mesh in _model.Meshes)
      {
        foreach (BasicEffect effect in mesh.Effects)
        {
          // デフォルトのライト適用
          effect.EnableDefaultLighting();

          // プロジェクションマトリックスをあらかじめ設定
          effect.Projection = _projection;
        }
      }
    }

    /// <summary>
    /// ゲームが終了するときに一回だけ呼ばれ
    /// すべてのゲームコンテンツをアンロードします
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: ContentManager で管理されていないコンテンツを
      //       ここでアンロードしてください
    }

    /// <summary>
    /// 描画以外のデータ更新等の処理を行うメソッド
    /// 主に入力処理、衝突判定などの物理計算、オーディオの再生など
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Update(GameTime gameTime)
    {
      // ゲームパッドの Back ボタン、またはキーボードの Esc キーを押したときにゲームを終了させます
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
      {
        Exit();
      }

      // カメラの水平角度を自動更新
      _theta = (float)gameTime.TotalGameTime.TotalSeconds / 2.0f;

      // 登録された GameComponent を更新する
      base.Update(gameTime);
    }

    /// <summary>
    /// 描画処理を行うメソッド
    /// </summary>
    /// <param name="gameTime">このメソッドが呼ばれたときのゲーム時間</param>
    protected override void Draw(GameTime gameTime)
    {
      // 画面を指定した色でクリアします
      GraphicsDevice.Clear(Color.CornflowerBlue);

      // Zバッファを有効にする
      GraphicsDevice.DepthStencilState = DepthStencilState.Default;

      // ビューマトリックスに回転を合成算出
      Matrix rotatedView = Matrix.CreateRotationY(_theta) * _view;

      for (int i = 0; i < _positions.Length; i++)
      {
        foreach (ModelMesh mesh in _model.Meshes)
        {
          foreach (BasicEffect effect in mesh.Effects)
          {
            // ビューマトリックス
            effect.View = rotatedView;

            // モデルの位置設定
            effect.World = Matrix.CreateTranslation(_positions[i]);
          }

          // モデルを描画
          mesh.Draw();
        }
      }

      // ビューポート取得
      Viewport viewport = GraphicsDevice.Viewport;

      // スプライトの描画準備
      _spriteBatch.Begin();

      // 各モデルの位置にテキストを描画
      for (int i = 0; i < _positions.Length; i++)
      {
        // ３次元座標からスクリーンの座標算出
        Vector3 v3 = viewport.Project(_positions[i],
                                      _projection,
                                      rotatedView,
                                      Matrix.Identity);
        Vector2 screenPosition = new Vector2(v3.X, v3.Y);

        // テキスト描画
        _spriteBatch.DrawString(_font, "Model " + (i + 1).ToString(),
            screenPosition, Color.White);
      }

      // スプライトの一括描画
      _spriteBatch.End();

      // 登録された DrawableGameComponent を描画する
      base.Draw(gameTime);
    }
  }
}
