using Content.Shared.Mutations;
using Content.Shared.Jittering;
using Content.Shared.Light.Components;
using Content.Server.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.StatusEffect;
using Content.Shared.Fluids;
using Content.Server.Stunnable;
using Content.Shared.Chemistry.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Chemistry.Components;
using Robust.Shared.Random;
using Content.Shared.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Clumsy;
using Content.Server.Atmos.Components;
using Content.Server.Temperature.Components;
using Content.Server.Radiation.Components;
using Content.Server.Speech.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Shared.Body.Systems;
using Content.Server.Emp;
using Robust.Server.GameObjects;
using Content.Shared.Slippery;
using Content.Shared.Construction.Components;
using Content.Shared.Item;
using Robust.Shared.GameObjects;
using System.Numerics;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;


public sealed partial class MututationSystem : EntitySystem
{
    //star wars intro or something i just stole john stations joke lmfao pwned fatso
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedPointLightSystem _pointlight = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedBodySystem _limbs = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SlipperySystem _slipperySystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly BlindableSystem _blindable = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;

    public override void Update(float frameTime) //erm... we gotta update the stupid bool checks for all the effects to keep em updated. aint called shitgenetics for no reason.
    {
        base.Update(frameTime);

        foreach (var comp in EntityManager.EntityQuery<MutationComponent>())
        {
            var uid = comp.Owner; // go fuck yourself what is this SHIT

            CycleEffects(uid, comp);


            if (comp.Twitch) //thugging this shit out yandev style
            {
                Twitching(uid, comp);
            }
            if (comp.Light)
            {
                Light(uid, comp);
            }
            if (comp.RedLight)
            {
                RedLight(uid, comp);
            }
            if (comp.BlueLight)
            {
                BlueLight(uid, comp);
            }
            if (comp.RGBLight)
            {
                RGBLight(uid, comp);
            }
            if (comp.Clumsy)
            {
                Clumsy(uid, comp);
            }
            if (comp.TempImmune)
            {
                TempImmune(uid, comp);
            }
            if (comp.OkayAccent)
            {
                OkayAccent(uid, comp);
            }
            if (comp.TempImmune)
            {
                TempImmune(uid, comp);
            }
            if (comp.PressureImmune)
            {
                PressureImmune(uid, comp);
            }
            if (comp.RadiationImmune)
            {
                RadImmune(uid, comp);
            }
            if (comp.Prickmode)
            {
                PrickAccent(uid, comp);
            }
            if (comp.OWOAccent)
            {
                OWOAccent(uid, comp);
            }
            if (comp.StutterAccent)
            {
                StutterAccent(uid, comp);
            }
            if (comp.ScrambleAccent)
            {
                ScrambleAccent(uid, comp);
            }
            if (comp.OhioAccent)
            {
                OhioAccent(uid, comp);
            }
            if (comp.BackwardsAccent)
            {
                BackwardsAccent(uid, comp);
            }
            if (comp.MobsterAccent)
            {
                MobsterAccent(uid, comp);
            }
            if (comp.Item)
            {
                Item(uid, comp);
            }
            if (comp.Anchorable)
            {
                Anchorable(uid, comp);
            }
            if (comp.BigSize)
            {
                BigSize(uid, comp);
                if (comp.TinySize) //basically, i dont want you to have 3 different sizes, so it'll just remove the others on big since its kinda the worst ver
                {
                    comp.TinySize = false;
                    comp.Amount = comp.Amount - 1;
                }
                if (comp.SmallSize)
                {
                    comp.SmallSize = false;
                    comp.Amount = comp.Amount - 1;
                }
            }
            if (comp.SmallSize)
            {
                SmallSize(uid, comp);
                if (comp.TinySize)
                {
                    comp.TinySize = false;
                    comp.Amount = comp.Amount - 1;
                }
            }
            if (comp.TinySize) //basically, cuz of small and big, you'll need to make sure you only get this mutation to have it. Aka dont get big and ruin it
            {
                TinySize(uid, comp);
            }


            if (comp.Cancel)
            {
                EntityManager.RemoveComponent<MutationComponent>(uid); // delete self after tick
            }
        }
    }

