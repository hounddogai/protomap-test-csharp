namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional refresh request calculation.</summary>
public interface IRegionalRefreshRequestStep
{
    string Describe();
    double Adjust(RegionalRefreshRequest item, double valueValue);
}

public sealed record CapRegionalRefreshRequestAdjuster(double Cap) : IRegionalRefreshRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalRefreshRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalRefreshRequestAdjuster(double Step) : IRegionalRefreshRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalRefreshRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalRefreshRequestAdjuster(List<double> Bounds, List<double> Rates) : IRegionalRefreshRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalRefreshRequest item, double valueValue)
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

public sealed record MinimumRegionalRefreshRequestAdjuster(double Minimum) : IRegionalRefreshRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalRefreshRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelRegionalRefreshRequestAdjuster(Dictionary<string, double> Factors) : IRegionalRefreshRequestStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalRefreshRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional refresh request steps in order and keeps a trace of each.</summary>
public sealed class RegionalRefreshRequestEngine(List<IRegionalRefreshRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalRefreshRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalRefreshRequestEngine DefaultRegionalRefreshRequestEngine()
    {
        return new RegionalRefreshRequestEngine(
            new List<IRegionalRefreshRequestStep>
            {
                new CapRegionalRefreshRequestAdjuster(44879.0),
                new RoundingRegionalRefreshRequestAdjuster(0.05),
                new TieredRegionalRefreshRequestAdjuster(
                    new List<double> { 1206.0, 7331.0, 9457.0 },
                    new List<double> { 0.88, 0.96, 0.85 }),
                new MinimumRegionalRefreshRequestAdjuster(45.2),
                new LabelRegionalRefreshRequestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.17, ["north"] = 0.85, ["east"] = 1.32 }),
            },
            new List<string>(),
            5.0,
            65236.1);
    }

    public double Evaluate(RegionalRefreshRequest item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalRefreshRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
