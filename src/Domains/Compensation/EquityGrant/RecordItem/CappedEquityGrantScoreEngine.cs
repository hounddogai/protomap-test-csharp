namespace Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>One step of the capped equity grant score calculation.</summary>
public interface ICappedEquityGrantScoreAdjuster
{
    string Describe();
    double Adjust(CappedEquityGrantScore item, double valueValue);
}

public sealed record PercentCappedEquityGrantScoreAdjuster(double Percent) : ICappedEquityGrantScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedEquityGrantScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedEquityGrantScoreAttemptsOffsetAdjuster(double Weight) : ICappedEquityGrantScoreAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(CappedEquityGrantScore item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CapCappedEquityGrantScoreAdjuster(double Cap) : ICappedEquityGrantScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedEquityGrantScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped equity grant score steps in order and keeps a trace of each.</summary>
public sealed class CappedEquityGrantScoreEngine(List<ICappedEquityGrantScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedEquityGrantScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedEquityGrantScoreEngine DefaultCappedEquityGrantScoreEngine()
    {
        return new CappedEquityGrantScoreEngine(
            new List<ICappedEquityGrantScoreAdjuster>
            {
                new PercentCappedEquityGrantScoreAdjuster(12.6),
                new CappedEquityGrantScoreAttemptsOffsetAdjuster(0.02),
                new CapCappedEquityGrantScoreAdjuster(42348.0),
            },
            new List<string>(),
            3.7,
            91909.2);
    }

    public double Price(CappedEquityGrantScore item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedEquityGrantScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
