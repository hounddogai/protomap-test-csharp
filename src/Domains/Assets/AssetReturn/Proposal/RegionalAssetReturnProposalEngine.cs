namespace Acme.Workforce.Assets.AssetReturn.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional asset return proposal calculation.</summary>
public interface IRegionalAssetReturnProposalAdjuster
{
    string Describe();
    double Adjust(RegionalAssetReturnProposal item, double valueValue);
}

public sealed record FlaggedRegionalAssetReturnProposalAdjuster(Registry Flags, string Flag, double Percent) : IRegionalAssetReturnProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalAssetReturnProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalAssetReturnProposalAdjuster(double Cap) : IRegionalAssetReturnProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalAssetReturnProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalAssetReturnProposalAdjuster(double Percent) : IRegionalAssetReturnProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalAssetReturnProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRegionalAssetReturnProposalAdjuster(Dictionary<string, double> Factors) : IRegionalAssetReturnProposalAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RegionalAssetReturnProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumRegionalAssetReturnProposalAdjuster(double Minimum) : IRegionalAssetReturnProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalAssetReturnProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional asset return proposal steps in order and keeps a trace of each.</summary>
public sealed class RegionalAssetReturnProposalEngine(List<IRegionalAssetReturnProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAssetReturnProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAssetReturnProposalEngine DefaultRegionalAssetReturnProposalEngine(Registry flags)
    {
        return new RegionalAssetReturnProposalEngine(
            new List<IRegionalAssetReturnProposalAdjuster>
            {
                new FlaggedRegionalAssetReturnProposalAdjuster(flags, "regional-asset-return-proposal-weighted", 0.5),
                new CapRegionalAssetReturnProposalAdjuster(8799.0),
                new PercentRegionalAssetReturnProposalAdjuster(-1.6),
                new TitleRegionalAssetReturnProposalAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.89, ["north"] = 0.96 }),
                new MinimumRegionalAssetReturnProposalAdjuster(21.5),
            },
            new List<string>(),
            0.3,
            25846.2);
    }

    public double Evaluate(RegionalAssetReturnProposal item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAssetReturnProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
