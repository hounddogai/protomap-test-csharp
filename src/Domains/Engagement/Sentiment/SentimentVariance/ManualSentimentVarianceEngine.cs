namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual sentiment variance calculation.</summary>
public interface IManualSentimentVarianceStep
{
    string Describe();
    double Adjust(ManualSentimentVariance item, double valueValue);
}

public sealed record TieredManualSentimentVarianceAdjuster(List<double> Bounds, List<double> Rates) : IManualSentimentVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualSentimentVariance item, double valueValue)
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

public sealed record ManualSentimentVariancePriorityOffsetAdjuster(double Weight) : IManualSentimentVarianceStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(ManualSentimentVariance item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record MinimumManualSentimentVarianceAdjuster(double Minimum) : IManualSentimentVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualSentimentVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentManualSentimentVarianceAdjuster(double Percent) : IManualSentimentVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualSentimentVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentManualSentimentVarianceAdjuster(Dictionary<string, double> Factors) : IManualSentimentVarianceStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ManualSentimentVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingManualSentimentVarianceAdjuster(double Step) : IManualSentimentVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualSentimentVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the manual sentiment variance steps in order and keeps a trace of each.</summary>
public sealed class ManualSentimentVarianceEngine(List<IManualSentimentVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualSentimentVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualSentimentVarianceEngine DefaultManualSentimentVarianceEngine()
    {
        return new ManualSentimentVarianceEngine(
            new List<IManualSentimentVarianceStep>
            {
                new TieredManualSentimentVarianceAdjuster(
                    new List<double> { 2866.0, 5137.0, 9937.0 },
                    new List<double> { 0.89, 0.87, 1.08 }),
                new ManualSentimentVariancePriorityOffsetAdjuster(0.18),
                new MinimumManualSentimentVarianceAdjuster(7.7),
                new PercentManualSentimentVarianceAdjuster(-2.8),
                new DepartmentManualSentimentVarianceAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.25, ["west"] = 1.22 }),
                new RoundingManualSentimentVarianceAdjuster(0.25),
            },
            new List<string>(),
            4.3,
            87769.1);
    }

    public double Evaluate(ManualSentimentVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualSentimentVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
