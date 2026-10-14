namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive garnishment allocation calculation.</summary>
public interface IAdaptiveGarnishmentAllocationModifier
{
    string Describe();
    double Adjust(AdaptiveGarnishmentAllocation item, double valueValue);
}

public sealed record TieredAdaptiveGarnishmentAllocationAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveGarnishmentAllocation item, double valueValue)
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

public sealed record CategoryAdaptiveGarnishmentAllocationAdjuster(Dictionary<string, double> Factors) : IAdaptiveGarnishmentAllocationModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AdaptiveGarnishmentAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveGarnishmentAllocationAdjuster(double Step) : IAdaptiveGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveGarnishmentAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveGarnishmentAllocationAdjuster(double Cap) : IAdaptiveGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveGarnishmentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive garnishment allocation steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveGarnishmentAllocationEngine(List<IAdaptiveGarnishmentAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveGarnishmentAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveGarnishmentAllocationEngine DefaultAdaptiveGarnishmentAllocationEngine()
    {
        return new AdaptiveGarnishmentAllocationEngine(
            new List<IAdaptiveGarnishmentAllocationModifier>
            {
                new TieredAdaptiveGarnishmentAllocationAdjuster(
                    new List<double> { 1189.0, 1486.0, 7571.0 },
                    new List<double> { 1.09, 1.17, 1.14 }),
                new CategoryAdaptiveGarnishmentAllocationAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.14, ["west"] = 0.9 }),
                new RoundingAdaptiveGarnishmentAllocationAdjuster(1.0),
                new CapAdaptiveGarnishmentAllocationAdjuster(24258.0),
            },
            new List<string>(),
            4.2,
            4407.3);
    }

    public double Evaluate(AdaptiveGarnishmentAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveGarnishmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
