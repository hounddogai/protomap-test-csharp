namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered position policy calculation.</summary>
public interface ITieredPositionPolicyStep
{
    string Describe();
    double Adjust(TieredPositionPolicy item, double valueValue);
}

public sealed record TieredPositionPolicyCapacityOffsetAdjuster(double Weight) : ITieredPositionPolicyStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(TieredPositionPolicy item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record FlaggedTieredPositionPolicyAdjuster(Registry Flags, string Flag, double Percent) : ITieredPositionPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPositionPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredPositionPolicyAdjuster(double Cap) : ITieredPositionPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPositionPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTieredPositionPolicyAdjuster(List<double> Bounds, List<double> Rates) : ITieredPositionPolicyStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPositionPolicy item, double valueValue)
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

public sealed record TitleTieredPositionPolicyAdjuster(Dictionary<string, double> Factors) : ITieredPositionPolicyStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TieredPositionPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTieredPositionPolicyAdjuster(double Minimum) : ITieredPositionPolicyStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredPositionPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered position policy steps in order and keeps a trace of each.</summary>
public sealed class TieredPositionPolicyEngine(List<ITieredPositionPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPositionPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPositionPolicyEngine DefaultTieredPositionPolicyEngine(Registry flags)
    {
        return new TieredPositionPolicyEngine(
            new List<ITieredPositionPolicyStep>
            {
                new TieredPositionPolicyCapacityOffsetAdjuster(1.08),
                new FlaggedTieredPositionPolicyAdjuster(flags, "tiered-position-policy-strict", -9.1),
                new CapTieredPositionPolicyAdjuster(22163.0),
                new TieredTieredPositionPolicyAdjuster(
                    new List<double> { 3389.0, 8232.0, 9265.0 },
                    new List<double> { 1.06, 0.84, 1.13 }),
                new TitleTieredPositionPolicyAdjuster(new Dictionary<string, double> { ["online"] = 0.79 }),
                new MinimumTieredPositionPolicyAdjuster(5.4),
            },
            new List<string>(),
            0.2,
            67860.0);
    }

    public double Price(TieredPositionPolicy item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPositionPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
