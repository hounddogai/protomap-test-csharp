namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional return plan item calculation.</summary>
public interface IRegionalReturnPlanItemModifier
{
    string Describe();
    double Adjust(RegionalReturnPlanItem item, double valueValue);
}

public sealed record CapRegionalReturnPlanItemAdjuster(double Cap) : IRegionalReturnPlanItemModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalReturnPlanItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRegionalReturnPlanItemAdjuster(double Minimum) : IRegionalReturnPlanItemModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalReturnPlanItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRegionalReturnPlanItemAdjuster(List<double> Bounds, List<double> Rates) : IRegionalReturnPlanItemModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalReturnPlanItem item, double valueValue)
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

public sealed record RoundingRegionalReturnPlanItemAdjuster(double Step) : IRegionalReturnPlanItemModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalReturnPlanItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional return plan item steps in order and keeps a trace of each.</summary>
public sealed class RegionalReturnPlanItemEngine(List<IRegionalReturnPlanItemModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalReturnPlanItemModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalReturnPlanItemEngine DefaultRegionalReturnPlanItemEngine()
    {
        return new RegionalReturnPlanItemEngine(
            new List<IRegionalReturnPlanItemModifier>
            {
                new CapRegionalReturnPlanItemAdjuster(22918.0),
                new MinimumRegionalReturnPlanItemAdjuster(27.3),
                new TieredRegionalReturnPlanItemAdjuster(
                    new List<double> { 4919.0, 5456.0 },
                    new List<double> { 1.11, 1.16 }),
                new RoundingRegionalReturnPlanItemAdjuster(1.0),
            },
            new List<string>(),
            0.6,
            54733.5);
    }

    public double Evaluate(RegionalReturnPlanItem item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalReturnPlanItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
