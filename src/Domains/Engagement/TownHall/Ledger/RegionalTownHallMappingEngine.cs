namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional town hall mapping calculation.</summary>
public interface IRegionalTownHallMappingFactor
{
    string Describe();
    double Adjust(RegionalTownHallMapping item, double valueValue);
}

public sealed record CapRegionalTownHallMappingAdjuster(double Cap) : IRegionalTownHallMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalTownHallMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalTownHallMappingAdjuster(double Step) : IRegionalTownHallMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalTownHallMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalTownHallMappingAdjuster(double Minimum) : IRegionalTownHallMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalTownHallMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRegionalTownHallMappingAdjuster(Dictionary<string, double> Factors) : IRegionalTownHallMappingFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalTownHallMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalTownHallMappingVersionOffsetAdjuster(double Weight) : IRegionalTownHallMappingFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(RegionalTownHallMapping item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the regional town hall mapping steps in order and keeps a trace of each.</summary>
public sealed class RegionalTownHallMappingEngine(List<IRegionalTownHallMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTownHallMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTownHallMappingEngine DefaultRegionalTownHallMappingEngine()
    {
        return new RegionalTownHallMappingEngine(
            new List<IRegionalTownHallMappingFactor>
            {
                new CapRegionalTownHallMappingAdjuster(24463.0),
                new RoundingRegionalTownHallMappingAdjuster(5.0),
                new MinimumRegionalTownHallMappingAdjuster(6.9),
                new NameRegionalTownHallMappingAdjuster(new Dictionary<string, double> { ["north"] = 1.05 }),
                new RegionalTownHallMappingVersionOffsetAdjuster(0.98),
            },
            new List<string>(),
            1.2,
            42768.3);
    }

    public double Price(RegionalTownHallMapping item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTownHallMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
