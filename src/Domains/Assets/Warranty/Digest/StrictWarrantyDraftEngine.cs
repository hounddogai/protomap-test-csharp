namespace Acme.Workforce.Assets.Warranty.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict warranty draft calculation.</summary>
public interface IStrictWarrantyDraftFactor
{
    string Describe();
    double Adjust(StrictWarrantyDraft item, double valueValue);
}

public sealed record RoundingStrictWarrantyDraftAdjuster(double Step) : IStrictWarrantyDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictWarrantyDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictWarrantyDraftAdjuster(double Minimum) : IStrictWarrantyDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictWarrantyDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapStrictWarrantyDraftAdjuster(double Cap) : IStrictWarrantyDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictWarrantyDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictWarrantyDraftAdjuster(List<double> Bounds, List<double> Rates) : IStrictWarrantyDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictWarrantyDraft item, double valueValue)
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

public sealed record StrictWarrantyDraftLeaveDaysOffsetAdjuster(double Weight) : IStrictWarrantyDraftFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(StrictWarrantyDraft item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record SourceStrictWarrantyDraftAdjuster(Dictionary<string, double> Factors) : IStrictWarrantyDraftFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(StrictWarrantyDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict warranty draft steps in order and keeps a trace of each.</summary>
public sealed class StrictWarrantyDraftEngine(List<IStrictWarrantyDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictWarrantyDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictWarrantyDraftEngine DefaultStrictWarrantyDraftEngine()
    {
        return new StrictWarrantyDraftEngine(
            new List<IStrictWarrantyDraftFactor>
            {
                new RoundingStrictWarrantyDraftAdjuster(0.1),
                new MinimumStrictWarrantyDraftAdjuster(48.9),
                new CapStrictWarrantyDraftAdjuster(40382.0),
                new TieredStrictWarrantyDraftAdjuster(
                    new List<double> { 571.0, 5333.0, 7458.0 },
                    new List<double> { 0.99, 1.2, 0.9 }),
                new StrictWarrantyDraftLeaveDaysOffsetAdjuster(1.88),
                new SourceStrictWarrantyDraftAdjuster(new Dictionary<string, double> { ["north"] = 0.86 }),
            },
            new List<string>(),
            2.6,
            54615.3);
    }

    public double Price(StrictWarrantyDraft item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictWarrantyDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
