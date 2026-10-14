namespace Acme.Workforce.Kit.Testing;

using Acme.Workforce.Kit.Clock;

/// <summary>OffsetClock runs ahead of (or behind) another clock by a fixed number of seconds.</summary>
public sealed record OffsetClock(IClock Base, int Offset) : IClock
{
    public int Now()
    {
        return Base.Now() + Offset;
    }
}
