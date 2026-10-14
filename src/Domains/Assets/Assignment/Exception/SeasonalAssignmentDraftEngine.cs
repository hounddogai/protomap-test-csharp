namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal assignment draft calculation.</summary>
public interface ISeasonalAssignmentDraftStep
{
    string Describe();
    double Adjust(SeasonalAssignmentDraft item, double valueValue);
}

public sealed record CapSeasonalAssignmentDraftAdjuster(double Cap) : ISeasonalAssignmentDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAssignmentDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameSeasonalAssignmentDraftAdjuster(Dictionary<string, double> Factors) : ISeasonalAssignmentDraftStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalAssignmentDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalAssignmentDraftAdjuster(double Step) : ISeasonalAssignmentDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalAssignmentDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalAssignmentDraftAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalAssignmentDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalAssignmentDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalAssignmentDraftExpiresAtOffsetAdjuster(double Weight) : ISeasonalAssignmentDraftStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(SeasonalAssignmentDraft item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the seasonal assignment draft steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAssignmentDraftEngine(List<ISeasonalAssignmentDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAssignmentDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAssignmentDraftEngine DefaultSeasonalAssignmentDraftEngine(Registry flags)
    {
        return new SeasonalAssignmentDraftEngine(
            new List<ISeasonalAssignmentDraftStep>
            {
                new CapSeasonalAssignmentDraftAdjuster(33151.0),
                new NameSeasonalAssignmentDraftAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.34, ["central"] = 1.13, ["online"] = 0.75 }),
                new RoundingSeasonalAssignmentDraftAdjuster(1.0),
                new FlaggedSeasonalAssignmentDraftAdjuster(flags, "seasonal-assignment-draft-scheduled", 2.2),
                new SeasonalAssignmentDraftExpiresAtOffsetAdjuster(1.45),
            },
            new List<string>(),
            1.9,
            36752.6);
    }

    public double Run(SeasonalAssignmentDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAssignmentDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
