namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the training due policy calculation.</summary>
public interface ITrainingDuePolicyAdjuster
{
    string Describe();
    double Adjust(TrainingDuePolicy item, double valueValue);
}

public sealed record TrainingDuePolicyVersionOffsetAdjuster(double Weight) : ITrainingDuePolicyAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(TrainingDuePolicy item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumTrainingDuePolicyAdjuster(double Minimum) : ITrainingDuePolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TrainingDuePolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelTrainingDuePolicyAdjuster(Dictionary<string, double> Factors) : ITrainingDuePolicyAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TrainingDuePolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTrainingDuePolicyAdjuster(List<double> Bounds, List<double> Rates) : ITrainingDuePolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TrainingDuePolicy item, double valueValue)
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

public sealed record PercentTrainingDuePolicyAdjuster(double Percent) : ITrainingDuePolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TrainingDuePolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTrainingDuePolicyAdjuster(double Step) : ITrainingDuePolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TrainingDuePolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the training due policy steps in order and keeps a trace of each.</summary>
public sealed class TrainingDuePolicyEngine(List<ITrainingDuePolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITrainingDuePolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TrainingDuePolicyEngine DefaultTrainingDuePolicyEngine()
    {
        return new TrainingDuePolicyEngine(
            new List<ITrainingDuePolicyAdjuster>
            {
                new TrainingDuePolicyVersionOffsetAdjuster(1.8),
                new MinimumTrainingDuePolicyAdjuster(2.2),
                new ChannelTrainingDuePolicyAdjuster(new Dictionary<string, double> { ["central"] = 1.36 }),
                new TieredTrainingDuePolicyAdjuster(
                    new List<double> { 6358.0, 6610.0, 9305.0 },
                    new List<double> { 1.2, 1.09, 0.94 }),
                new PercentTrainingDuePolicyAdjuster(-7.0),
                new RoundingTrainingDuePolicyAdjuster(0.05),
            },
            new List<string>(),
            0.2,
            24088.5);
    }

    public double Price(TrainingDuePolicy item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TrainingDuePolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
