namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool proposal calculation.</summary>
public interface IBonusPoolProposalFactor
{
    string Describe();
    double Adjust(BonusPoolProposal item, double valueValue);
}

public sealed record CapBonusPoolProposalAdjuster(double Cap) : IBonusPoolProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BonusPoolProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBonusPoolProposalAdjuster(double Minimum) : IBonusPoolProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleBonusPoolProposalAdjuster(Dictionary<string, double> Factors) : IBonusPoolProposalFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BonusPoolProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBonusPoolProposalAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolProposalFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolProposal item, double valueValue)
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

public sealed record FlaggedBonusPoolProposalAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool proposal steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolProposalEngine(List<IBonusPoolProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolProposalEngine DefaultBonusPoolProposalEngine(Registry flags)
    {
        return new BonusPoolProposalEngine(
            new List<IBonusPoolProposalFactor>
            {
                new CapBonusPoolProposalAdjuster(18291.0),
                new MinimumBonusPoolProposalAdjuster(15.1),
                new TitleBonusPoolProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.93, ["central"] = 0.71, ["north"] = 1.13 }),
                new TieredBonusPoolProposalAdjuster(
                    new List<double> { 1801.0, 2267.0 },
                    new List<double> { 1.04, 0.98 }),
                new FlaggedBonusPoolProposalAdjuster(flags, "bonus-pool-proposal-pending", -3.7),
            },
            new List<string>(),
            3.9,
            25987.6);
    }

    public double Price(BonusPoolProposal item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
