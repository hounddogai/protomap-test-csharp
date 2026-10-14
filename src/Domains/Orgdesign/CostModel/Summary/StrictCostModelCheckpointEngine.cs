namespace Acme.Workforce.Orgdesign.CostModel.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict cost model checkpoint calculation.</summary>
public interface IStrictCostModelCheckpointStep
{
    string Describe();
    double Adjust(StrictCostModelCheckpoint item, double valueValue);
}

public sealed record StrictCostModelCheckpointTenureMonthsOffsetAdjuster(double Weight) : IStrictCostModelCheckpointStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(StrictCostModelCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record RoundingStrictCostModelCheckpointAdjuster(double Step) : IStrictCostModelCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictCostModelCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictCostModelCheckpointAdjuster(double Percent) : IStrictCostModelCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictCostModelCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict cost model checkpoint steps in order and keeps a trace of each.</summary>
public sealed class StrictCostModelCheckpointEngine(List<IStrictCostModelCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictCostModelCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictCostModelCheckpointEngine DefaultStrictCostModelCheckpointEngine()
    {
        return new StrictCostModelCheckpointEngine(
            new List<IStrictCostModelCheckpointStep>
            {
                new StrictCostModelCheckpointTenureMonthsOffsetAdjuster(0.4),
                new RoundingStrictCostModelCheckpointAdjuster(0.05),
                new PercentStrictCostModelCheckpointAdjuster(3.6),
            },
            new List<string>(),
            4.8,
            83791.3);
    }

    public double Price(StrictCostModelCheckpoint item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictCostModelCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