    public void CycleEffects(EntityUid uid, MutationComponent comp) // this fuckin bullshit is for effects that i skibidi my sigma not per every tick cuz thats fucked
    {

        comp.MutationUpdateTimer += 1;
        if (comp.MutationUpdateTimer >= comp.MutationUpdateCooldown)
        {
            comp.MutationUpdateTimer = 0;

            if (comp.Amount > 6) //todo: change the 6 to a cvar. If i forget, its probably on purpose cuz i hate THIS FUCKING SERVER MAN
            {
                GeneticPain(uid, comp);
            }

            if (comp.Vomit) //fent warriors please rise up
            {
                Vomit(uid, comp);
            }
            if (comp.BloodVomit)
            {
                VomitBlood(uid, comp);
            }
            if (comp.AcidVomit)
            {
                VomitAcid(uid, comp);
            }
            if (comp.PlasmaFarter)
            {
                PlasmaFart(uid, comp);
            }
            if (comp.TritFarter)
            {
                TritFart(uid, comp);
            }
            if (comp.BZFarter)
            {
                BZFart(uid, comp);
            }
            if (comp.FireSkin)
            {
                FireSkin(uid, comp);
            }
            if (comp.Prickmode)
            {
                PrickSay(uid, comp);
            }
            if (comp.SelfHeal)
            {
                SelfHeal(uid, comp);
            }
            if (comp.Cancer)
            {
                Cancer(uid, comp);
            }
            if (comp.Leprosy)
            {
                Leprosy(uid, comp);
            }
            if (comp.High)
            {
                High(uid, comp);
            }
            if (comp.Slippy)
            {
                Slippy(uid, comp);
            }
            if (comp.EMPer)
            {
                EMPer(uid, comp);
            }
            if (comp.LubeVomit)
            {
                VomitLube(uid, comp);
            }
            if (comp.Explode)
            {
                Explode(uid, comp);
            }
            if (comp.UberSlippy)
            {
                UberSlippy(uid, comp);
            }
            if (comp.EyeDamage)
            {
                EyeDamage(uid, comp);
            }
            if (comp.Blindness)
            {
                Blindness(uid, comp);
            }
        }
    }

    #region Visual
    private void Twitching(EntityUid uid, MutationComponent comp)
    {
        _jitter.DoJitter(uid, TimeSpan.FromSeconds(.5f), true, amplitude: 5, frequency: 10);
    }

    private void Light(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        if (comp.Cancel) _pointlight.RemoveLightDeferred(uid);
    }

    private void RedLight(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        _pointlight.SetColor(uid, new Color(255, 0, 0));
        if (comp.Cancel) _pointlight.RemoveLightDeferred(uid);
    }

    private void BlueLight(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        _pointlight.SetColor(uid, new Color(0, 0, 255));
        if (comp.Cancel) _pointlight.RemoveLightDeferred(uid);
    }

    private void RGBLight(EntityUid uid, MutationComponent comp)
    {
        _pointlight.EnsureLight(uid);
        EnsureComp<RgbLightControllerComponent>(uid);
        if (comp.Cancel)
        {
            _pointlight.RemoveLightDeferred(uid);
            RemComp<RgbLightControllerComponent>(uid);
        }
    }
    private void BigSize(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ScaleVisualsComponent>(uid);
        var scale = 1.5f;
        var oldScale = Vector2.One;
        var appearance = _entityManager.System<AppearanceSystem>();
        var appearanceComponent = _entityManager.EnsureComponent<AppearanceComponent>(uid);
        appearance.SetData(uid, ScaleVisuals.Scale, scale * oldScale, appearanceComponent);
        if (comp.Cancel)
        {
            appearance.SetData(uid, ScaleVisuals.Scale, 1f * oldScale, appearanceComponent);
        }
    }
    private void SmallSize(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ScaleVisualsComponent>(uid);
        var scale = 0.85f;
        var oldScale = Vector2.One;
        var appearance = _entityManager.System<AppearanceSystem>();
        var appearanceComponent = _entityManager.EnsureComponent<AppearanceComponent>(uid);
        appearance.SetData(uid, ScaleVisuals.Scale, scale * oldScale, appearanceComponent);
        if (comp.Cancel)
        {
            appearance.SetData(uid, ScaleVisuals.Scale, 1f * oldScale, appearanceComponent);
        }
    }
    private void TinySize(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ScaleVisualsComponent>(uid);
        var scale = 0.5f;
        var oldScale = Vector2.One;
        var appearance = _entityManager.System<AppearanceSystem>();
        var appearanceComponent = _entityManager.EnsureComponent<AppearanceComponent>(uid);
        appearance.SetData(uid, ScaleVisuals.Scale, scale * oldScale, appearanceComponent);
        if (comp.Cancel)
        {
            appearance.SetData(uid, ScaleVisuals.Scale, 1f * oldScale, appearanceComponent);
        }
    }
    #endregion

