using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Cuffs.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Content.Shared.Genetics.Components;
using Content.Shared.Mutations;

namespace Content.Shared.Genetics
{
    public abstract partial class SharedGeneSystem : EntitySystem // I HAVE NO FUCKING IDEA WHAT IM FUCKING DOING, HELP ME
    {

        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
        [Dependency] private readonly AlertsSystem _alerts = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
        [Dependency] private readonly SharedInteractionSystem _interaction = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly UseDelaySystem _delay = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GeneinjectorComponent, MeleeHitEvent>(OnMeleeInject);
            SubscribeLocalEvent<GeneinjectorComponent, AddInjectDoAfterEvent>(OnAddDNADoAfter);
        }
        private void OnMeleeInject(EntityUid uid, GeneinjectorComponent component, MeleeHitEvent args)
        {
            if (!args.HitEntities.Any())
                return;

            TryInjecting(args.User, args.HitEntities.First(), uid, component);
            args.Handled = true;
        }
        public bool TryInjecting(EntityUid user, EntityUid target, EntityUid item, GeneinjectorComponent? injectorComponent = null, CuffableComponent? cuffable = null)
        {
            if (!Resolve(item, ref injectorComponent) || !Resolve(target, ref cuffable, false)) //use the fartass cuffable cuz it works
                return false;

            var injectTime = injectorComponent.InjectTime;

            if (HasComp<StunnedComponent>(target))
                injectTime = MathF.Max(0.1f, injectTime - injectorComponent.StunBonus);

            var doAfterEventArgs = new DoAfterArgs(EntityManager, user, injectTime, new AddInjectDoAfterEvent(), item, target, item)
            {
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
                BreakOnDamage = true,
                NeedHand = true,
                DistanceThreshold = 1f
            };

            if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
                return true;

            _popup.PopupEntity(Loc.GetString("geneinjector-component-start-injecting-observer",
                    ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                target, Filter.Pvs(target, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);

            if (target == user)
            {
                _popup.PopupClient(Loc.GetString("geneinjector-component-target-self"), user, user);
            }
            else
            {
                _popup.PopupClient(Loc.GetString("geneinjector-component-start-injecting-target-message",
                    ("targetName", Identity.Name(target, EntityManager, user))), user, user);
                _popup.PopupEntity(Loc.GetString("geneinjector-component-start-injecting-by-other-message",
                    ("otherName", Identity.Name(user, EntityManager, target))), target, target);
            }
            return true;
        }

        private void OnAddDNADoAfter(EntityUid uid, GeneinjectorComponent component, AddInjectDoAfterEvent args)
        {
            var user = args.Args.User;

            if (!TryComp<CuffableComponent>(args.Args.Target, out var cuffable))
                return;

            var target = args.Args.Target.Value;

            if (args.Handled)
                return;
            args.Handled = true;

            if (!args.Cancelled && TryAddMutation(target, user, uid, cuffable))
            {

                _popup.PopupEntity(Loc.GetString("geneinjector-component-inject-observer-success-message",
                        ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                    target, Filter.Pvs(target, entityManager: EntityManager)
                        .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);


                if (target == user)
                {
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-self-success-message"), user, user);
                    _adminLog.Add(LogType.Action, LogImpact.Medium,
                        $"{ToPrettyString(user):player} has injected himself");

                    EntityManager.DeleteEntity(uid);
                }
                else
                {
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-other-success-message",
                        ("otherName", Identity.Name(target, EntityManager, user))), user, user);
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-by-other-success-message",
                        ("otherName", Identity.Name(user, EntityManager, target))), target, target);
                    _adminLog.Add(LogType.Action, LogImpact.Medium,
                        $"{ToPrettyString(user):player} has injected {ToPrettyString(target):player}");

                    EntityManager.DeleteEntity(uid);
                }
            }
            else
            {
                if (target == user)
                {
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-interrupt-self-message"), user, user);
                }
                else
                {

                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-interrupt-message",
                        ("targetName", Identity.Name(target, EntityManager, user))), user, user);
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-interrupt-other-message",
                        ("otherName", Identity.Name(user, EntityManager, target))), target, target);
                }
            }
        }

        public bool TryAddMutation(EntityUid target, EntityUid user, EntityUid item, CuffableComponent? component = null, GeneinjectorComponent? gene = null, InjectionPresetComponent? inject = null, MutationComponent? mutation = null)
        {

            if (!_interaction.InRangeUnobstructed(item, target))
                return false;

            EnsureComp<MutationComponent>(target);

            if (TryComp<MutationComponent>(target, out var mutations))
            {
                if (TryComp<InjectionPresetComponent>(item, out var injectpreset))
                {
                    if ((mutations != null) && (injectpreset != null)) //to anyone whos like trying to make mutations, im so sorry.
                    {
                        if (injectpreset.AcidVomit) //its 3 am, im fucking tired, im just gonna hardcode it, im so sorry taydeo, I have dishonored the john space bloodline.
                        {
                            mutations.AcidVomit = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Anchorable)
                        {
                            mutations.Anchorable = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.BackwardsAccent)
                        {
                            mutations.BackwardsAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.BigSize)
                        {
                            mutations.BigSize = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Blindness)
                        {
                            mutations.Blindness = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.BloodVomit)
                        {
                            mutations.BloodVomit = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.BlueLight)
                        {
                            mutations.BlueLight = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.BZFarter)
                        {
                            mutations.BZFarter = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Cancer)
                        {
                            mutations.Cancer = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Clumsy)
                        {
                            mutations.Clumsy = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.EMPer)
                        {
                            mutations.EMPer = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Explode)
                        {
                            mutations.Explode = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.EyeDamage)
                        {
                            mutations.EyeDamage = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.FireSkin)
                        {
                            mutations.FireSkin = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.High)
                        {
                            mutations.High = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Item)
                        {
                            mutations.Item = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Leprosy)
                        {
                            mutations.Leprosy = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Light)
                        {
                            mutations.Light = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.LubeVomit)
                        {
                            mutations.LubeVomit = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.MobsterAccent)
                        {
                            mutations.MobsterAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.OhioAccent)
                        {
                            mutations.OhioAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.OkayAccent)
                        {
                            mutations.OkayAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.OWOAccent)
                        {
                            mutations.OWOAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.PlasmaFarter)
                        {
                            mutations.PlasmaFarter = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.PressureImmune)
                        {
                            mutations.PressureImmune = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Prickmode)
                        {
                            mutations.Prickmode = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.RadiationImmune)
                        {
                            mutations.RadiationImmune = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.RedLight)
                        {
                            mutations.RedLight = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.RGBLight)
                        {
                            mutations.RGBLight = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.ScrambleAccent)
                        {
                            mutations.ScrambleAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.SelfHeal)
                        {
                            mutations.SelfHeal = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Slippy)
                        {
                            mutations.Slippy = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.SmallSize)
                        {
                            mutations.SmallSize = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.StutterAccent)
                        {
                            mutations.StutterAccent = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.TempImmune)
                        {
                            mutations.TempImmune = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.TinySize)
                        {
                            mutations.TinySize = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.TritFarter)
                        {
                            mutations.TritFarter = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Twitch)
                        {
                            mutations.Twitch = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.UberSlippy)
                        {
                            mutations.UberSlippy = true;
                            ++mutations.Amount;
                        }
                        if (injectpreset.Vomit)
                        {
                            mutations.Vomit = true;
                            ++mutations.Amount;
                        }
                    }
                }
            }
            return true;
        }

        public IReadOnlyList<EntityUid> GetAllCuffs(CuffableComponent component)
        {
            return component.Container.ContainedEntities;
        }

        [Serializable, NetSerializable]
        private sealed partial class AddInjectDoAfterEvent : SimpleDoAfterEvent
        {
        }
    }
}

