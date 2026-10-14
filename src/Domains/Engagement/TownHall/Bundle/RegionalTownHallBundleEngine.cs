namespace Acme.Workforce.Engagement.TownHall.Bundle;

/// <summary>One step of the regional town hall bundle calculation.</summary>
public interface IRegionalTownHallBundleStep
{
    string Describe();
    double Adjust(RegionalTownHallBundle item, double valueValue);
}

public sealed record PercentRegionalTownHallBundleAdjuster(double Percent) : IRegionalTownHallBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTownHallBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRegionalTownHallBundleAdjuster(Dictionary<string, double> Factors) : IRegionalTownHallBundleStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RegionalTownHallBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RegionalTownHallBundleFloorValueOffsetAdjuster(double Weight) : IRegionalTownHallBundleStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(RegionalTownHallBundle item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the regional town hall bundle steps in order and keeps a trace of each.</summary>
public sealed class RegionalTownHallBundleEngine(List<IRegionalTownHallBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTownHallBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTownHallBundleEngine DefaultRegionalTownHallBundleEngine()
    {
        return new RegionalTownHallBundleEngine(
            new List<IRegionalTownHallBundleStep>
            {
                new PercentRegionalTownHallBundleAdjuster(21.7),
                new TitleRegionalTownHallBundleAdjuster(new Dictionary<string, double> { ["retail"] = 1.02 }),
                new RegionalTownHallBundleFloorValueOffsetAdjuster(1.71),
            },
            new List<string>(),
            0.4,
            31705.4);
    }

    public double Run(RegionalTownHallBundle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTownHallBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
