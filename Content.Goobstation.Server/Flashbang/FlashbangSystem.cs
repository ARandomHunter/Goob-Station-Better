using Content.Goobstation.Shared.Flashbang;
using Content.Server.Flash;
using Content.Server.Stunnable;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Tag;

namespace Content.Goobstation.Server.Flashbang;

public sealed class FlashbangSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlashbangComponent, AreaFlashEvent>(OnFlash);
        SubscribeLocalEvent<FlashSoundSuppressionComponent, InventoryRelayedEvent<GetFlashbangedEvent>>(
            OnInventoryFlashbanged);
        SubscribeLocalEvent<FlashSoundSuppressionComponent, GetFlashbangedEvent>(OnFlashbanged);
        SubscribeLocalEvent<FlashSoundSuppressionComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<FlashSoundSuppressionComponent> ent, ref ExaminedEvent args)
    {
        var range = ent.Comp.ProtectionRange;
        var message = range > 0
            ? Loc.GetString("flash-sound-suppression-examine", ("range", range))
            : Loc.GetString("flash-sound-suppression-fully-examine");

        args.PushMarkup(message);
    }

    private void OnFlashbanged(Entity<FlashSoundSuppressionComponent> ent, ref GetFlashbangedEvent args)
    {
        args.ProtectionRange = MathF.Min(args.ProtectionRange, ent.Comp.ProtectionRange);
    }

    private void OnInventoryFlashbanged(Entity<FlashSoundSuppressionComponent> ent,
        ref InventoryRelayedEvent<GetFlashbangedEvent> args)
    {
        args.Args.ProtectionRange = MathF.Min(args.Args.ProtectionRange, ent.Comp.ProtectionRange);
    }

    private void OnFlash(Entity<FlashbangComponent> ent, ref AreaFlashEvent args)
    {
        var comp = ent.Comp;

        if (comp is { KnockdownTime: <= 0, StunTime: <= 0 })
            return;

        var protectionRange = args.Range;

        if (!_tag.HasTag(ent, FlashSystem.IgnoreResistancesTag))
        {
            var ev = new GetFlashbangedEvent(args.Range);
            RaiseLocalEvent(args.Target, ev);

            protectionRange = ev.ProtectionRange;
        }

        if (protectionRange <= 0f)
            return;

        var distance = MathF.Max(0f, args.Distance);

        if (distance > protectionRange)
            return;

        var ratio = distance / protectionRange;

        var knockdownTime = float.Lerp(comp.KnockdownTime, 0f, ratio);
        if (knockdownTime > 0f)
            _stun.TryKnockdown(args.Target, TimeSpan.FromSeconds(knockdownTime), true);

        var stunTime = float.Lerp(comp.StunTime, 0f, ratio);
        if (stunTime > 0f)
            _stun.TryStun(args.Target, TimeSpan.FromSeconds(stunTime), true);
    }
}
