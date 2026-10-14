namespace Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>One step of the headcount window calculation.</summary>
public interface IHeadcountWindowFactor
{
    string Describe();
    double Adjust(HeadcountWindow item, double valueValue);
}

public sealed record TieredHeadcountWindowAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountWindowFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountWindow item, double valueValue)
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

public sealed record CapHeadcountWindowAdjuster(double Cap) : IHeadcountWindowFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentHeadcountWindowAdjuster(double Percent) : IHeadcountWindowFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceHeadcountWindowAdjuster(Dictionary<string, double> Factors) : IHeadcountWindowFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(HeadcountWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the headcount window steps in order and keeps a trace of each.</summary>
public sealed class HeadcountWindowEngine(List<IHeadcountWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountWindowEngine DefaultHeadcountWindowEngine()
    {
        return new HeadcountWindowEngine(
            new List<IHeadcountWindowFactor>
            {
                new TieredHeadcountWindowAdjuster(
                    new List<double> { 3197.0, 5890.0, 6423.0, 8425.0 },
                    new List<double> { 1.02, 0.89, 0.91, 1.19 }),
                new CapHeadcountWindowAdjuster(3470.0),
                new PercentHeadcountWindowAdjuster(-6.0),
                new SourceHeadcountWindowAdjuster(new Dictionary<string, double> { ["east"] = 1.34 }),
            },
            new List<string>(),
            3.1,
            96967.9);
    }

    public double Price(HeadcountWindow item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
