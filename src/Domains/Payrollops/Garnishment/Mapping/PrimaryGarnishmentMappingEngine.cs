namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>One step of the primary garnishment mapping calculation.</summary>
public interface IPrimaryGarnishmentMappingFactor
{
    string Describe();
    double Adjust(PrimaryGarnishmentMapping item, double valueValue);
}

public sealed record CapPrimaryGarnishmentMappingAdjuster(double Cap) : IPrimaryGarnishmentMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryGarnishmentMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NamePrimaryGarnishmentMappingAdjuster(Dictionary<string, double> Factors) : IPrimaryGarnishmentMappingFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryGarnishmentMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PrimaryGarnishmentMappingUpdatedAtOffsetAdjuster(double Weight) : IPrimaryGarnishmentMappingFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(PrimaryGarnishmentMapping item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentPrimaryGarnishmentMappingAdjuster(double Percent) : IPrimaryGarnishmentMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryGarnishmentMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPrimaryGarnishmentMappingAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryGarnishmentMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryGarnishmentMapping item, double valueValue)
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

/// <summary>Runs the primary garnishment mapping steps in order and keeps a trace of each.</summary>
public sealed class PrimaryGarnishmentMappingEngine(List<IPrimaryGarnishmentMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryGarnishmentMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryGarnishmentMappingEngine DefaultPrimaryGarnishmentMappingEngine()
    {
        return new PrimaryGarnishmentMappingEngine(
            new List<IPrimaryGarnishmentMappingFactor>
            {
                new CapPrimaryGarnishmentMappingAdjuster(38243.0),
                new NamePrimaryGarnishmentMappingAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.01, ["north"] = 1.07, ["west"] = 0.74 }),
                new PrimaryGarnishmentMappingUpdatedAtOffsetAdjuster(1.99),
                new PercentPrimaryGarnishmentMappingAdjuster(-11.0),
                new TieredPrimaryGarnishmentMappingAdjuster(
                    new List<double> { 1704.0, 5519.0, 7700.0 },
                    new List<double> { 0.96, 1.01, 1.03 }),
            },
            new List<string>(),
            3.2,
            94168.0);
    }

    public double Compute(PrimaryGarnishmentMapping item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryGarnishmentMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
