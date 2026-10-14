namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due window calculation.</summary>
public interface ITrainingDueWindowModifier
{
    string Describe();
    double Adjust(TrainingDueWindow item, double valueValue);
}

public sealed record FlaggedTrainingDueWindowAdjuster(Registry Flags, string Flag, double Percent) : ITrainingDueWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TrainingDueWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTrainingDueWindowAdjuster(double Percent) : ITrainingDueWindowModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDueWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelTrainingDueWindowAdjuster(Dictionary<string, double> Factors) : ITrainingDueWindowModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TrainingDueWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTrainingDueWindowAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDueWindowModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDueWindow item, double valueValue)
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

public sealed record CapTrainingDueWindowAdjuster(double Cap) : ITrainingDueWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TrainingDueWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TrainingDueWindowVersionOffsetAdjuster(double Weight) : ITrainingDueWindowModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TrainingDueWindow item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the training due window steps in order and keeps a trace of each.</summary>
public sealed class TrainingDueWindowEngine(List<ITrainingDueWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDueWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDueWindowEngine DefaultTrainingDueWindowEngine(Registry flags)
    {
        return new TrainingDueWindowEngine(
            new List<ITrainingDueWindowModifier>
            {
                new FlaggedTrainingDueWindowAdjuster(flags, "training-due-window-capped", -2.9),
                new PercentTrainingDueWindowAdjuster(-20.1),
                new ChannelTrainingDueWindowAdjuster(new Dictionary<string, double> { ["retail"] = 1.17 }),
                new TieredTrainingDueWindowAdjuster(
                    new List<double> { 1429.0, 1845.0, 5605.0 },
                    new List<double> { 1.19, 1.02, 0.81 }),
                new CapTrainingDueWindowAdjuster(4385.0),
                new TrainingDueWindowVersionOffsetAdjuster(0.95),
            },
            new List<string>(),
            3.9,
            60881.1);
    }

    public double Evaluate(TrainingDueWindow item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDueWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
