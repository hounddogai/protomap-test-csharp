namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global equity grant draft calculation.</summary>
public interface IGlobalEquityGrantDraftFactor
{
    string Describe();
    double Adjust(GlobalEquityGrantDraft item, double valueValue);
}

public sealed record RegionGlobalEquityGrantDraftAdjuster(Dictionary<string, double> Factors) : IGlobalEquityGrantDraftFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(GlobalEquityGrantDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalEquityGrantDraftAdjuster(double Minimum) : IGlobalEquityGrantDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalEquityGrantDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalEquityGrantDraftAdjuster(double Cap) : IGlobalEquityGrantDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalEquityGrantDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record GlobalEquityGrantDraftRetriesOffsetAdjuster(double Weight) : IGlobalEquityGrantDraftFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(GlobalEquityGrantDraft item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedGlobalEquityGrantDraftAdjuster(Registry Flags, string Flag, double Percent) : IGlobalEquityGrantDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalEquityGrantDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalEquityGrantDraftAdjuster(double Step) : IGlobalEquityGrantDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalEquityGrantDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global equity grant draft steps in order and keeps a trace of each.</summary>
public sealed class GlobalEquityGrantDraftEngine(List<IGlobalEquityGrantDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalEquityGrantDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalEquityGrantDraftEngine DefaultGlobalEquityGrantDraftEngine(Registry flags)
    {
        return new GlobalEquityGrantDraftEngine(
            new List<IGlobalEquityGrantDraftFactor>
            {
                new RegionGlobalEquityGrantDraftAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.22, ["south"] = 0.98, ["west"] = 1.12 }),
                new MinimumGlobalEquityGrantDraftAdjuster(2.3),
                new CapGlobalEquityGrantDraftAdjuster(15334.0),
                new GlobalEquityGrantDraftRetriesOffsetAdjuster(0.62),
                new FlaggedGlobalEquityGrantDraftAdjuster(flags, "global-equity-grant-draft-seasonal", 0.9),
                new RoundingGlobalEquityGrantDraftAdjuster(0.05),
            },
            new List<string>(),
            1.2,
            70646.6);
    }

    public double Price(GlobalEquityGrantDraft item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalEquityGrantDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
