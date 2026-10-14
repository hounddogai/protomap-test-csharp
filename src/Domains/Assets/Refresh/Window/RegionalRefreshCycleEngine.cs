namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional refresh cycle calculation.</summary>
public interface IRegionalRefreshCycleStep
{
    string Describe();
    double Adjust(RegionalRefreshCycle item, double valueValue);
}

public sealed record CapRegionalRefreshCycleAdjuster(double Cap) : IRegionalRefreshCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalRefreshCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRegionalRefreshCycleAdjuster(List<double> Bounds, List<double> Rates) : IRegionalRefreshCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalRefreshCycle item, double valueValue)
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

public sealed record MinimumRegionalRefreshCycleAdjuster(double Minimum) : IRegionalRefreshCycleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalRefreshCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalRefreshCycleAdjuster(double Percent) : IRegionalRefreshCycleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalRefreshCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameRegionalRefreshCycleAdjuster(Dictionary<string, double> Factors) : IRegionalRefreshCycleStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalRefreshCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRegionalRefreshCycleAdjuster(double Step) : IRegionalRefreshCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalRefreshCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional refresh cycle steps in order and keeps a trace of each.</summary>
public sealed class RegionalRefreshCycleEngine(List<IRegionalRefreshCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalRefreshCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalRefreshCycleEngine DefaultRegionalRefreshCycleEngine()
    {
        return new RegionalRefreshCycleEngine(
            new List<IRegionalRefreshCycleStep>
            {
                new CapRegionalRefreshCycleAdjuster(26100.0),
                new TieredRegionalRefreshCycleAdjuster(
                    new List<double> { 256.0, 4950.0 },
                    new List<double> { 1.14, 0.84 }),
                new MinimumRegionalRefreshCycleAdjuster(23.4),
                new PercentRegionalRefreshCycleAdjuster(-16.6),
                new NameRegionalRefreshCycleAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.37, ["central"] = 1.12, ["east"] = 0.92 }),
                new RoundingRegionalRefreshCycleAdjuster(1.0),
            },
            new List<string>(),
            4.2,
            85565.7);
    }

    public double Price(RegionalRefreshCycle item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalRefreshCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
