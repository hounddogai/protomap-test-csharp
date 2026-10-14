namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted accrual bundle calculation.</summary>
public interface IWeightedAccrualBundleStep
{
    string Describe();
    double Adjust(WeightedAccrualBundle item, double valueValue);
}

public sealed record WeightedAccrualBundleCeilingOffsetAdjuster(double Weight) : IWeightedAccrualBundleStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(WeightedAccrualBundle item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record MinimumWeightedAccrualBundleAdjuster(double Minimum) : IWeightedAccrualBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAccrualBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedAccrualBundleAdjuster(double Percent) : IWeightedAccrualBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAccrualBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedAccrualBundleAdjuster(double Step) : IWeightedAccrualBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedAccrualBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted accrual bundle steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccrualBundleEngine(List<IWeightedAccrualBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccrualBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccrualBundleEngine DefaultWeightedAccrualBundleEngine()
    {
        return new WeightedAccrualBundleEngine(
            new List<IWeightedAccrualBundleStep>
            {
                new WeightedAccrualBundleCeilingOffsetAdjuster(1.2),
                new MinimumWeightedAccrualBundleAdjuster(2.0),
                new PercentWeightedAccrualBundleAdjuster(19.8),
                new RoundingWeightedAccrualBundleAdjuster(1.0),
            },
            new List<string>(),
            4.8,
            32406.4);
    }

    public double Compute(WeightedAccrualBundle item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccrualBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
