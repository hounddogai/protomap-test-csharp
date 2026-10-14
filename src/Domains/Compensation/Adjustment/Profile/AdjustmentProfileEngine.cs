namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment profile calculation.</summary>
public interface IAdjustmentProfileAdjuster
{
    string Describe();
    double Adjust(AdjustmentProfile item, double valueValue);
}

public sealed record PercentAdjustmentProfileAdjuster(double Percent) : IAdjustmentProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdjustmentProfileWindowDaysOffsetAdjuster(double Weight) : IAdjustmentProfileAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(AdjustmentProfile item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record CapAdjustmentProfileAdjuster(double Cap) : IAdjustmentProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdjustmentProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamAdjustmentProfileAdjuster(Dictionary<string, double> Factors) : IAdjustmentProfileAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AdjustmentProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdjustmentProfileAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentProfileAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentProfile item, double valueValue)
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

public sealed record FlaggedAdjustmentProfileAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment profile steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentProfileEngine(List<IAdjustmentProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentProfileEngine DefaultAdjustmentProfileEngine(Registry flags)
    {
        return new AdjustmentProfileEngine(
            new List<IAdjustmentProfileAdjuster>
            {
                new PercentAdjustmentProfileAdjuster(20.3),
                new AdjustmentProfileWindowDaysOffsetAdjuster(0.77),
                new CapAdjustmentProfileAdjuster(20320.0),
                new TeamAdjustmentProfileAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.33, ["north"] = 0.85 }),
                new TieredAdjustmentProfileAdjuster(
                    new List<double> { 2644.0, 6160.0, 8114.0 },
                    new List<double> { 0.97, 1.1, 0.94 }),
                new FlaggedAdjustmentProfileAdjuster(flags, "adjustment-profile-pending", -3.4),
            },
            new List<string>(),
            2.3,
            94031.9);
    }

    public double Evaluate(AdjustmentProfile item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
