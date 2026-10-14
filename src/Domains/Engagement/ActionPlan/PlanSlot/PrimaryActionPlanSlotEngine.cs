namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>One step of the primary action plan slot calculation.</summary>
public interface IPrimaryActionPlanSlotModifier
{
    string Describe();
    double Adjust(PrimaryActionPlanSlot item, double valueValue);
}

public sealed record LocalePrimaryActionPlanSlotAdjuster(Dictionary<string, double> Factors) : IPrimaryActionPlanSlotModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PrimaryActionPlanSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPrimaryActionPlanSlotAdjuster(double Minimum) : IPrimaryActionPlanSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryActionPlanSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPrimaryActionPlanSlotAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryActionPlanSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryActionPlanSlot item, double valueValue)
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

/// <summary>Runs the primary action plan slot steps in order and keeps a trace of each.</summary>
public sealed class PrimaryActionPlanSlotEngine(List<IPrimaryActionPlanSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryActionPlanSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryActionPlanSlotEngine DefaultPrimaryActionPlanSlotEngine()
    {
        return new PrimaryActionPlanSlotEngine(
            new List<IPrimaryActionPlanSlotModifier>
            {
                new LocalePrimaryActionPlanSlotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.89, ["west"] = 1.3, ["central"] = 1.21 }),
                new MinimumPrimaryActionPlanSlotAdjuster(24.1),
                new TieredPrimaryActionPlanSlotAdjuster(
                    new List<double> { 1247.0, 5636.0 },
                    new List<double> { 0.86, 1.11 }),
            },
            new List<string>(),
            3.2,
            25525.0);
    }

    public double Evaluate(PrimaryActionPlanSlot item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryActionPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
