namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual action plan slot calculation.</summary>
public interface IManualActionPlanSlotFactor
{
    string Describe();
    double Adjust(ManualActionPlanSlot item, double valueValue);
}

public sealed record PercentManualActionPlanSlotAdjuster(double Percent) : IManualActionPlanSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualActionPlanSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualActionPlanSlotAdjuster(List<double> Bounds, List<double> Rates) : IManualActionPlanSlotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualActionPlanSlot item, double valueValue)
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

public sealed record CapManualActionPlanSlotAdjuster(double Cap) : IManualActionPlanSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualActionPlanSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleManualActionPlanSlotAdjuster(Dictionary<string, double> Factors) : IManualActionPlanSlotFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ManualActionPlanSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual action plan slot steps in order and keeps a trace of each.</summary>
public sealed class ManualActionPlanSlotEngine(List<IManualActionPlanSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualActionPlanSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualActionPlanSlotEngine DefaultManualActionPlanSlotEngine()
    {
        return new ManualActionPlanSlotEngine(
            new List<IManualActionPlanSlotFactor>
            {
                new PercentManualActionPlanSlotAdjuster(-23.7),
                new TieredManualActionPlanSlotAdjuster(
                    new List<double> { 122.0, 1834.0, 3444.0, 7270.0 },
                    new List<double> { 0.88, 0.9, 1.06, 1.17 }),
                new CapManualActionPlanSlotAdjuster(40946.0),
                new LocaleManualActionPlanSlotAdjuster(new Dictionary<string, double> { ["south"] = 0.82 }),
            },
            new List<string>(),
            3.9,
            8771.4);
    }

    public double Evaluate(ManualActionPlanSlot item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualActionPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
