namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant score calculation.</summary>
public interface IEquityGrantScoreStep
{
    string Describe();
    double Adjust(EquityGrantScore item, double valueValue);
}

public sealed record RoundingEquityGrantScoreAdjuster(double Step) : IEquityGrantScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapEquityGrantScoreAdjuster(double Cap) : IEquityGrantScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record EquityGrantScoreRateOffsetAdjuster(double Weight) : IEquityGrantScoreStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(EquityGrantScore item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

/// <summary>Runs the equity grant score steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantScoreEngine(List<IEquityGrantScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantScoreEngine DefaultEquityGrantScoreEngine()
    {
        return new EquityGrantScoreEngine(
            new List<IEquityGrantScoreStep>
            {
                new RoundingEquityGrantScoreAdjuster(0.25),
                new CapEquityGrantScoreAdjuster(25793.0),
                new EquityGrantScoreRateOffsetAdjuster(1.74),
            },
            new List<string>(),
            1.0,
            43710.6);
    }

    public double Run(EquityGrantScore item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
