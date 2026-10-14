namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional succession plan draft calculation.</summary>
public interface IRegionalSuccessionPlanDraftFactor
{
    string Describe();
    double Adjust(RegionalSuccessionPlanDraft item, double valueValue);
}

public sealed record FlaggedRegionalSuccessionPlanDraftAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSuccessionPlanDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSuccessionPlanDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalSuccessionPlanDraftAdjuster(double Minimum) : IRegionalSuccessionPlanDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSuccessionPlanDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalSuccessionPlanDraftAdjuster(double Cap) : IRegionalSuccessionPlanDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalSuccessionPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalSuccessionPlanDraftAdjuster(double Step) : IRegionalSuccessionPlanDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSuccessionPlanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionalSuccessionPlanDraftAmountOffsetAdjuster(double Weight) : IRegionalSuccessionPlanDraftFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RegionalSuccessionPlanDraft item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredRegionalSuccessionPlanDraftAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSuccessionPlanDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSuccessionPlanDraft item, double valueValue)
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

/// <summary>Runs the regional succession plan draft steps in order and keeps a trace of each.</summary>
public sealed class RegionalSuccessionPlanDraftEngine(List<IRegionalSuccessionPlanDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSuccessionPlanDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSuccessionPlanDraftEngine DefaultRegionalSuccessionPlanDraftEngine(Registry flags)
    {
        return new RegionalSuccessionPlanDraftEngine(
            new List<IRegionalSuccessionPlanDraftFactor>
            {
                new FlaggedRegionalSuccessionPlanDraftAdjuster(flags, "regional-succession-plan-draft-weighted", 1.1),
                new MinimumRegionalSuccessionPlanDraftAdjuster(39.1),
                new CapRegionalSuccessionPlanDraftAdjuster(6612.0),
                new RoundingRegionalSuccessionPlanDraftAdjuster(0.1),
                new RegionalSuccessionPlanDraftAmountOffsetAdjuster(0.61),
                new TieredRegionalSuccessionPlanDraftAdjuster(
                    new List<double> { 6379.0, 7032.0 },
                    new List<double> { 0.89, 1.03 }),
            },
            new List<string>(),
            3.1,
            3336.9);
    }

    public double Price(RegionalSuccessionPlanDraft item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSuccessionPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
