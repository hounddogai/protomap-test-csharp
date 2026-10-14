namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy assignment baseline calculation.</summary>
public interface ILegacyAssignmentBaselineModifier
{
    string Describe();
    double Adjust(LegacyAssignmentBaseline item, double valueValue);
}

public sealed record PercentLegacyAssignmentBaselineAdjuster(double Percent) : ILegacyAssignmentBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyAssignmentBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyAssignmentBaselineAdjuster(double Minimum) : ILegacyAssignmentBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyAssignmentBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyAssignmentBaselineAdjuster(Registry Flags, string Flag, double Percent) : ILegacyAssignmentBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyAssignmentBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyAssignmentBaselineAdjuster(List<double> Bounds, List<double> Rates) : ILegacyAssignmentBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyAssignmentBaseline item, double valueValue)
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

/// <summary>Runs the legacy assignment baseline steps in order and keeps a trace of each.</summary>
public sealed class LegacyAssignmentBaselineEngine(List<ILegacyAssignmentBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAssignmentBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAssignmentBaselineEngine DefaultLegacyAssignmentBaselineEngine(Registry flags)
    {
        return new LegacyAssignmentBaselineEngine(
            new List<ILegacyAssignmentBaselineModifier>
            {
                new PercentLegacyAssignmentBaselineAdjuster(11.6),
                new MinimumLegacyAssignmentBaselineAdjuster(6.3),
                new FlaggedLegacyAssignmentBaselineAdjuster(flags, "legacy-assignment-baseline-global", 9.9),
                new TieredLegacyAssignmentBaselineAdjuster(
                    new List<double> { 878.0, 3795.0, 6874.0 },
                    new List<double> { 0.99, 1.01, 0.87 }),
            },
            new List<string>(),
            4.2,
            29840.1);
    }

    public double Price(LegacyAssignmentBaseline item)
    {
        var current = item.NetLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LegacyAssignmentBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
