namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the equity grant draft calculation.</summary>
public interface IEquityGrantDraftAdjuster
{
    string Describe();
    double Adjust(EquityGrantDraft item, double valueValue);
}

public sealed record CapEquityGrantDraftAdjuster(double Cap) : IEquityGrantDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EquityGrantDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentEquityGrantDraftAdjuster(double Percent) : IEquityGrantDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EquityGrantDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingEquityGrantDraftAdjuster(double Step) : IEquityGrantDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EquityGrantDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleEquityGrantDraftAdjuster(Dictionary<string, double> Factors) : IEquityGrantDraftAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(EquityGrantDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEquityGrantDraftAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantDraft item, double valueValue)
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

/// <summary>Runs the equity grant draft steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantDraftEngine(List<IEquityGrantDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantDraftEngine DefaultEquityGrantDraftEngine()
    {
        return new EquityGrantDraftEngine(
            new List<IEquityGrantDraftAdjuster>
            {
                new CapEquityGrantDraftAdjuster(7591.0),
                new PercentEquityGrantDraftAdjuster(-12.2),
                new RoundingEquityGrantDraftAdjuster(0.25),
                new TitleEquityGrantDraftAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.35, ["retail"] = 0.93 }),
                new TieredEquityGrantDraftAdjuster(
                    new List<double> { 3873.0, 4221.0, 5927.0, 9024.0 },
                    new List<double> { 0.93, 1.17, 0.86, 0.99 }),
            },
            new List<string>(),
            0.2,
            62172.8);
    }

    public double Price(EquityGrantDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
