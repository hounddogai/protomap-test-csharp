namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy floor plan notice calculation.</summary>
public interface ILegacyFloorPlanNoticeModifier
{
    string Describe();
    double Adjust(LegacyFloorPlanNotice item, double valueValue);
}

public sealed record RoundingLegacyFloorPlanNoticeAdjuster(double Step) : ILegacyFloorPlanNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyFloorPlanNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleLegacyFloorPlanNoticeAdjuster(Dictionary<string, double> Factors) : ILegacyFloorPlanNoticeModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyFloorPlanNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyFloorPlanNoticeAdjuster(Registry Flags, string Flag, double Percent) : ILegacyFloorPlanNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyFloorPlanNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyFloorPlanNoticeAdjuster(double Minimum) : ILegacyFloorPlanNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyFloorPlanNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLegacyFloorPlanNoticeAdjuster(List<double> Bounds, List<double> Rates) : ILegacyFloorPlanNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyFloorPlanNotice item, double valueValue)
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

/// <summary>Runs the legacy floor plan notice steps in order and keeps a trace of each.</summary>
public sealed class LegacyFloorPlanNoticeEngine(List<ILegacyFloorPlanNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyFloorPlanNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyFloorPlanNoticeEngine DefaultLegacyFloorPlanNoticeEngine(Registry flags)
    {
        return new LegacyFloorPlanNoticeEngine(
            new List<ILegacyFloorPlanNoticeModifier>
            {
                new RoundingLegacyFloorPlanNoticeAdjuster(1.0),
                new TitleLegacyFloorPlanNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.97, ["west"] = 0.9, ["retail"] = 0.81 }),
                new FlaggedLegacyFloorPlanNoticeAdjuster(flags, "legacy-floor-plan-notice-seasonal", 1.1),
                new MinimumLegacyFloorPlanNoticeAdjuster(11.3),
                new TieredLegacyFloorPlanNoticeAdjuster(
                    new List<double> { 1589.0, 2897.0, 4977.0, 9148.0 },
                    new List<double> { 1.19, 1.08, 1.02, 1.13 }),
            },
            new List<string>(),
            3.2,
            18342.4);
    }

    public double Compute(LegacyFloorPlanNotice item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyFloorPlanNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
