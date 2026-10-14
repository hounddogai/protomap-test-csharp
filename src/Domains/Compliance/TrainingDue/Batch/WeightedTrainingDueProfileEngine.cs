namespace Acme.Workforce.Compliance.TrainingDue.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted training due profile calculation.</summary>
public interface IWeightedTrainingDueProfileModifier
{
    string Describe();
    double Adjust(WeightedTrainingDueProfile item, double valueValue);
}

public sealed record TieredWeightedTrainingDueProfileAdjuster(List<double> Bounds, List<double> Rates) : IWeightedTrainingDueProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedTrainingDueProfile item, double valueValue)
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

public sealed record TitleWeightedTrainingDueProfileAdjuster(Dictionary<string, double> Factors) : IWeightedTrainingDueProfileModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WeightedTrainingDueProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWeightedTrainingDueProfileAdjuster(Registry Flags, string Flag, double Percent) : IWeightedTrainingDueProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedTrainingDueProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapWeightedTrainingDueProfileAdjuster(double Cap) : IWeightedTrainingDueProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedTrainingDueProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedTrainingDueProfileAdjuster(double Percent) : IWeightedTrainingDueProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedTrainingDueProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted training due profile steps in order and keeps a trace of each.</summary>
public sealed class WeightedTrainingDueProfileEngine(List<IWeightedTrainingDueProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedTrainingDueProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedTrainingDueProfileEngine DefaultWeightedTrainingDueProfileEngine(Registry flags)
    {
        return new WeightedTrainingDueProfileEngine(
            new List<IWeightedTrainingDueProfileModifier>
            {
                new TieredWeightedTrainingDueProfileAdjuster(
                    new List<double> { 177.0, 3362.0 },
                    new List<double> { 1.09, 0.85 }),
                new TitleWeightedTrainingDueProfileAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.18, ["online"] = 0.78, ["north"] = 0.87 }),
                new FlaggedWeightedTrainingDueProfileAdjuster(flags, "weighted-training-due-profile-scheduled", -7.2),
                new CapWeightedTrainingDueProfileAdjuster(10421.0),
                new PercentWeightedTrainingDueProfileAdjuster(24.5),
            },
            new List<string>(),
            4.2,
            56680.8);
    }

    public double Price(WeightedTrainingDueProfile item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedTrainingDueProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
