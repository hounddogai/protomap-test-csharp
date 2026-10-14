namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pay band estimate calculation.</summary>
public interface IDefaultPayBandEstimateStep
{
    string Describe();
    double Adjust(DefaultPayBandEstimate item, double valueValue);
}

public sealed record RoundingDefaultPayBandEstimateAdjuster(double Step) : IDefaultPayBandEstimateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPayBandEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultPayBandEstimateAdjuster(double Percent) : IDefaultPayBandEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayBandEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultPayBandEstimateAdjuster(double Cap) : IDefaultPayBandEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayBandEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDefaultPayBandEstimateAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPayBandEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPayBandEstimate item, double valueValue)
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

public sealed record MinimumDefaultPayBandEstimateAdjuster(double Minimum) : IDefaultPayBandEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayBandEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamDefaultPayBandEstimateAdjuster(Dictionary<string, double> Factors) : IDefaultPayBandEstimateStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DefaultPayBandEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default pay band estimate steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayBandEstimateEngine(List<IDefaultPayBandEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayBandEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayBandEstimateEngine DefaultDefaultPayBandEstimateEngine()
    {
        return new DefaultPayBandEstimateEngine(
            new List<IDefaultPayBandEstimateStep>
            {
                new RoundingDefaultPayBandEstimateAdjuster(1.0),
                new PercentDefaultPayBandEstimateAdjuster(-3.6),
                new CapDefaultPayBandEstimateAdjuster(48088.0),
                new TieredDefaultPayBandEstimateAdjuster(
                    new List<double> { 2409.0, 3364.0, 4181.0, 7742.0 },
                    new List<double> { 1.13, 1.13, 1.19, 0.97 }),
                new MinimumDefaultPayBandEstimateAdjuster(37.4),
                new TeamDefaultPayBandEstimateAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.85, ["south"] = 0.82 }),
            },
            new List<string>(),
            3.7,
            11851.5);
    }

    public double Price(DefaultPayBandEstimate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayBandEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
