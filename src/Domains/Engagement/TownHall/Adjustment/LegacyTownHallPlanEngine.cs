namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy town hall plan calculation.</summary>
public interface ILegacyTownHallPlanStep
{
    string Describe();
    double Adjust(LegacyTownHallPlan item, double valueValue);
}

public sealed record LegacyTownHallPlanRevisionOffsetAdjuster(double Weight) : ILegacyTownHallPlanStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(LegacyTownHallPlan item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record NameLegacyTownHallPlanAdjuster(Dictionary<string, double> Factors) : ILegacyTownHallPlanStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LegacyTownHallPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyTownHallPlanAdjuster(double Percent) : ILegacyTownHallPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyTownHallPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyTownHallPlanAdjuster(double Step) : ILegacyTownHallPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyTownHallPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyTownHallPlanAdjuster(List<double> Bounds, List<double> Rates) : ILegacyTownHallPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyTownHallPlan item, double valueValue)
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

public sealed record FlaggedLegacyTownHallPlanAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTownHallPlanStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTownHallPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy town hall plan steps in order and keeps a trace of each.</summary>
public sealed class LegacyTownHallPlanEngine(List<ILegacyTownHallPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTownHallPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTownHallPlanEngine DefaultLegacyTownHallPlanEngine(Registry flags)
    {
        return new LegacyTownHallPlanEngine(
            new List<ILegacyTownHallPlanStep>
            {
                new LegacyTownHallPlanRevisionOffsetAdjuster(0.33),
                new NameLegacyTownHallPlanAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.33, ["online"] = 1.24, ["north"] = 1.25 }),
                new PercentLegacyTownHallPlanAdjuster(-4.7),
                new RoundingLegacyTownHallPlanAdjuster(0.05),
                new TieredLegacyTownHallPlanAdjuster(
                    new List<double> { 2993.0, 3910.0, 5434.0, 7880.0 },
                    new List<double> { 1.05, 0.98, 1.16, 1.15 }),
                new FlaggedLegacyTownHallPlanAdjuster(flags, "legacy-town-hall-plan-default", -2.1),
            },
            new List<string>(),
            0.7,
            20239.1);
    }

    public double Evaluate(LegacyTownHallPlan item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTownHallPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
