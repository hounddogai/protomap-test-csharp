namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped desk adjustment calculation.</summary>
public interface ICappedDeskAdjustmentModifier
{
    string Describe();
    double Adjust(CappedDeskAdjustment item, double valueValue);
}

public sealed record TieredCappedDeskAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ICappedDeskAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedDeskAdjustment item, double valueValue)
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

public sealed record FlaggedCappedDeskAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : ICappedDeskAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedDeskAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedDeskAdjustmentAdjuster(double Percent) : ICappedDeskAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedDeskAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped desk adjustment steps in order and keeps a trace of each.</summary>
public sealed class CappedDeskAdjustmentEngine(List<ICappedDeskAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedDeskAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedDeskAdjustmentEngine DefaultCappedDeskAdjustmentEngine(Registry flags)
    {
        return new CappedDeskAdjustmentEngine(
            new List<ICappedDeskAdjustmentModifier>
            {
                new TieredCappedDeskAdjustmentAdjuster(
                    new List<double> { 1048.0, 5123.0, 9968.0 },
                    new List<double> { 1.13, 1.12, 1.03 }),
                new FlaggedCappedDeskAdjustmentAdjuster(flags, "capped-desk-adjustment-primary", 3.0),
                new PercentCappedDeskAdjustmentAdjuster(19.1),
            },
            new List<string>(),
            4.5,
            16403.8);
    }

    public double Evaluate(CappedDeskAdjustment item)
    {
        var current = item.ProjectedLoad();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CappedDeskAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
