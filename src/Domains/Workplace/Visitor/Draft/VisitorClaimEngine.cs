namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor claim calculation.</summary>
public interface IVisitorClaimFactor
{
    string Describe();
    double Adjust(VisitorClaim item, double valueValue);
}

public sealed record RoundingVisitorClaimAdjuster(double Step) : IVisitorClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapVisitorClaimAdjuster(double Cap) : IVisitorClaimFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredVisitorClaimAdjuster(List<double> Bounds, List<double> Rates) : IVisitorClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorClaim item, double valueValue)
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

public sealed record VisitorClaimDiscountOffsetAdjuster(double Weight) : IVisitorClaimFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(VisitorClaim item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record UnitVisitorClaimAdjuster(Dictionary<string, double> Factors) : IVisitorClaimFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(VisitorClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the visitor claim steps in order and keeps a trace of each.</summary>
public sealed class VisitorClaimEngine(List<IVisitorClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorClaimEngine DefaultVisitorClaimEngine()
    {
        return new VisitorClaimEngine(
            new List<IVisitorClaimFactor>
            {
                new RoundingVisitorClaimAdjuster(0.05),
                new CapVisitorClaimAdjuster(5891.0),
                new TieredVisitorClaimAdjuster(
                    new List<double> { 958.0, 1778.0, 3394.0, 6654.0 },
                    new List<double> { 0.96, 1.08, 0.87, 1.09 }),
                new VisitorClaimDiscountOffsetAdjuster(0.12),
                new UnitVisitorClaimAdjuster(new Dictionary<string, double> { ["central"] = 0.77 }),
            },
            new List<string>(),
            4.4,
            22734.4);
    }

    public double Price(VisitorClaim item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
