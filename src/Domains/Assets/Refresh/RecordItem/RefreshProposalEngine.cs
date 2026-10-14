namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh proposal calculation.</summary>
public interface IRefreshProposalFactor
{
    string Describe();
    double Adjust(RefreshProposal item, double valueValue);
}

public sealed record PercentRefreshProposalAdjuster(double Percent) : IRefreshProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRefreshProposalAdjuster(double Minimum) : IRefreshProposalFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRefreshProposalAdjuster(Registry Flags, string Flag, double Percent) : IRefreshProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshProposalWindowDaysOffsetAdjuster(double Weight) : IRefreshProposalFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(RefreshProposal item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record CapRefreshProposalAdjuster(double Cap) : IRefreshProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleRefreshProposalAdjuster(Dictionary<string, double> Factors) : IRefreshProposalFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RefreshProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the refresh proposal steps in order and keeps a trace of each.</summary>
public sealed class RefreshProposalEngine(List<IRefreshProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshProposalEngine DefaultRefreshProposalEngine(Registry flags)
    {
        return new RefreshProposalEngine(
            new List<IRefreshProposalFactor>
            {
                new PercentRefreshProposalAdjuster(-23.5),
                new MinimumRefreshProposalAdjuster(12.6),
                new FlaggedRefreshProposalAdjuster(flags, "refresh-proposal-fallback", 8.5),
                new RefreshProposalWindowDaysOffsetAdjuster(1.45),
                new CapRefreshProposalAdjuster(1000.0),
                new TitleRefreshProposalAdjuster(new Dictionary<string, double> { ["west"] = 1.29, ["east"] = 1.09 }),
            },
            new List<string>(),
            4.1,
            51589.2);
    }

    public double Evaluate(RefreshProposal item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
