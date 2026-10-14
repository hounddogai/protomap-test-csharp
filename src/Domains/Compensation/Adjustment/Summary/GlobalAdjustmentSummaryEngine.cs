namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global adjustment summary calculation.</summary>
public interface IGlobalAdjustmentSummaryAdjuster
{
    string Describe();
    double Adjust(GlobalAdjustmentSummary item, double valueValue);
}

public sealed record PercentGlobalAdjustmentSummaryAdjuster(double Percent) : IGlobalAdjustmentSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalAdjustmentSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalAdjustmentSummaryEndsAtOffsetAdjuster(double Weight) : IGlobalAdjustmentSummaryAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(GlobalAdjustmentSummary item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record MinimumGlobalAdjustmentSummaryAdjuster(double Minimum) : IGlobalAdjustmentSummaryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAdjustmentSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredGlobalAdjustmentSummaryAdjuster(List<double> Bounds, List<double> Rates) : IGlobalAdjustmentSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalAdjustmentSummary item, double valueValue)
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

public sealed record RoundingGlobalAdjustmentSummaryAdjuster(double Step) : IGlobalAdjustmentSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAdjustmentSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global adjustment summary steps in order and keeps a trace of each.</summary>
public sealed class GlobalAdjustmentSummaryEngine(List<IGlobalAdjustmentSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAdjustmentSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAdjustmentSummaryEngine DefaultGlobalAdjustmentSummaryEngine()
    {
        return new GlobalAdjustmentSummaryEngine(
            new List<IGlobalAdjustmentSummaryAdjuster>
            {
                new PercentGlobalAdjustmentSummaryAdjuster(-4.5),
                new GlobalAdjustmentSummaryEndsAtOffsetAdjuster(1.54),
                new MinimumGlobalAdjustmentSummaryAdjuster(38.8),
                new TieredGlobalAdjustmentSummaryAdjuster(
                    new List<double> { 8124.0, 9619.0 },
                    new List<double> { 1.17, 0.97 }),
                new RoundingGlobalAdjustmentSummaryAdjuster(0.05),
            },
            new List<string>(),
            3.0,
            50381.3);
    }

    public double Evaluate(GlobalAdjustmentSummary item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAdjustmentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
