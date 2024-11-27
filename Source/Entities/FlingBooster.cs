using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste;

// Token: 0x020005BD RID: 1469
[CustomEntity("Tetrahelper/FlingBooster")]
public class FlingBooster : Entity
{
    // Token: 0x04001DE9 RID: 7657
    private const float RespawnTime = 1f;

    // Token: 0x04001DEA RID: 7658
    public static ParticleType P_Burst;

    // Token: 0x04001DEB RID: 7659
    public static ParticleType P_BurstRed;

    // Token: 0x04001DEC RID: 7660
    public static ParticleType P_Appear;

    // Token: 0x04001DED RID: 7661
    public static ParticleType P_RedAppear;

    // Token: 0x04001DEE RID: 7662
    public static readonly Vector2 playerOffset;

    // Token: 0x04001DF5 RID: 7669
    private readonly DashListener dashListener;

    // Token: 0x04001DF4 RID: 7668
    private readonly Coroutine dashRoutine;

    // Token: 0x04001DFB RID: 7675
    private readonly SoundSource loopingSfx;

    // Token: 0x04001DEF RID: 7663
    private readonly Sprite sprite;

    // Token: 0x04001DF1 RID: 7665
    private readonly Wiggler wiggler;

    // Token: 0x04001DF2 RID: 7666
    private BloomPoint bloom;

    // Token: 0x04001DF9 RID: 7673
    private float cannotUseTimer;

    // Token: 0x04001DFC RID: 7676
    public bool Ch9HubFlingBooster;

    // Token: 0x04001DFD RID: 7677
    public bool Ch9HubTransition;

    public int Direction;
    public int DirectionDeg;

    // Token: 0x04001DF3 RID: 7667
    private VertexLight light;

    // Token: 0x04001DF0 RID: 7664
    private Entity outline;

    // Token: 0x04001DF6 RID: 7670
    private ParticleType particleType;

    // Token: 0x04001DFA RID: 7674
    private bool red;

    // Token: 0x04001DF8 RID: 7672
    private float respawnTimer;
    public int Speed;
    public int SprDirectionDeg;

    // Token: 0x060024D4 RID: 9428 RVA: 0x000C8280 File Offset: 0x000C6480
    // Note: this type is marked as 'beforefieldinit'.
    static FlingBooster()
    {
        playerOffset = new Vector2(0f, -2f);
    }

    // Token: 0x060024C5 RID: 9413 RVA: 0x000C79F8 File Offset: 0x000C5BF8
    public FlingBooster(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Depth = -8500;
        Collider = new Circle(10f, 0f, 2f);

        Add(sprite = GFX.SpriteBank.Create("FlingBooster"));
        Add(new PlayerCollider(OnPlayer));
        Add(light = new VertexLight(Color.White, 1f, 16, 32));
        Add(bloom = new BloomPoint(0.1f, 16f));
        Add(wiggler = Wiggler.Create(0.5f, 4f, delegate(float f) { sprite.Scale = Vector2.One * (1f + f * 0.25f); }));
        Add(dashRoutine = new Coroutine(false));
        Add(dashListener = new DashListener());
        Add(new MirrorReflection());
        Add(loopingSfx = new SoundSource());
        dashListener.OnDash = OnPlayerDashed;
        particleType = Booster.P_Burst;


        Speed = data.Int("Speed", 500);
        switch (data.Attr("Direction", "R"))
        {
            case "R":
                Direction = 0;
                break;
            case "DR":
                Direction = 1;
                break;
            case "D":
                Direction = 2;
                break;
            case "DL":
                Direction = 3;
                break;
            case "L":
                Direction = 4;
                break;
            case "UL":
                Direction = 5;
                break;
            case "U":
                Direction = 6;
                break;
            case "UR":
                Direction = 7;
                break;
            default:
                Direction = 0;
                break;
        }

        DirectionDeg = Direction * 45;
        SprDirectionDeg = Direction / 2 * 90;
    }

    public bool BoostingPlayer { get; private set; }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        var image = new Image(GFX.Game["objects/FlingBooster/outline"]);
        image.CenterOrigin();
        image.Color = Color.White * 0.75f;
        outline = new Entity(Position);
        outline.Depth = 8999;
        outline.Visible = false;
        outline.Add(image);
        outline.Add(new MirrorReflection());
        scene.Add(outline);


        sprite.Play(Direction % 2 == 0 ? "idle" : "upidle");

        sprite.Rotation = Calc.ToRad(SprDirectionDeg);


