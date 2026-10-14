namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor draft calculation.</summary>
public interface IVisitorDraftModifier
{
    string Describe();
    double Adjust(VisitorDraft item, double valueValue);
}

public sealed record TieredVisitorDraftAdjuster(List<double> Bounds, List<double> Rates) : IVisitorDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorDraft item, double valueValue)
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

public sealed record PercentVisitorDraftAdjuster(double Percent) : IVisitorDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedVisitorDraftAdjuster(Registry Flags, string Flag, double Percent) : IVisitorDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapVisitorDraftAdjuster(double Cap) : IVisitorDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(VisitorDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record VisitorDraftRatioOffsetAdjuster(double Weight) : IVisitorDraftModifier
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(VisitorDraft item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record MinimumVisitorDraftAdjuster(double Minimum) : IVisitorDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the visitor draft steps in order and keeps a trace of each.</summary>
public sealed class VisitorDraftEngine(List<IVisitorDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorDraftEngine DefaultVisitorDraftEngine(Registry flags)
    {
        return new VisitorDraftEngine(
            new List<IVisitorDraftModifier>
            {
                new TieredVisitorDraftAdjuster(new List<double> { 5195.0, 8715.0 }, new List<double> { 0.82, 1.14 }),
                new PercentVisitorDraftAdjuster(-16.1),
                new FlaggedVisitorDraftAdjuster(flags, "visitor-draft-rolling", -5.2),
                new CapVisitorDraftAdjuster(43585.0),
                new VisitorDraftRatioOffsetAdjuster(1.11),
                new MinimumVisitorDraftAdjuster(3.3),
            },
            new List<string>(),
            0.2,
            15073.8);
    }

    public double Compute(VisitorDraft item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
