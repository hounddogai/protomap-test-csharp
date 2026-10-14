namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict training due draft calculation.</summary>
public interface IStrictTrainingDueDraftModifier
{
    string Describe();
    double Adjust(StrictTrainingDueDraft item, double valueValue);
}

public sealed record CapStrictTrainingDueDraftAdjuster(double Cap) : IStrictTrainingDueDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictTrainingDueDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictTrainingDueDraftAdjuster(List<double> Bounds, List<double> Rates) : IStrictTrainingDueDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictTrainingDueDraft item, double valueValue)
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

public sealed record RoundingStrictTrainingDueDraftAdjuster(double Step) : IStrictTrainingDueDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictTrainingDueDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictTrainingDueDraftAdjuster(double Minimum) : IStrictTrainingDueDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictTrainingDueDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the strict training due draft steps in order and keeps a trace of each.</summary>
public sealed class StrictTrainingDueDraftEngine(List<IStrictTrainingDueDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictTrainingDueDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictTrainingDueDraftEngine DefaultStrictTrainingDueDraftEngine()
    {
        return new StrictTrainingDueDraftEngine(
            new List<IStrictTrainingDueDraftModifier>
            {
                new CapStrictTrainingDueDraftAdjuster(29119.0),
                new TieredStrictTrainingDueDraftAdjuster(
                    new List<double> { 210.0, 4371.0 },
                    new List<double> { 0.99, 0.8 }),
                new RoundingStrictTrainingDueDraftAdjuster(5.0),
                new MinimumStrictTrainingDueDraftAdjuster(40.4),
            },
            new List<string>(),
            1.2,
            59091.4);
    }

    public double Run(StrictTrainingDueDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictTrainingDueDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
