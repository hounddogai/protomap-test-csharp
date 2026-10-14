namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual merit cycle proposal calculation.</summary>
public interface IManualMeritCycleProposalFactor
{
    string Describe();
    double Adjust(ManualMeritCycleProposal item, double valueValue);
}

public sealed record PercentManualMeritCycleProposalAdjuster(double Percent) : IManualMeritCycleProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualMeritCycleProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualMeritCycleProposalAdjuster(double Cap) : IManualMeritCycleProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualMeritCycleProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryManualMeritCycleProposalAdjuster(Dictionary<string, double> Factors) : IManualMeritCycleProposalFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ManualMeritCycleProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedManualMeritCycleProposalAdjuster(Registry Flags, string Flag, double Percent) : IManualMeritCycleProposalFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualMeritCycleProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual merit cycle proposal steps in order and keeps a trace of each.</summary>
public sealed class ManualMeritCycleProposalEngine(List<IManualMeritCycleProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualMeritCycleProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualMeritCycleProposalEngine DefaultManualMeritCycleProposalEngine(Registry flags)
    {
        return new ManualMeritCycleProposalEngine(
            new List<IManualMeritCycleProposalFactor>
            {
                new PercentManualMeritCycleProposalAdjuster(-21.9),
                new CapManualMeritCycleProposalAdjuster(22443.0),
                new CountryManualMeritCycleProposalAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.18, ["north"] = 0.88, ["east"] = 0.97 }),
                new FlaggedManualMeritCycleProposalAdjuster(flags, "manual-merit-cycle-proposal-legacy", -1.3),
            },
            new List<string>(),
            0.1,
            76237.4);
    }

    public double Run(ManualMeritCycleProposal item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualMeritCycleProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
