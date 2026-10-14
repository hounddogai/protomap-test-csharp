namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted certification batch calculation.</summary>
public interface IWeightedCertificationBatchModifier
{
    string Describe();
    double Adjust(WeightedCertificationBatch item, double valueValue);
}

public sealed record RoundingWeightedCertificationBatchAdjuster(double Step) : IWeightedCertificationBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedCertificationBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedCertificationBatchAdjuster(double Minimum) : IWeightedCertificationBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedCertificationBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record WeightedCertificationBatchLimitOffsetAdjuster(double Weight) : IWeightedCertificationBatchModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(WeightedCertificationBatch item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

/// <summary>Runs the weighted certification batch steps in order and keeps a trace of each.</summary>
public sealed class WeightedCertificationBatchEngine(List<IWeightedCertificationBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedCertificationBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedCertificationBatchEngine DefaultWeightedCertificationBatchEngine()
    {
        return new WeightedCertificationBatchEngine(
            new List<IWeightedCertificationBatchModifier>
            {
                new RoundingWeightedCertificationBatchAdjuster(0.1),
                new MinimumWeightedCertificationBatchAdjuster(34.7),
                new WeightedCertificationBatchLimitOffsetAdjuster(0.68),
            },
            new List<string>(),
            4.0,
            92136.8);
    }

    public double Price(WeightedCertificationBatch item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedCertificationBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
