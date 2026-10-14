namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted assignment audit calculation.</summary>
public interface IWeightedAssignmentAuditAdjuster
{
    string Describe();
    double Adjust(WeightedAssignmentAudit item, double valueValue);
}

public sealed record WeightedAssignmentAuditRatingOffsetAdjuster(double Weight) : IWeightedAssignmentAuditAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(WeightedAssignmentAudit item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record MinimumWeightedAssignmentAuditAdjuster(double Minimum) : IWeightedAssignmentAuditAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAssignmentAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingWeightedAssignmentAuditAdjuster(double Step) : IWeightedAssignmentAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAssignmentAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWeightedAssignmentAuditAdjuster(List<double> Bounds, List<double> Rates) : IWeightedAssignmentAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedAssignmentAudit item, double valueValue)
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

public sealed record CapWeightedAssignmentAuditAdjuster(double Cap) : IWeightedAssignmentAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedAssignmentAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedWeightedAssignmentAuditAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAssignmentAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAssignmentAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted assignment audit steps in order and keeps a trace of each.</summary>
public sealed class WeightedAssignmentAuditEngine(List<IWeightedAssignmentAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAssignmentAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAssignmentAuditEngine DefaultWeightedAssignmentAuditEngine(Registry flags)
    {
        return new WeightedAssignmentAuditEngine(
            new List<IWeightedAssignmentAuditAdjuster>
            {
                new WeightedAssignmentAuditRatingOffsetAdjuster(1.6),
                new MinimumWeightedAssignmentAuditAdjuster(7.3),
                new RoundingWeightedAssignmentAuditAdjuster(0.25),
                new TieredWeightedAssignmentAuditAdjuster(
                    new List<double> { 844.0, 1938.0, 3920.0, 7174.0 },
                    new List<double> { 0.94, 1.09, 1.18, 0.83 }),
                new CapWeightedAssignmentAuditAdjuster(13917.0),
                new FlaggedWeightedAssignmentAuditAdjuster(flags, "weighted-assignment-audit-fallback", 0.9),
            },
            new List<string>(),
            1.6,
            91846.6);
    }

    public double Evaluate(WeightedAssignmentAudit item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAssignmentAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
