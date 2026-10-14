namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional career path rollup calculation.</summary>
public interface IRegionalCareerPathRollupAdjuster
{
    string Describe();
    double Adjust(RegionalCareerPathRollup item, double valueValue);
}

public sealed record CapRegionalCareerPathRollupAdjuster(double Cap) : IRegionalCareerPathRollupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalCareerPathRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalCareerPathRollupAdjuster(double Percent) : IRegionalCareerPathRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalCareerPathRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalCareerPathRollupAdjuster(double Minimum) : IRegionalCareerPathRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalCareerPathRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRegionalCareerPathRollupAdjuster(double Step) : IRegionalCareerPathRollupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalCareerPathRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalCareerPathRollupAdjuster(List<double> Bounds, List<double> Rates) : IRegionalCareerPathRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalCareerPathRollup item, double valueValue)
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

/// <summary>Runs the regional career path rollup steps in order and keeps a trace of each.</summary>
public sealed class RegionalCareerPathRollupEngine(List<IRegionalCareerPathRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalCareerPathRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalCareerPathRollupEngine DefaultRegionalCareerPathRollupEngine()
    {
        return new RegionalCareerPathRollupEngine(
            new List<IRegionalCareerPathRollupAdjuster>
            {
                new CapRegionalCareerPathRollupAdjuster(17312.0),
                new PercentRegionalCareerPathRollupAdjuster(-0.9),
                new MinimumRegionalCareerPathRollupAdjuster(11.4),
                new RoundingRegionalCareerPathRollupAdjuster(1.0),
                new TieredRegionalCareerPathRollupAdjuster(
                    new List<double> { 1972.0, 3835.0, 7864.0 },
                    new List<double> { 0.93, 1.15, 0.95 }),
            },
            new List<string>(),
            3.0,
            71121.9);
    }

    public double Price(RegionalCareerPathRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalCareerPathRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
