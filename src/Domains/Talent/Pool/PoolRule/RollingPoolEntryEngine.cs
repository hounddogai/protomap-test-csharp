namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling pool entry calculation.</summary>
public interface IRollingPoolEntryStep
{
    string Describe();
    double Adjust(RollingPoolEntry item, double valueValue);
}

public sealed record RoundingRollingPoolEntryAdjuster(double Step) : IRollingPoolEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPoolEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRollingPoolEntryAdjuster(Registry Flags, string Flag, double Percent) : IRollingPoolEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPoolEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingPoolEntryAdjuster(double Cap) : IRollingPoolEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPoolEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling pool entry steps in order and keeps a trace of each.</summary>
public sealed class RollingPoolEntryEngine(List<IRollingPoolEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPoolEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPoolEntryEngine DefaultRollingPoolEntryEngine(Registry flags)
    {
        return new RollingPoolEntryEngine(
            new List<IRollingPoolEntryStep>
            {
                new RoundingRollingPoolEntryAdjuster(5.0),
                new FlaggedRollingPoolEntryAdjuster(flags, "rolling-pool-entry-capped", -8.4),
                new CapRollingPoolEntryAdjuster(10309.0),
            },
            new List<string>(),
            1.0,
            6132.9);
    }

    public double Compute(RollingPoolEntry item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RollingPoolEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
