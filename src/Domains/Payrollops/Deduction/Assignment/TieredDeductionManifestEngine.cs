namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered deduction manifest calculation.</summary>
public interface ITieredDeductionManifestModifier
{
    string Describe();
    double Adjust(TieredDeductionManifest item, double valueValue);
}

public sealed record TeamTieredDeductionManifestAdjuster(Dictionary<string, double> Factors) : ITieredDeductionManifestModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(TieredDeductionManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDeductionManifestFloorValueOffsetAdjuster(double Weight) : ITieredDeductionManifestModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(TieredDeductionManifest item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record RoundingTieredDeductionManifestAdjuster(double Step) : ITieredDeductionManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredDeductionManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredDeductionManifestAdjuster(double Minimum) : ITieredDeductionManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredDeductionManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTieredDeductionManifestAdjuster(Registry Flags, string Flag, double Percent) : ITieredDeductionManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredDeductionManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered deduction manifest steps in order and keeps a trace of each.</summary>
public sealed class TieredDeductionManifestEngine(List<ITieredDeductionManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredDeductionManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredDeductionManifestEngine DefaultTieredDeductionManifestEngine(Registry flags)
    {
        return new TieredDeductionManifestEngine(
            new List<ITieredDeductionManifestModifier>
            {
                new TeamTieredDeductionManifestAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.17, ["retail"] = 1.32, ["online"] = 0.97 }),
                new TieredDeductionManifestFloorValueOffsetAdjuster(1.88),
                new RoundingTieredDeductionManifestAdjuster(0.1),
                new MinimumTieredDeductionManifestAdjuster(2.2),
                new FlaggedTieredDeductionManifestAdjuster(flags, "tiered-deduction-manifest-scheduled", -2.8),
            },
            new List<string>(),
            4.5,
            29495.8);
    }

    public double Price(TieredDeductionManifest item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredDeductionManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
