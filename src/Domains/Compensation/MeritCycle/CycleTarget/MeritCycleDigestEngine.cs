namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle digest calculation.</summary>
public interface IMeritCycleDigestStep
{
    string Describe();
    double Adjust(MeritCycleDigest item, double valueValue);
}

public sealed record PercentMeritCycleDigestAdjuster(double Percent) : IMeritCycleDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedMeritCycleDigestAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMeritCycleDigestAdjuster(double Cap) : IMeritCycleDigestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MeritCycleDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredMeritCycleDigestAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleDigestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleDigest item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the merit cycle digest steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleDigestEngine(List<IMeritCycleDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleDigestEngine DefaultMeritCycleDigestEngine(Registry flags)
    {
        return new MeritCycleDigestEngine(
            new List<IMeritCycleDigestStep>
            {
                new PercentMeritCycleDigestAdjuster(-12.8),
                new FlaggedMeritCycleDigestAdjuster(flags, "merit-cycle-digest-tiered", 9.6),
                new CapMeritCycleDigestAdjuster(44516.0),
                new TieredMeritCycleDigestAdjuster(
                    new List<double> { 8394.0, 9552.0 },
                    new List<double> { 1.12, 1.1 }),
            },
            new List<string>(),
            1.0,
            87901.9);
    }

    public double Price(MeritCycleDigest item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
