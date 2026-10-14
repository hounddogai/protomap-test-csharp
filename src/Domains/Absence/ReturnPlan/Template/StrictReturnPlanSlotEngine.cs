namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict return plan slot calculation.</summary>
public interface IStrictReturnPlanSlotAdjuster
{
    string Describe();
    double Adjust(StrictReturnPlanSlot item, double valueValue);
}

public sealed record CountryStrictReturnPlanSlotAdjuster(Dictionary<string, double> Factors) : IStrictReturnPlanSlotAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(StrictReturnPlanSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredStrictReturnPlanSlotAdjuster(List<double> Bounds, List<double> Rates) : IStrictReturnPlanSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictReturnPlanSlot item, double valueValue)
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

public sealed record RoundingStrictReturnPlanSlotAdjuster(double Step) : IStrictReturnPlanSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictReturnPlanSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the strict return plan slot steps in order and keeps a trace of each.</summary>
public sealed class StrictReturnPlanSlotEngine(List<IStrictReturnPlanSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictReturnPlanSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictReturnPlanSlotEngine DefaultStrictReturnPlanSlotEngine()
    {
        return new StrictReturnPlanSlotEngine(
            new List<IStrictReturnPlanSlotAdjuster>
            {
                new CountryStrictReturnPlanSlotAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.34, ["central"] = 0.8 }),
                new TieredStrictReturnPlanSlotAdjuster(
                    new List<double> { 2230.0, 4457.0, 7509.0, 9172.0 },
                    new List<double> { 0.97, 0.94, 0.94, 0.91 }),
                new RoundingStrictReturnPlanSlotAdjuster(0.5),
            },
            new List<string>(),
            0.9,
            99235.5);
    }

    public double Price(StrictReturnPlanSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictReturnPlanSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
