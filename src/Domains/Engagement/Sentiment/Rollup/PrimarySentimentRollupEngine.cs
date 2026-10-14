namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary sentiment rollup calculation.</summary>
public interface IPrimarySentimentRollupFactor
{
    string Describe();
    double Adjust(PrimarySentimentRollup item, double valueValue);
}

public sealed record RoundingPrimarySentimentRollupAdjuster(double Step) : IPrimarySentimentRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimarySentimentRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPrimarySentimentRollupAdjuster(double Minimum) : IPrimarySentimentRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimarySentimentRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPrimarySentimentRollupAdjuster(double Percent) : IPrimarySentimentRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimarySentimentRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary sentiment rollup steps in order and keeps a trace of each.</summary>
public sealed class PrimarySentimentRollupEngine(List<IPrimarySentimentRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySentimentRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySentimentRollupEngine DefaultPrimarySentimentRollupEngine()
    {
        return new PrimarySentimentRollupEngine(
            new List<IPrimarySentimentRollupFactor>
            {
                new RoundingPrimarySentimentRollupAdjuster(1.0),
                new MinimumPrimarySentimentRollupAdjuster(25.6),
                new PercentPrimarySentimentRollupAdjuster(-14.5),
            },
            new List<string>(),
            4.5,
            53265.2);
    }

    public double Evaluate(PrimarySentimentRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySentimentRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
