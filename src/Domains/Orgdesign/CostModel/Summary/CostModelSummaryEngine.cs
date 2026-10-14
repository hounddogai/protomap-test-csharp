namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model summary calculation.</summary>
public interface ICostModelSummaryStep
{
    string Describe();
    double Adjust(CostModelSummary item, double valueValue);
}

public sealed record SourceCostModelSummaryAdjuster(Dictionary<string, double> Factors) : ICostModelSummaryStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(CostModelSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CostModelSummaryBatchSizeOffsetAdjuster(double Weight) : ICostModelSummaryStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(CostModelSummary item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record RoundingCostModelSummaryAdjuster(double Step) : ICostModelSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCostModelSummaryAdjuster(double Percent) : ICostModelSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelSummaryAdjuster(List<double> Bounds, List<double> Rates) : ICostModelSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelSummary item, double valueValue)
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

/// <summary>Runs the cost model summary steps in order and keeps a trace of each.</summary>
public sealed class CostModelSummaryEngine(List<ICostModelSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelSummaryEngine DefaultCostModelSummaryEngine()
    {
        return new CostModelSummaryEngine(
            new List<ICostModelSummaryStep>
            {
                new SourceCostModelSummaryAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["central"] = 1.12 }),
                new CostModelSummaryBatchSizeOffsetAdjuster(0.81),
                new RoundingCostModelSummaryAdjuster(0.25),
                new PercentCostModelSummaryAdjuster(20.5),
                new TieredCostModelSummaryAdjuster(
                    new List<double> { 1954.0, 2317.0, 5964.0 },
                    new List<double> { 0.86, 1.12, 0.85 }),
            },
            new List<string>(),
            3.4,
            34259.6);
    }

    public double Price(CostModelSummary item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
