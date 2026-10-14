namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the training due slot calculation.</summary>
public interface ITrainingDueSlotFactor
{
    string Describe();
    double Adjust(TrainingDueSlot item, double valueValue);
}

public sealed record TrainingDueSlotAttemptsOffsetAdjuster(double Weight) : ITrainingDueSlotFactor
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(TrainingDueSlot item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record FlaggedTrainingDueSlotAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueSlotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceTrainingDueSlotAdjuster(Dictionary<string, double> Factors) : ITrainingDueSlotFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TrainingDueSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTrainingDueSlotAdjuster(double Percent) : ITrainingDueSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTrainingDueSlotAdjuster(double Cap) : ITrainingDueSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTrainingDueSlotAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueSlotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueSlot item, double valueValue)
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

/// <summary>Runs the training due slot steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueSlotEngine(List<ITrainingDueSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueSlotEngine DefaultTrainingDueSlotEngine(Registry flags)
    {
        return new TrainingDueSlotEngine(
            new List<ITrainingDueSlotFactor>
            {
                new TrainingDueSlotAttemptsOffsetAdjuster(0.28),
                new FlaggedTrainingDueSlotAdjuster(flags, "training-due-slot-scheduled", 4.2),
                new SourceTrainingDueSlotAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.95, ["retail"] = 1.14 }),
                new PercentTrainingDueSlotAdjuster(-1.8),
                new CapTrainingDueSlotAdjuster(44803.0),
                new TieredTrainingDueSlotAdjuster(
                    new List<double> { 2167.0, 5178.0, 6584.0, 9690.0 },
                    new List<double> { 1.08, 1.08, 0.8, 0.9 }),
            },
            new List<string>(),
            2.5,
            8070.7);
    }

    public double Evaluate(TrainingDueSlot item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
