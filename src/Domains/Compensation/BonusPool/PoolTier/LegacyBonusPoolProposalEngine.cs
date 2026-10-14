namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy bonus pool proposal calculation.</summary>
public interface ILegacyBonusPoolProposalAdjuster
{
    string Describe();
    double Adjust(LegacyBonusPoolProposal item, double valueValue);
}

public sealed record MinimumLegacyBonusPoolProposalAdjuster(double Minimum) : ILegacyBonusPoolProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyBonusPoolProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionLegacyBonusPoolProposalAdjuster(Dictionary<string, double> Factors) : ILegacyBonusPoolProposalAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LegacyBonusPoolProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLegacyBonusPoolProposalAdjuster(double Cap) : ILegacyBonusPoolProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyBonusPoolProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyBonusPoolProposalAdjuster(List<double> Bounds, List<double> Rates) : ILegacyBonusPoolProposalAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyBonusPoolProposal item, double valueValue)
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

public sealed record RoundingLegacyBonusPoolProposalAdjuster(double Step) : ILegacyBonusPoolProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyBonusPoolProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy bonus pool proposal steps in order and keeps a trace of each.</summary>
public sealed class LegacyBonusPoolProposalEngine(List<ILegacyBonusPoolProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyBonusPoolProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyBonusPoolProposalEngine DefaultLegacyBonusPoolProposalEngine()
    {
        return new LegacyBonusPoolProposalEngine(
            new List<ILegacyBonusPoolProposalAdjuster>
            {
                new MinimumLegacyBonusPoolProposalAdjuster(31.8),
                new RegionLegacyBonusPoolProposalAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.33, ["central"] = 1.33, ["online"] = 1.24 }),
                new CapLegacyBonusPoolProposalAdjuster(6881.0),
                new TieredLegacyBonusPoolProposalAdjuster(
                    new List<double> { 2576.0, 4354.0, 6513.0, 6755.0 },
                    new List<double> { 0.99, 1.11, 1.2, 0.82 }),
                new RoundingLegacyBonusPoolProposalAdjuster(5.0),
            },
            new List<string>(),
            3.5,
            94364.6);
    }

    public double Price(LegacyBonusPoolProposal item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyBonusPoolProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
