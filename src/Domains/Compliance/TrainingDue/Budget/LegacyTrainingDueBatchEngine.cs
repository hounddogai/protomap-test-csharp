namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy training due batch calculation.</summary>
public interface ILegacyTrainingDueBatchModifier
{
    string Describe();
    double Adjust(LegacyTrainingDueBatch item, double valueValue);
}

public sealed record FlaggedLegacyTrainingDueBatchAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTrainingDueBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTrainingDueBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyTrainingDueBatchAdjuster(double Cap) : ILegacyTrainingDueBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTrainingDueBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyTrainingDueBatchAdjuster(double Minimum) : ILegacyTrainingDueBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTrainingDueBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyTrainingDueBatchAdjuster(double Step) : ILegacyTrainingDueBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTrainingDueBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyTrainingDueBatchAdjuster(List<double> Bounds, List<double> Rates) : ILegacyTrainingDueBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyTrainingDueBatch item, double valueValue)
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

/// <summary>Runs the legacy training due batch steps in order and keeps a trace of each.</summary>
public sealed class LegacyTrainingDueBatchEngine(List<ILegacyTrainingDueBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTrainingDueBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTrainingDueBatchEngine DefaultLegacyTrainingDueBatchEngine(Registry flags)
    {
        return new LegacyTrainingDueBatchEngine(
            new List<ILegacyTrainingDueBatchModifier>
            {
                new FlaggedLegacyTrainingDueBatchAdjuster(flags, "legacy-training-due-batch-pending", 8.0),
                new CapLegacyTrainingDueBatchAdjuster(33392.0),
                new MinimumLegacyTrainingDueBatchAdjuster(29.6),
                new RoundingLegacyTrainingDueBatchAdjuster(0.1),
                new TieredLegacyTrainingDueBatchAdjuster(
                    new List<double> { 232.0, 3507.0, 8305.0 },
                    new List<double> { 0.91, 1.09, 1.07 }),
            },
            new List<string>(),
            4.0,
            16483.2);
    }

    public double Evaluate(LegacyTrainingDueBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTrainingDueBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
