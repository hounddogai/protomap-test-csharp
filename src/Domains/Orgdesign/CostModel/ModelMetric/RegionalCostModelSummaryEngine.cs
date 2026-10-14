namespace Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>One step of the regional cost model summary calculation.</summary>
public interface IRegionalCostModelSummaryModifier
{
    string Describe();
    double Adjust(RegionalCostModelSummary item, double valueValue);
}

public sealed record CategoryRegionalCostModelSummaryAdjuster(Dictionary<string, double> Factors) : IRegionalCostModelSummaryModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RegionalCostModelSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalCostModelSummaryAdjuster(double Minimum) : IRegionalCostModelSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalCostModelSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRegionalCostModelSummaryAdjuster(List<double> Bounds, List<double> Rates) : IRegionalCostModelSummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalCostModelSummary item, double valueValue)
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

public sealed record CapRegionalCostModelSummaryAdjuster(double Cap) : IRegionalCostModelSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalCostModelSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalCostModelSummaryCreatedAtOffsetAdjuster(double Weight) : IRegionalCostModelSummaryModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(RegionalCostModelSummary item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the regional cost model summary steps in order and keeps a trace of each.</summary>
public sealed class RegionalCostModelSummaryEngine(List<IRegionalCostModelSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalCostModelSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalCostModelSummaryEngine DefaultRegionalCostModelSummaryEngine()
    {
        return new RegionalCostModelSummaryEngine(
            new List<IRegionalCostModelSummaryModifier>
            {
                new CategoryRegionalCostModelSummaryAdjuster(new Dictionary<string, double> { ["north"] = 1.09 }),
                new MinimumRegionalCostModelSummaryAdjuster(37.8),
                new TieredRegionalCostModelSummaryAdjuster(
                    new List<double> { 779.0, 2306.0, 4318.0 },
                    new List<double> { 0.81, 0.99, 1.08 }),
                new CapRegionalCostModelSummaryAdjuster(29657.0),
                new RegionalCostModelSummaryCreatedAtOffsetAdjuster(0.06),
            },
            new List<string>(),
            3.5,
            74729.2);
    }

    public double Evaluate(RegionalCostModelSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalCostModelSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