        //sprite.Visible = false;
    }

    // Token: 0x060024C8 RID: 9416 RVA: 0x000C7C30 File Offset: 0x000C5E30
    public void Appear()
    {
        Audio.Play("event:/game/04_cliffside/greenBooster_reappear", Position);
        //this.sprite.Play("appear", false, false);
        wiggler.Start();
        Visible = true;
        AppearParticles();
    }

    // Token: 0x060024C9 RID: 9417 RVA: 0x000C7C88 File Offset: 0x000C5E88
    private void AppearParticles()
    {
        var particlesBG = SceneAs<Level>().ParticlesBG;
        for (var i = 0; i < 360; i += 30)
            particlesBG.Emit(red ? Booster.P_RedAppear : Booster.P_Appear, 1, Center, Vector2.One * 2f,
                i * 0.017453292f);
    }

    // Token: 0x060024CA RID: 9418 RVA: 0x000C7CEC File Offset: 0x000C5EEC
    private void OnPlayer(Player player)
    {
        if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
        {
            cannotUseTimer = 0.45f;

            Audio.Play("event:/game/04_cliffside/greenBooster_enter", Position);
            wiggler.Start();
            sprite.Play(Direction % 2 == 0 ? "Boost" : "upBoost");

            //this.sprite.FlipX = (player.Facing == Facings.Left);
            Add(new Coroutine(BoostRoutine(player)));
        }
    }

    // Token: 0x060024CB RID: 9419 RVA: 0x000C7D98 File Offset: 0x000C5F98


    // Token: 0x060024CC RID: 9420 RVA: 0x000C7F08 File Offset: 0x000C6108
    private IEnumerator BoostRoutine(Player player)
    {
        for (var i = 0; i < 10; i++)
        {
            player.StateMachine.State = 6;
            player.Speed = Vector2.Zero;
            var vector = Calc.Approach(player.ExactPosition, Position - new Vector2(0, -8), 100f * Engine.DeltaTime);
            player.MoveToX(vector.X);
            player.MoveToY(vector.Y);
            yield return null;
        }


        Audio.Play("event:/game/03_resort/forcefield_bump", Position);


        player.Speed = new Vector2(400, 0).Rotate(Calc.ToRad(DirectionDeg));
        //Console.WriteLine(new Vector2(400, 0).Rotate(Calc.ToRad(DirectionDeg)));
        //Console.WriteLine("deg: " + DirectionDeg);
        //Console.WriteLine("---");


        //player.Speed.Y += -120f;


        Celeste.Freeze(0.1f);
        SlashFx.Burst(Center, player.Speed.Angle());
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
        SceneAs<Level>().DirectionalShake(player.Speed, 0.15f);
        SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
        SceneAs<Level>().Particles.Emit(Bumper.P_Launch, 10, Center * 12f, Vector2.One * 3f, player.Speed.Angle());

        player.RefillDash();
        player.RefillStamina();
    }

    // Token: 0x060024CD RID: 9421 RVA: 0x000C7F25 File Offset: 0x000C6125
    public void OnPlayerDashed(Vector2 direction)
    {
        if (BoostingPlayer) BoostingPlayer = false;
    }

    // Token: 0x060024CE RID: 9422 RVA: 0x000C7F38 File Offset: 0x000C6138
    public void PlayerReleased()
    {
        Audio.Play(
            red ? "event:/game/05_mirror_temple/redFlingBooster_end" : "event:/game/04_cliffside/greenFlingBooster_end",
            sprite.RenderPosition);
        //this.sprite.Play("pop", false, false);
        cannotUseTimer = 0f;
        respawnTimer = 1f;
        BoostingPlayer = false;
        wiggler.Stop();
        loopingSfx.Stop();
    }

    // Token: 0x060024CF RID: 9423 RVA: 0x000C7FB1 File Offset: 0x000C61B1
    public void PlayerDied()
    {
        if (BoostingPlayer)
        {
            PlayerReleased();
            dashRoutine.Active = false;
            Tag = 0;
        }
    }

    // Token: 0x060024D0 RID: 9424 RVA: 0x000C7FD4 File Offset: 0x000C61D4
    public void Respawn()
    {
        Audio.Play(
            red
                ? "event:/game/05_mirror_temple/redFlingBooster_reappear"
                : "event:/game/04_cliffside/greenFlingBooster_reappear", Position);
        sprite.Position = Vector2.Zero;
        sprite.Play("idle", true);
        wiggler.Start();
        sprite.Visible = true;
        outline.Visible = false;
        AppearParticles();
    }

    // Token: 0x060024D1 RID: 9425 RVA: 0x000C804C File Offset: 0x000C624C
    public override void Update()
    {
        base.Update();
        if (cannotUseTimer > 0f) cannotUseTimer -= Engine.DeltaTime;
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f) Respawn();
        }

        if (!dashRoutine.Active && respawnTimer <= 0f)
        {
            var target = Vector2.Zero;
            var entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && CollideCheck(entity)) target = entity.Center + playerOffset - Position;
            sprite.Position = Calc.Approach(sprite.Position, target, 80f * Engine.DeltaTime);
        }

        if (sprite.CurrentAnimationID == "inside" && !BoostingPlayer && !CollideCheck<Player>()) sprite.Play("loop");
    }

    // Token: 0x060024D2 RID: 9426 RVA: 0x000C816C File Offset: 0x000C636C
    public override void Render()
    {
        var position = sprite.Position;
        sprite.Position = position.Floor();
        if (sprite.CurrentAnimationID != "pop" && sprite.Visible) sprite.DrawOutline();
        base.Render();
        sprite.Position = position;
    }

    // Token: 0x060024D3 RID: 9427 RVA: 0x000C81D8 File Offset: 0x000C63D8
    public override void Removed(Scene scene)
    {
        if (Ch9HubTransition)
        {
            var level = scene as Level;
            foreach (var backdrop in level.Background.GetEach<Backdrop>("bright"))
            {
                backdrop.ForceVisible = false;
                backdrop.FadeAlphaMultiplier = 1f;
            }

            level.Bloom.Base = AreaData.Get(level).BloomBase + 0.25f;
            level.Session.BloomBaseAdd = 0.25f;
        }

        base.Removed(scene);
    }
}