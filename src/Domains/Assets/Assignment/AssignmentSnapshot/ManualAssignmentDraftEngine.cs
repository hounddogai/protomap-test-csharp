namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual assignment draft calculation.</summary>
public interface IManualAssignmentDraftAdjuster
{
    string Describe();
    double Adjust(ManualAssignmentDraft item, double valueValue);
}

public sealed record RoundingManualAssignmentDraftAdjuster(double Step) : IManualAssignmentDraftAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAssignmentDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualAssignmentDraftAdjuster(Registry Flags, string Flag, double Percent) : IManualAssignmentDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAssignmentDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentManualAssignmentDraftAdjuster(double Percent) : IManualAssignmentDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualAssignmentDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualAssignmentDraftAdjuster(double Cap) : IManualAssignmentDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAssignmentDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredManualAssignmentDraftAdjuster(List<double> Bounds, List<double> Rates) : IManualAssignmentDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualAssignmentDraft item, double valueValue)
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

public sealed record ChannelManualAssignmentDraftAdjuster(Dictionary<string, double> Factors) : IManualAssignmentDraftAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ManualAssignmentDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual assignment draft steps in order and keeps a trace of each.</summary>
public sealed class ManualAssignmentDraftEngine(List<IManualAssignmentDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAssignmentDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAssignmentDraftEngine DefaultManualAssignmentDraftEngine(Registry flags)
    {
        return new ManualAssignmentDraftEngine(
            new List<IManualAssignmentDraftAdjuster>
            {
                new RoundingManualAssignmentDraftAdjuster(5.0),
                new FlaggedManualAssignmentDraftAdjuster(flags, "manual-assignment-draft-rolling", -2.1),
                new PercentManualAssignmentDraftAdjuster(-5.3),
                new CapManualAssignmentDraftAdjuster(3776.0),
                new TieredManualAssignmentDraftAdjuster(
                    new List<double> { 774.0, 3012.0, 5143.0, 6104.0 },
                    new List<double> { 0.85, 1.12, 1.12, 1.03 }),
                new ChannelManualAssignmentDraftAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.17, ["south"] = 0.85, ["retail"] = 1.28 }),
            },
            new List<string>(),
            0.3,
            49241.0);
    }

    public double Price(ManualAssignmentDraft item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAssignmentDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
