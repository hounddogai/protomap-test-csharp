namespace Acme.Workforce.Compliance.TrainingDue.DueSchedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched training due mapping calculation.</summary>
public interface IBatchedTrainingDueMappingStep
{
    string Describe();
    double Adjust(BatchedTrainingDueMapping item, double valueValue);
}

public sealed record TieredBatchedTrainingDueMappingAdjuster(List<double> Bounds, List<double> Rates) : IBatchedTrainingDueMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedTrainingDueMapping item, double valueValue)
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

public sealed record UnitBatchedTrainingDueMappingAdjuster(Dictionary<string, double> Factors) : IBatchedTrainingDueMappingStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(BatchedTrainingDueMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedTrainingDueMappingAdjuster(Registry Flags, string Flag, double Percent) : IBatchedTrainingDueMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedTrainingDueMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedTrainingDueMappingAdjuster(double Step) : IBatchedTrainingDueMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedTrainingDueMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapBatchedTrainingDueMappingAdjuster(double Cap) : IBatchedTrainingDueMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedTrainingDueMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BatchedTrainingDueMappingHoursOffsetAdjuster(double Weight) : IBatchedTrainingDueMappingStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(BatchedTrainingDueMapping item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the batched training due mapping steps in order and keeps a trace of each.</summary>
public sealed class BatchedTrainingDueMappingEngine(List<IBatchedTrainingDueMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedTrainingDueMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedTrainingDueMappingEngine DefaultBatchedTrainingDueMappingEngine(Registry flags)
    {
        return new BatchedTrainingDueMappingEngine(
            new List<IBatchedTrainingDueMappingStep>
            {
                new TieredBatchedTrainingDueMappingAdjuster(
                    new List<double> { 1164.0, 6809.0, 9114.0 },
                    new List<double> { 0.99, 0.82, 1.04 }),
                new UnitBatchedTrainingDueMappingAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.85, ["north"] = 1.07, ["south"] = 1.16 }),
                new FlaggedBatchedTrainingDueMappingAdjuster(flags, "batched-training-due-mapping-strict", 7.1),
                new RoundingBatchedTrainingDueMappingAdjuster(0.25),
                new CapBatchedTrainingDueMappingAdjuster(41893.0),
                new BatchedTrainingDueMappingHoursOffsetAdjuster(1.42),
            },
            new List<string>(),
            3.0,
            50062.7);
    }

    public double Evaluate(BatchedTrainingDueMapping item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedTrainingDueMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
