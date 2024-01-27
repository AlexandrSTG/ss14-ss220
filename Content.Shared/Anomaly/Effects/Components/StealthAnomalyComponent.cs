namespace Content.Shared.Anomaly.Effects.Components;

[RegisterComponent]
public sealed partial class StealthAnomalyComponent : Component
{
    /// <summary>
    /// The maximum distance from which you can be stealthed by the anomaly.
    /// </summary>
    [DataField("maximumStealthingRadius")]
    public float MaximumStealthingRadius = 5f;
}
