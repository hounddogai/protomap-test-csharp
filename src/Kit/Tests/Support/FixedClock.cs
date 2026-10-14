namespace Acme.Workforce.Kit.Testing;

using Acme.Workforce.Kit.Clock;

/// <summary>FixedClock always tells the same time.</summary>
public sealed record FixedClock(int At) : IClock
{
    public int Now()
    {
        return At;
    }
}
