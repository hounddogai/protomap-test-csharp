namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

/// <summary>One step of the manual succession plan batch calculation.</summary>
public interface IManualSuccessionPlanBatchFactor
{
    string Describe();
    double Adjust(ManualSuccessionPlanBatch item, double valueValue);
}

public sealed record PercentManualSuccessionPlanBatchAdjuster(double Percent) : IManualSuccessionPlanBatchFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSuccessionPlanBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualSuccessionPlanBatchAdjuster(double Minimum) : IManualSuccessionPlanBatchFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualSuccessionPlanBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ManualSuccessionPlanBatchStartsAtOffsetAdjuster(double Weight) : IManualSuccessionPlanBatchFactor
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(ManualSuccessionPlanBatch item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record CapManualSuccessionPlanBatchAdjuster(double Cap) : IManualSuccessionPlanBatchFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualSuccessionPlanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the manual succession plan batch steps in order and keeps a trace of each.</summary>
public sealed class ManualSuccessionPlanBatchEngine(List<IManualSuccessionPlanBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSuccessionPlanBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSuccessionPlanBatchEngine DefaultManualSuccessionPlanBatchEngine()
    {
        return new ManualSuccessionPlanBatchEngine(
            new List<IManualSuccessionPlanBatchFactor>
            {
                new PercentManualSuccessionPlanBatchAdjuster(-23.5),
                new MinimumManualSuccessionPlanBatchAdjuster(13.2),
                new ManualSuccessionPlanBatchStartsAtOffsetAdjuster(0.1),
                new CapManualSuccessionPlanBatchAdjuster(4236.0),
            },
            new List<string>(),
            0.7,
            40951.2);
    }

    public double Price(ManualSuccessionPlanBatch item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualSuccessionPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
