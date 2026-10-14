namespace Acme.Workforce.Kit.Testing;

using Acme.Workforce.Kit.Clock;

/// <summary>StepClock advances by step seconds every time it is read.</summary>
public sealed class StepClock(int current, int step) : IClock
{
    public int Current { get; set; } = current;
    public int Step { get; set; } = step;

    public int Now()
    {
        var at = Current;
        Current = Current + Step;
        return at;
    }
}
