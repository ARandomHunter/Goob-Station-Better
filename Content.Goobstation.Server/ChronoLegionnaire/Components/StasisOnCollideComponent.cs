using Content.Goobstation.Server.ChronoLegionnaire.Systems;

namespace Content.Goobstation.Server.ChronoLegionnaire.Components;

/// <summary>
/// Marks projectiles that will apply stasis on hit
/// </summary>
[RegisterComponent, Access(typeof(StasisOnCollideSystem))]
public sealed partial class StasisOnCollideComponent : Component
{
    [DataField("stasisTime")]
    public TimeSpan StasisTime = TimeSpan.FromSeconds(60);

    [DataField("fixture")]
    public string FixtureID = "projectile";
}
