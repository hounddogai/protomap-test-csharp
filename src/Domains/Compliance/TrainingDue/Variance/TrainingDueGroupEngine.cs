namespace Acme.Workforce.Compliance.TrainingDue.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due group calculation.</summary>
public interface ITrainingDueGroupModifier
{
    string Describe();
    double Adjust(TrainingDueGroup item, double valueValue);
}

public sealed record CapTrainingDueGroupAdjuster(double Cap) : ITrainingDueGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTrainingDueGroupAdjuster(double Step) : ITrainingDueGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDueGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TrainingDueGroupWindowDaysOffsetAdjuster(double Weight) : ITrainingDueGroupModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(TrainingDueGroup item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record FlaggedTrainingDueGroupAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTrainingDueGroupAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueGroup item, double valueValue)
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

public sealed record PercentTrainingDueGroupAdjuster(double Percent) : ITrainingDueGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the training due group steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueGroupEngine(List<ITrainingDueGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueGroupEngine DefaultTrainingDueGroupEngine(Registry flags)
    {
        return new TrainingDueGroupEngine(
            new List<ITrainingDueGroupModifier>
            {
                new CapTrainingDueGroupAdjuster(14282.0),
                new RoundingTrainingDueGroupAdjuster(0.1),
                new TrainingDueGroupWindowDaysOffsetAdjuster(0.84),
                new FlaggedTrainingDueGroupAdjuster(flags, "training-due-group-strict", 7.4),
                new TieredTrainingDueGroupAdjuster(
                    new List<double> { 3083.0, 5959.0, 7218.0 },
                    new List<double> { 0.87, 0.89, 0.94 }),
                new PercentTrainingDueGroupAdjuster(6.3),
            },
            new List<string>(),
            4.0,
            10863.5);
    }

    public double Price(TrainingDueGroup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
