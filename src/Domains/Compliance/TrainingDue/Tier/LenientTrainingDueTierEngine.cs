namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient training due tier calculation.</summary>
public interface ILenientTrainingDueTierStep
{
    string Describe();
    double Adjust(LenientTrainingDueTier item, double valueValue);
}

public sealed record TieredLenientTrainingDueTierAdjuster(List<double> Bounds, List<double> Rates) : ILenientTrainingDueTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientTrainingDueTier item, double valueValue)
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

public sealed record RoundingLenientTrainingDueTierAdjuster(double Step) : ILenientTrainingDueTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientTrainingDueTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLenientTrainingDueTierAdjuster(double Minimum) : ILenientTrainingDueTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientTrainingDueTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LenientTrainingDueTierCeilingOffsetAdjuster(double Weight) : ILenientTrainingDueTierStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(LenientTrainingDueTier item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

/// <summary>Runs the lenient training due tier steps in order and keeps a trace of each.</summary>
public sealed class LenientTrainingDueTierEngine(List<ILenientTrainingDueTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTrainingDueTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTrainingDueTierEngine DefaultLenientTrainingDueTierEngine()
    {
        return new LenientTrainingDueTierEngine(
            new List<ILenientTrainingDueTierStep>
            {
                new TieredLenientTrainingDueTierAdjuster(
                    new List<double> { 2439.0, 4212.0, 6399.0, 8280.0 },
                    new List<double> { 0.89, 1.14, 1.07, 1.16 }),
                new RoundingLenientTrainingDueTierAdjuster(0.1),
                new MinimumLenientTrainingDueTierAdjuster(0.1),
                new LenientTrainingDueTierCeilingOffsetAdjuster(1.79),
            },
            new List<string>(),
            1.3,
            14495.6);
    }

    public double Evaluate(LenientTrainingDueTier item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTrainingDueTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
