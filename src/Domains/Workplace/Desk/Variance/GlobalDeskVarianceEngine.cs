namespace Acme.Workforce.Workplace.Desk.Variance;

/// <summary>One step of the global desk variance calculation.</summary>
public interface IGlobalDeskVarianceModifier
{
    string Describe();
    double Adjust(GlobalDeskVariance item, double valueValue);
}

public sealed record MinimumGlobalDeskVarianceAdjuster(double Minimum) : IGlobalDeskVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalDeskVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalDeskVarianceAdjuster(double Cap) : IGlobalDeskVarianceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalDeskVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelGlobalDeskVarianceAdjuster(Dictionary<string, double> Factors) : IGlobalDeskVarianceModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalDeskVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record GlobalDeskVarianceCostOffsetAdjuster(double Weight) : IGlobalDeskVarianceModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(GlobalDeskVariance item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record TieredGlobalDeskVarianceAdjuster(List<double> Bounds, List<double> Rates) : IGlobalDeskVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalDeskVariance item, double valueValue)
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

/// <summary>Runs the global desk variance steps in order and keeps a trace of each.</summary>
public sealed class GlobalDeskVarianceEngine(List<IGlobalDeskVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalDeskVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalDeskVarianceEngine DefaultGlobalDeskVarianceEngine()
    {
        return new GlobalDeskVarianceEngine(
            new List<IGlobalDeskVarianceModifier>
            {
                new MinimumGlobalDeskVarianceAdjuster(48.0),
                new CapGlobalDeskVarianceAdjuster(19447.0),
                new LabelGlobalDeskVarianceAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.77, ["online"] = 0.82 }),
                new GlobalDeskVarianceCostOffsetAdjuster(1.16),
                new TieredGlobalDeskVarianceAdjuster(
                    new List<double> { 904.0, 5022.0, 6335.0 },
                    new List<double> { 0.86, 1.07, 1.08 }),
            },
            new List<string>(),
            1.4,
            42894.6);
    }

    public double Evaluate(GlobalDeskVariance item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalDeskVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
