namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span allocation calculation.</summary>
public interface ISpanAllocationAdjuster
{
    string Describe();
    double Adjust(SpanAllocation item, double valueValue);
}

public sealed record PercentSpanAllocationAdjuster(double Percent) : ISpanAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SpanAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSpanAllocationAdjuster(List<double> Bounds, List<double> Rates) : ISpanAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanAllocation item, double valueValue)
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

public sealed record LabelSpanAllocationAdjuster(Dictionary<string, double> Factors) : ISpanAllocationAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SpanAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSpanAllocationAdjuster(double Minimum) : ISpanAllocationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SpanAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSpanAllocationAdjuster(double Step) : ISpanAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SpanAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the span allocation steps in order and keeps a trace of each.</summary>
public sealed class SpanAllocationEngine(List<ISpanAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanAllocationEngine DefaultSpanAllocationEngine()
    {
        return new SpanAllocationEngine(
            new List<ISpanAllocationAdjuster>
            {
                new PercentSpanAllocationAdjuster(-20.2),
                new TieredSpanAllocationAdjuster(
                    new List<double> { 5310.0, 7906.0, 8990.0 },
                    new List<double> { 1.06, 0.89, 0.9 }),
                new LabelSpanAllocationAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.0, ["online"] = 1.03, ["north"] = 1.18 }),
                new MinimumSpanAllocationAdjuster(22.7),
                new RoundingSpanAllocationAdjuster(0.1),
            },
            new List<string>(),
            3.6,
            54228.2);
    }

    public double Compute(SpanAllocation item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SpanAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