    #region Emitting Stuff

    private void Vomit(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 3);

        if (random == 2)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 10f;
            solution.AddReagent("Vomit", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void VomitBlood(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 5);

        if (random == 4)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 10f;
            solution.AddReagent("Blood", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void VomitAcid(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 25f;
            solution.AddReagent("PolytrinicAcid", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void PlasmaFart(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            var tilepos = _xform.GetGridOrMapTilePosition(uid, Transform(uid));
            var enumerator = _atmos.GetAdjacentTileMixtures(Transform(uid).GridUid!.Value, tilepos, false, false);
            while (enumerator.MoveNext(out var mix))
            {
                mix.AdjustMoles(Gas.Plasma, 10f);
                _popup.PopupEntity(Loc.GetString("gas-fart", ("person", Identity.Entity(uid, EntityManager))), uid);
            }
        }
    }

    private void TritFart(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            var tilepos = _xform.GetGridOrMapTilePosition(uid, Transform(uid));
            var enumerator = _atmos.GetAdjacentTileMixtures(Transform(uid).GridUid!.Value, tilepos, false, false);
            while (enumerator.MoveNext(out var mix))
            {
                mix.AdjustMoles(Gas.Tritium, 5f);
                _popup.PopupEntity(Loc.GetString("gas-fart", ("person", Identity.Entity(uid, EntityManager))), uid);
            }
        }
    }

    private void BZFart(EntityUid uid, MutationComponent comp) // could be peak if you get a bunch of monkeys in a single room with this
    {
        var random = (int) _rand.Next(1, 7);

        if (random == 6)
        {
            var tilepos = _xform.GetGridOrMapTilePosition(uid, Transform(uid));
            var enumerator = _atmos.GetAdjacentTileMixtures(Transform(uid).GridUid!.Value, tilepos, false, false);
            while (enumerator.MoveNext(out var mix))
            {
                mix.AdjustMoles(Gas.BZ, 25f);
                _popup.PopupEntity(Loc.GetString("gas-fart", ("person", Identity.Entity(uid, EntityManager))), uid);
            }
        }
    }

    private void EMPer(EntityUid uid, MutationComponent comp) // fuck you ipcs x2
    {
        var random = (int) _rand.Next(1, 12);

        if (random == 11)
        {
            var pos = _transform.GetMapCoordinates(uid);
            var power = 5f; //hardcoded again, dont fucking care, smell you later nerd
            _emp.EmpPulse(pos, power, 5000f, power);
        }
    }

    private void VomitLube(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 7);

        if (random == 6)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1.5f), true, 0.5f, 0.5f, status);

            var solution = new Solution();

            var vomitAmount = 15f;
            solution.AddReagent("SpaceLube", vomitAmount);

            _puddle.TrySplashSpillAt(uid, Transform(uid).Coordinates, solution, out _);

