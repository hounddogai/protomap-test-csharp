namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement exception calculation.</summary>
public interface IEngagementExceptionFactor
{
    string Describe();
    double Adjust(EngagementException item, double valueValue);
}

public sealed record TieredEngagementExceptionAdjuster(List<double> Bounds, List<double> Rates) : IEngagementExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementException item, double valueValue)
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

public sealed record LocaleEngagementExceptionAdjuster(Dictionary<string, double> Factors) : IEngagementExceptionFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(EngagementException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumEngagementExceptionAdjuster(double Minimum) : IEngagementExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record EngagementExceptionMaximumOffsetAdjuster(double Weight) : IEngagementExceptionFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(EngagementException item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the engagement exception steps in order and keeps a trace of each.</summary>
public sealed class EngagementExceptionEngine(List<IEngagementExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementExceptionEngine DefaultEngagementExceptionEngine()
    {
        return new EngagementExceptionEngine(
            new List<IEngagementExceptionFactor>
            {
                new TieredEngagementExceptionAdjuster(
                    new List<double> { 332.0, 4348.0 },
                    new List<double> { 0.94, 1.06 }),
                new LocaleEngagementExceptionAdjuster(new Dictionary<string, double> { ["online"] = 0.99 }),
                new MinimumEngagementExceptionAdjuster(13.0),
                new EngagementExceptionMaximumOffsetAdjuster(0.32),
            },
            new List<string>(),
            1.0,
            87724.1);
    }

    public double Price(EngagementException item)
    {
        var current = item.EffectiveVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<EngagementException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
