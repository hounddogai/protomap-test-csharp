namespace Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>One step of the primary training due plan calculation.</summary>
public interface IPrimaryTrainingDuePlanAdjuster
{
    string Describe();
    double Adjust(PrimaryTrainingDuePlan item, double valueValue);
}

public sealed record CapPrimaryTrainingDuePlanAdjuster(double Cap) : IPrimaryTrainingDuePlanAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryTrainingDuePlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPrimaryTrainingDuePlanAdjuster(double Percent) : IPrimaryTrainingDuePlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryTrainingDuePlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitlePrimaryTrainingDuePlanAdjuster(Dictionary<string, double> Factors) : IPrimaryTrainingDuePlanAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryTrainingDuePlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPrimaryTrainingDuePlanAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryTrainingDuePlanAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryTrainingDuePlan item, double valueValue)
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

public sealed record MinimumPrimaryTrainingDuePlanAdjuster(double Minimum) : IPrimaryTrainingDuePlanAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryTrainingDuePlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary training due plan steps in order and keeps a trace of each.</summary>
public sealed class PrimaryTrainingDuePlanEngine(List<IPrimaryTrainingDuePlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryTrainingDuePlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryTrainingDuePlanEngine DefaultPrimaryTrainingDuePlanEngine()
    {
        return new PrimaryTrainingDuePlanEngine(
            new List<IPrimaryTrainingDuePlanAdjuster>
            {
                new CapPrimaryTrainingDuePlanAdjuster(33308.0),
                new PercentPrimaryTrainingDuePlanAdjuster(2.5),
                new TitlePrimaryTrainingDuePlanAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.32, ["retail"] = 0.81 }),
                new TieredPrimaryTrainingDuePlanAdjuster(
                    new List<double> { 1725.0, 4657.0, 8275.0 },
                    new List<double> { 1.05, 0.87, 0.94 }),
                new MinimumPrimaryTrainingDuePlanAdjuster(32.2),
            },
            new List<string>(),
            1.3,
            49976.3);
    }

    public double Run(PrimaryTrainingDuePlan item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryTrainingDuePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
