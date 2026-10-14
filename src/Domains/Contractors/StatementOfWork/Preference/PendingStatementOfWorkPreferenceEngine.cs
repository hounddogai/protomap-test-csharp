namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending statement of work preference calculation.</summary>
public interface IPendingStatementOfWorkPreferenceStep
{
    string Describe();
    double Adjust(PendingStatementOfWorkPreference item, double valueValue);
}

public sealed record PercentPendingStatementOfWorkPreferenceAdjuster(double Percent) : IPendingStatementOfWorkPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingStatementOfWorkPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPendingStatementOfWorkPreferenceAdjuster(double Cap) : IPendingStatementOfWorkPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingStatementOfWorkPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingStatementOfWorkPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IPendingStatementOfWorkPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingStatementOfWorkPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingStatementOfWorkPreferenceAdjuster(double Step) : IPendingStatementOfWorkPreferenceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingStatementOfWorkPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPendingStatementOfWorkPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPendingStatementOfWorkPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingStatementOfWorkPreference item, double valueValue)
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

/// <summary>Runs the pending statement of work preference steps in order and keeps a trace of each.</summary>
public sealed class PendingStatementOfWorkPreferenceEngine(List<IPendingStatementOfWorkPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingStatementOfWorkPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingStatementOfWorkPreferenceEngine DefaultPendingStatementOfWorkPreferenceEngine(Registry flags)
    {
        return new PendingStatementOfWorkPreferenceEngine(
            new List<IPendingStatementOfWorkPreferenceStep>
            {
                new PercentPendingStatementOfWorkPreferenceAdjuster(-8.1),
                new CapPendingStatementOfWorkPreferenceAdjuster(12287.0),
                new FlaggedPendingStatementOfWorkPreferenceAdjuster(
                    flags,
                    "pending-statement-of-work-preference-manual",
                    4.5),
                new RoundingPendingStatementOfWorkPreferenceAdjuster(0.05),
                new TieredPendingStatementOfWorkPreferenceAdjuster(
                    new List<double> { 135.0, 2015.0, 3279.0, 9959.0 },
                    new List<double> { 0.82, 1.12, 0.83, 0.83 }),
            },
            new List<string>(),
            0.5,
            32107.4);
    }

    public double Evaluate(PendingStatementOfWorkPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingStatementOfWorkPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
