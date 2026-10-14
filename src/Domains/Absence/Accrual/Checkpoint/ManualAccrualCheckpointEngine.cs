namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual accrual checkpoint calculation.</summary>
public interface IManualAccrualCheckpointStep
{
    string Describe();
    double Adjust(ManualAccrualCheckpoint item, double valueValue);
}

public sealed record ManualAccrualCheckpointSequenceOffsetAdjuster(double Weight) : IManualAccrualCheckpointStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(ManualAccrualCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedManualAccrualCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IManualAccrualCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAccrualCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualAccrualCheckpointAdjuster(double Step) : IManualAccrualCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAccrualCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredManualAccrualCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IManualAccrualCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualAccrualCheckpoint item, double valueValue)
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

public sealed record CapManualAccrualCheckpointAdjuster(double Cap) : IManualAccrualCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAccrualCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryManualAccrualCheckpointAdjuster(Dictionary<string, double> Factors) : IManualAccrualCheckpointStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ManualAccrualCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual accrual checkpoint steps in order and keeps a trace of each.</summary>
public sealed class ManualAccrualCheckpointEngine(List<IManualAccrualCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAccrualCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAccrualCheckpointEngine DefaultManualAccrualCheckpointEngine(Registry flags)
    {
        return new ManualAccrualCheckpointEngine(
            new List<IManualAccrualCheckpointStep>
            {
                new ManualAccrualCheckpointSequenceOffsetAdjuster(0.93),
                new FlaggedManualAccrualCheckpointAdjuster(flags, "manual-accrual-checkpoint-manual", -0.4),
                new RoundingManualAccrualCheckpointAdjuster(0.05),
                new TieredManualAccrualCheckpointAdjuster(
                    new List<double> { 2077.0, 3365.0, 6421.0, 8140.0 },
                    new List<double> { 0.94, 1.03, 0.96, 1.06 }),
                new CapManualAccrualCheckpointAdjuster(26075.0),
                new CategoryManualAccrualCheckpointAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.98, ["east"] = 1.3 }),
            },
            new List<string>(),
            4.0,
            94208.4);
    }

    public double Price(ManualAccrualCheckpoint item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAccrualCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
