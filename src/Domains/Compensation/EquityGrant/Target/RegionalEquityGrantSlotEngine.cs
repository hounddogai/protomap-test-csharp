namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional equity grant slot calculation.</summary>
public interface IRegionalEquityGrantSlotModifier
{
    string Describe();
    double Adjust(RegionalEquityGrantSlot item, double valueValue);
}

public sealed record RoundingRegionalEquityGrantSlotAdjuster(double Step) : IRegionalEquityGrantSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalEquityGrantSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRegionalEquityGrantSlotAdjuster(double Cap) : IRegionalEquityGrantSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalEquityGrantSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalEquityGrantSlotAdjuster(double Percent) : IRegionalEquityGrantSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalEquityGrantSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional equity grant slot steps in order and keeps a trace of each.</summary>
public sealed class RegionalEquityGrantSlotEngine(List<IRegionalEquityGrantSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalEquityGrantSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalEquityGrantSlotEngine DefaultRegionalEquityGrantSlotEngine()
    {
        return new RegionalEquityGrantSlotEngine(
            new List<IRegionalEquityGrantSlotModifier>
            {
                new RoundingRegionalEquityGrantSlotAdjuster(1.0),
                new CapRegionalEquityGrantSlotAdjuster(33416.0),
                new PercentRegionalEquityGrantSlotAdjuster(-22.6),
            },
            new List<string>(),
            3.1,
            7699.5);
    }

    public double Evaluate(RegionalEquityGrantSlot item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalEquityGrantSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
