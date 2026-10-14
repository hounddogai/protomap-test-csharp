namespace Acme.Workforce.Payrollops.PayRun.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy pay run assignment calculation.</summary>
public interface ILegacyPayRunAssignmentModifier
{
    string Describe();
    double Adjust(LegacyPayRunAssignment item, double valueValue);
}

public sealed record NameLegacyPayRunAssignmentAdjuster(Dictionary<string, double> Factors) : ILegacyPayRunAssignmentModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacyPayRunAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLegacyPayRunAssignmentAdjuster(double Cap) : ILegacyPayRunAssignmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPayRunAssignment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLegacyPayRunAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPayRunAssignmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPayRunAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyPayRunAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ILegacyPayRunAssignmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyPayRunAssignment item, double valueValue)
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

public sealed record MinimumLegacyPayRunAssignmentAdjuster(double Minimum) : ILegacyPayRunAssignmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayRunAssignment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy pay run assignment steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayRunAssignmentEngine(List<ILegacyPayRunAssignmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayRunAssignmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayRunAssignmentEngine DefaultLegacyPayRunAssignmentEngine(Registry flags)
    {
        return new LegacyPayRunAssignmentEngine(
            new List<ILegacyPayRunAssignmentModifier>
            {
                new NameLegacyPayRunAssignmentAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.38, ["south"] = 0.88, ["west"] = 0.95 }),
                new CapLegacyPayRunAssignmentAdjuster(24514.0),
                new FlaggedLegacyPayRunAssignmentAdjuster(flags, "legacy-pay-run-assignment-legacy", 6.8),
                new TieredLegacyPayRunAssignmentAdjuster(
                    new List<double> { 7520.0, 9162.0, 9402.0, 9973.0 },
                    new List<double> { 1.19, 0.82, 1.07, 0.86 }),
                new MinimumLegacyPayRunAssignmentAdjuster(44.9),
            },
            new List<string>(),
            4.4,
            31843.2);
    }

    public double Compute(LegacyPayRunAssignment item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayRunAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
