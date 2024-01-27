/*
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects.Components;
using Robust.Shared.Map;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;

namespace Content.Server.Anomaly.Effects;

/// <summary>
/// This handles <see cref="StealthAnomalyComponent"/> and the events from <seealso cref="AnomalySystem"/>
/// </summary>
public sealed class StealthAnomalySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<StealthAnomalyComponent, AnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<StealthAnomalyComponent, AnomalySupercriticalEvent>(OnSupercritical);
    }

    private void OnPulse(EntityUid uid, StealthAnomalyComponent component, ref AnomalyPulseEvent args)
    {
        var xform = Transform(uid);
        var ignitionRadius = component.MaximumStealthingRadius * args.Stability;
        IgniteNearby(uid, xform.Coordinates, args.Severity, ignitionRadius);
    }

    private void OnSupercritical(EntityUid uid, StealthAnomalyComponent component, ref AnomalySupercriticalEvent args)
    {
        var xform = Transform(uid);
        IgniteNearby(uid, xform.Coordinates, 1, component.MaximumStealthingRadius * 2);
    }

    public void IgniteNearby(EntityUid uid, EntityCoordinates coordinates, float severity, float radius)
    {
        var stealthing = new HashSet<Entity<StealthComponent>>();
        _lookup.GetEntitiesInRange(coordinates, radius, stealthing);

        foreach (var flammable in stealthing)
        {
            //if(!SharedStealthSystem.GetVisibility(EntityUid, StealthComponent?)) {

            var ent = flammable.Owner;
            var stackAmount = 1 + (int) (severity / 0.15f);
            _flammable.OnStealthGetState(uid, !(SharedStealthSystem.GetVisibility(uid, StealthComponent?)), severety);
            _flammable.Ignite(ent, uid, flammable);
            //}
            //else continue;
        }
    }
}
*/