            _popup.PopupEntity(Loc.GetString("disease-vomit", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }

    private void Explode(EntityUid uid, MutationComponent comp) //kaboom
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            _explosionSystem.QueueExplosion(
                    (EntityUid) uid,
                    typeId: "Default",
                    totalIntensity: 2,
                    slope: (0.5f),
                    maxTileIntensity: (7)); //if you complain about it being hardcoded, i dont give a shit
        }
    }

    #endregion

    #region Body Stuff
    private void Clumsy(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ClumsyComponent>(uid);
        if (comp.Cancel) RemComp<ClumsyComponent>(uid);

    }

    private void FireSkin(EntityUid uid, MutationComponent comp) //i hope an ipc gets this and fucking DIES
    {
        var random = (int) _rand.Next(1, 3);

        if (random == 2)
        {
            _flammable.AdjustFireStacks(uid, 2.5f, ignite: true);
            _popup.PopupEntity(Loc.GetString("burst-flames", ("person", Identity.Entity(uid, EntityManager))), uid);

        }
    }
    private void TempImmune(EntityUid uid, MutationComponent comp)
    {
        if (TryComp<TemperatureComponent>(uid, out var temp))
        {
            temp.ColdDamageThreshold = -420f;
            temp.HeatDamageThreshold = 19841984f; //im sure this works :clueless:
            if (comp.Cancel)
            {
                temp.ColdDamageThreshold = 260f;
                temp.HeatDamageThreshold = 360f;
            }
        }
    }

    private void PressureImmune(EntityUid uid, MutationComponent comp)
    {
        if (TryComp<BarotraumaComponent>(uid, out var baro))
        {
            baro.HasImmunity = true;
            if (comp.Cancel)
            {
                baro.HasImmunity = false;
            }
        }
    }

    private void RadImmune(EntityUid uid, MutationComponent comp)
    {
        RemComp<RadiationReceiverComponent>(uid);
        if (comp.Cancel) EnsureComp<RadiationReceiverComponent>(uid);
    }

    private void SelfHeal(EntityUid uid, MutationComponent comp) // "This is better than I thought! JACKKKKKKKKPOOOOOTTTTTTT!!"
    {
        var solution = new Solution();
        solution.AddReagent("DoctorsDelight", 3f);
        if (_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            _solution.TryAddSolution(targetSolution.Value, solution);
    }
    private void Cancer(EntityUid uid, MutationComponent comp) //take damage, cough, walter white breaking bad
    {
        var random = (int) _rand.Next(1, 5);

        if (random == 4)
        {
            _chat.TryEmoteWithChat(uid, "Cough", ChatTransmitRange.HideChat, ignoreActionBlocker: true); //hardcoded cuz fuck you, bring back lrp funkymins

            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(1f), true, 0.5f, 0.5f, status);

            var prot = (ProtoId<DamageGroupPrototype>) "Toxin";
            var dmgtype = _proto.Index(prot);
            _damage.TryChangeDamage(uid, new DamageSpecifier(dmgtype, 5f), true);
        }
    }

    private void Leprosy(EntityUid uid, MutationComponent comp) //I don't know how to actually do shitmed stuff or how it works, so im just gonna make em really slow a bunch to simulate it
    {
        var random = (int) _rand.Next(1, 2); //I reccomend adding limb damage to this but i aint gonna do it. Fuck you. Lol. Lmao even.

        if (random == 1)
        {
            if (TryComp<StatusEffectsComponent>(uid, out var status))
                _stun.TrySlowdown(uid, TimeSpan.FromSeconds(3f), true, 0.25f, 0.25f, status);

            _chat.TryEmoteWithChat(uid, "Scream", ChatTransmitRange.HideChat, ignoreActionBlocker: true);

            var prot = (ProtoId<DamageGroupPrototype>) "Brute";
            var dmgtype = _proto.Index(prot);
            _damage.TryChangeDamage(uid, new DamageSpecifier(dmgtype, 2f), true);
            _popup.PopupEntity(Loc.GetString("leprosy-pain", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }
    private void High(EntityUid uid, MutationComponent comp) // "I'm sorry taydeo. Its just that lrp is so beautiful right now."
    {
        var solution = new Solution();
        solution.AddReagent("Happiness", 3f);
        if (_solution.TryGetInjectableSolution(uid, out var targetSolution, out var _))
            _solution.TryAddSolution(targetSolution.Value, solution);
    }
    private void Slippy(EntityUid uid, MutationComponent comp) //give this to cap for a good time
    {
        var random = (int) _rand.Next(1, 3);

        if (random == 2)
        {
            var hadSlipComponent = EnsureComp(uid, out SlipperyComponent slipComponent);
            if (!hadSlipComponent)
            {
                slipComponent.SuperSlippery = true;
                slipComponent.ParalyzeTime = 0.5f;
                slipComponent.LaunchForwardsMultiplier = 2;
            }

            _slipperySystem.TrySlip(uid, slipComponent, uid, requiresContact: false);
            if (!hadSlipComponent)
            {
                RemComp(uid, slipComponent);
            }
        }
    }
    private void UberSlippy(EntityUid uid, MutationComponent comp) //give this to cap for an even gooder time
    {
        var random = (int) _rand.Next(1, 6);

        if (random == 5)
        {
            var hadSlipComponent = EnsureComp(uid, out SlipperyComponent slipComponent);
            if (!hadSlipComponent)
            {
                slipComponent.SuperSlippery = true;
                slipComponent.ParalyzeTime = 4;
                slipComponent.LaunchForwardsMultiplier = 15;
            }

            _slipperySystem.TrySlip(uid, slipComponent, uid, requiresContact: false);
            if (!hadSlipComponent)
            {
                RemComp(uid, slipComponent);
            }
        }
    }
    private void Item(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ItemComponent>(uid);
        if (comp.Cancel) RemComp<ItemComponent>(uid);
    }
    private void Anchorable(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<AnchorableComponent>(uid);
        if (comp.Cancel)
        {
            _transform.Unanchor(uid);
            RemComp<AnchorableComponent>(uid);
        }
    }

    private void EyeDamage(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 5);

        if (random == 4)
        {
            if (!TryComp<BlindableComponent>(uid, out var blindable) || blindable.IsBlind)
                return;
            _blindable.AdjustEyeDamage((uid, blindable), 1);

            _popup.PopupEntity(Loc.GetString("eye-pain", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }
    private void Blindness(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 5);

        if (random == 4)
        {
            if (!TryComp<BlindableComponent>(uid, out var blindable) || blindable.IsBlind)
                return;
            var timeSpan = TimeSpan.FromSeconds(5f);
            _statusEffect.TryAddStatusEffect(uid, TemporaryBlindnessSystem.BlindingStatusEffect, timeSpan, false, TemporaryBlindnessSystem.BlindingStatusEffect);
        }
    }
    #endregion

    #region Accents

    private void OkayAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<OkayAccentComponent>(uid);
        if (comp.Cancel) RemComp<OkayAccentComponent>(uid);
    }

    private void PrickAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<PrickAccentComponent>(uid);
        if (comp.Cancel) RemComp<PrickAccentComponent>(uid);
    }
    private void PrickSay(EntityUid uid, MutationComponent comp)
    {

        var random = (int) _rand.Next(1, 10);

        if (random == 8)
        {
            _chat.TrySendInGameICMessage(uid, Loc.GetString("mutation-coolit"), InGameICChatType.Speak, false);
        }
        if (random == 9)
        {
            _chat.TrySendInGameICMessage(uid, Loc.GetString("mutation-sayit"), InGameICChatType.Speak, false);
        }
    }
    private void OWOAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<OwOAccentComponent>(uid);
        if (comp.Cancel) RemComp<OwOAccentComponent>(uid);
    }
    private void StutterAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<StutteringAccentComponent>(uid);
        if (comp.Cancel) RemComp<StutteringAccentComponent>(uid);
    }
    private void OhioAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<OhioAccentComponent>(uid);
        if (comp.Cancel) RemComp<OhioAccentComponent>(uid); // a momentary lapse of reason i promise
    }
    private void ScrambleAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<ScrambledAccentComponent>(uid);
        if (comp.Cancel) RemComp<ScrambledAccentComponent>(uid);
    }
    private void BackwardsAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<BackwardsAccentComponent>(uid);
        if (comp.Cancel) RemComp<BackwardsAccentComponent>(uid);
    }
    private void MobsterAccent(EntityUid uid, MutationComponent comp)
    {
        EnsureComp<MobsterAccentComponent>(uid);
        if (comp.Cancel) RemComp<MobsterAccentComponent>(uid);
    }

    #endregion

    private void GeneticPain(EntityUid uid, MutationComponent comp)
    {
        var random = (int) _rand.Next(1, 4);

        if (random == 3)
        {
            var prot = (ProtoId<DamageGroupPrototype>) "Genetic";
            var dmgtype = _proto.Index(prot);
            _damage.TryChangeDamage(uid, new DamageSpecifier(dmgtype, 5f), true);
            _popup.PopupEntity(Loc.GetString("genetic-pain", ("person", Identity.Entity(uid, EntityManager))), uid);
        }
    }
}
