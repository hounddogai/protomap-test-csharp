namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default audit checkpoint calculation.</summary>
public interface IDefaultAuditCheckpointFactor
{
    string Describe();
    double Adjust(DefaultAuditCheckpoint item, double valueValue);
}

public sealed record MinimumDefaultAuditCheckpointAdjuster(double Minimum) : IDefaultAuditCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAuditCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultAuditCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IDefaultAuditCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultAuditCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultAuditCheckpointAdjuster(double Cap) : IDefaultAuditCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAuditCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDefaultAuditCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAuditCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAuditCheckpoint item, double valueValue)
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

/// <summary>Runs the default audit checkpoint steps in order and keeps a trace of each.</summary>
public sealed class DefaultAuditCheckpointEngine(List<IDefaultAuditCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAuditCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAuditCheckpointEngine DefaultDefaultAuditCheckpointEngine(Registry flags)
    {
        return new DefaultAuditCheckpointEngine(
            new List<IDefaultAuditCheckpointFactor>
            {
                new MinimumDefaultAuditCheckpointAdjuster(26.9),
                new FlaggedDefaultAuditCheckpointAdjuster(flags, "default-audit-checkpoint-rolling", -6.8),
                new CapDefaultAuditCheckpointAdjuster(952.0),
                new TieredDefaultAuditCheckpointAdjuster(
                    new List<double> { 2168.0, 4696.0 },
                    new List<double> { 0.9, 0.88 }),
            },
            new List<string>(),
            4.9,
            4321.2);
    }

    public double Run(DefaultAuditCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAuditCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
