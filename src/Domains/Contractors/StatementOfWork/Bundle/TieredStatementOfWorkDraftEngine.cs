namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered statement of work draft calculation.</summary>
public interface ITieredStatementOfWorkDraftStep
{
    string Describe();
    double Adjust(TieredStatementOfWorkDraft item, double valueValue);
}

public sealed record TieredTieredStatementOfWorkDraftAdjuster(List<double> Bounds, List<double> Rates) : ITieredStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredStatementOfWorkDraft item, double valueValue)
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

public sealed record FlaggedTieredStatementOfWorkDraftAdjuster(Registry Flags, string Flag, double Percent) : ITieredStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredStatementOfWorkDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredStatementOfWorkDraftAdjuster(double Step) : ITieredStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredStatementOfWorkDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStatementOfWorkDraftRetriesOffsetAdjuster(double Weight) : ITieredStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(TieredStatementOfWorkDraft item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record PercentTieredStatementOfWorkDraftAdjuster(double Percent) : ITieredStatementOfWorkDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredStatementOfWorkDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered statement of work draft steps in order and keeps a trace of each.</summary>
public sealed class TieredStatementOfWorkDraftEngine(List<ITieredStatementOfWorkDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredStatementOfWorkDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredStatementOfWorkDraftEngine DefaultTieredStatementOfWorkDraftEngine(Registry flags)
    {
        return new TieredStatementOfWorkDraftEngine(
            new List<ITieredStatementOfWorkDraftStep>
            {
                new TieredTieredStatementOfWorkDraftAdjuster(
                    new List<double> { 3142.0, 8298.0 },
                    new List<double> { 0.86, 1.1 }),
                new FlaggedTieredStatementOfWorkDraftAdjuster(flags, "tiered-statement-of-work-draft-pending", -8.9),
                new RoundingTieredStatementOfWorkDraftAdjuster(0.1),
                new TieredStatementOfWorkDraftRetriesOffsetAdjuster(1.0),
                new PercentTieredStatementOfWorkDraftAdjuster(1.8),
            },
            new List<string>(),
            3.1,
            86124.1);
    }

    public double Price(TieredStatementOfWorkDraft item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredStatementOfWorkDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
