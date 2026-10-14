namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan ledger calculation.</summary>
public interface IActionPlanLedgerStep
{
    string Describe();
    double Adjust(ActionPlanLedger item, double valueValue);
}

public sealed record TieredActionPlanLedgerAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanLedgerStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanLedger item, double valueValue)
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

public sealed record RoundingActionPlanLedgerAdjuster(double Step) : IActionPlanLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedActionPlanLedgerAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ActionPlanLedgerFteOffsetAdjuster(double Weight) : IActionPlanLedgerStep
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(ActionPlanLedger item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TeamActionPlanLedgerAdjuster(Dictionary<string, double> Factors) : IActionPlanLedgerStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ActionPlanLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the action plan ledger steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanLedgerEngine(List<IActionPlanLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanLedgerEngine DefaultActionPlanLedgerEngine(Registry flags)
    {
        return new ActionPlanLedgerEngine(
            new List<IActionPlanLedgerStep>
            {
                new TieredActionPlanLedgerAdjuster(
                    new List<double> { 5758.0, 7458.0 },
                    new List<double> { 0.83, 0.95 }),
                new RoundingActionPlanLedgerAdjuster(1.0),
                new FlaggedActionPlanLedgerAdjuster(flags, "action-plan-ledger-default", -1.2),
                new ActionPlanLedgerFteOffsetAdjuster(0.83),
                new TeamActionPlanLedgerAdjuster(new Dictionary<string, double> { ["north"] = 0.93 }),
            },
            new List<string>(),
            0.6,
            78552.4);
    }

    public double Compute(ActionPlanLedger item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
