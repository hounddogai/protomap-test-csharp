namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered cost model entry calculation.</summary>
public interface ITieredCostModelEntryStep
{
    string Describe();
    double Adjust(TieredCostModelEntry item, double valueValue);
}

public sealed record PercentTieredCostModelEntryAdjuster(double Percent) : ITieredCostModelEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredCostModelEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredCostModelEntryAdjuster(double Step) : ITieredCostModelEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredCostModelEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredCostModelEntryAdjuster(double Minimum) : ITieredCostModelEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredCostModelEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered cost model entry steps in order and keeps a trace of each.</summary>
public sealed class TieredCostModelEntryEngine(List<ITieredCostModelEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredCostModelEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredCostModelEntryEngine DefaultTieredCostModelEntryEngine()
    {
        return new TieredCostModelEntryEngine(
            new List<ITieredCostModelEntryStep>
            {
                new PercentTieredCostModelEntryAdjuster(-15.1),
                new RoundingTieredCostModelEntryAdjuster(5.0),
                new MinimumTieredCostModelEntryAdjuster(11.2),
            },
            new List<string>(),
            3.5,
            5793.0);
    }

    public double Run(TieredCostModelEntry item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredCostModelEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
