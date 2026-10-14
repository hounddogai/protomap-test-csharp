namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default headcount draft calculation.</summary>
public interface IDefaultHeadcountDraftFactor
{
    string Describe();
    double Adjust(DefaultHeadcountDraft item, double valueValue);
}

public sealed record RoundingDefaultHeadcountDraftAdjuster(double Step) : IDefaultHeadcountDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultHeadcountDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultHeadcountDraftAdjuster(double Percent) : IDefaultHeadcountDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultHeadcountDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDefaultHeadcountDraftAdjuster(List<double> Bounds, List<double> Rates) : IDefaultHeadcountDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultHeadcountDraft item, double valueValue)
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

public sealed record CapDefaultHeadcountDraftAdjuster(double Cap) : IDefaultHeadcountDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultHeadcountDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the default headcount draft steps in order and keeps a trace of each.</summary>
public sealed class DefaultHeadcountDraftEngine(List<IDefaultHeadcountDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultHeadcountDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultHeadcountDraftEngine DefaultDefaultHeadcountDraftEngine()
    {
        return new DefaultHeadcountDraftEngine(
            new List<IDefaultHeadcountDraftFactor>
            {
                new RoundingDefaultHeadcountDraftAdjuster(0.05),
                new PercentDefaultHeadcountDraftAdjuster(-20.4),
                new TieredDefaultHeadcountDraftAdjuster(
                    new List<double> { 983.0, 3297.0, 7861.0, 9218.0 },
                    new List<double> { 0.84, 1.06, 1.02, 1.03 }),
                new CapDefaultHeadcountDraftAdjuster(1602.0),
            },
            new List<string>(),
            3.5,
            31000.2);
    }

    public double Compute(DefaultHeadcountDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultHeadcountDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
