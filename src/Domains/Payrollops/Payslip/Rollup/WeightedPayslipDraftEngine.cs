namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted payslip draft calculation.</summary>
public interface IWeightedPayslipDraftModifier
{
    string Describe();
    double Adjust(WeightedPayslipDraft item, double valueValue);
}

public sealed record CapWeightedPayslipDraftAdjuster(double Cap) : IWeightedPayslipDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedPayslipDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedPayslipDraftAdjuster(double Percent) : IWeightedPayslipDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPayslipDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record WeightedPayslipDraftBudgetOffsetAdjuster(double Weight) : IWeightedPayslipDraftModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(WeightedPayslipDraft item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingWeightedPayslipDraftAdjuster(double Step) : IWeightedPayslipDraftModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedPayslipDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedWeightedPayslipDraftAdjuster(Registry Flags, string Flag, double Percent) : IWeightedPayslipDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedPayslipDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedPayslipDraftAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPayslipDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPayslipDraft item, double valueValue)
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

/// <summary>Runs the weighted payslip draft steps in order and keeps a trace of each.</summary>
public sealed class WeightedPayslipDraftEngine(List<IWeightedPayslipDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPayslipDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPayslipDraftEngine DefaultWeightedPayslipDraftEngine(Registry flags)
    {
        return new WeightedPayslipDraftEngine(
            new List<IWeightedPayslipDraftModifier>
            {
                new CapWeightedPayslipDraftAdjuster(28121.0),
                new PercentWeightedPayslipDraftAdjuster(-1.3),
                new WeightedPayslipDraftBudgetOffsetAdjuster(1.09),
                new RoundingWeightedPayslipDraftAdjuster(1.0),
                new FlaggedWeightedPayslipDraftAdjuster(flags, "weighted-payslip-draft-global", 5.0),
                new TieredWeightedPayslipDraftAdjuster(
                    new List<double> { 3050.0, 4252.0, 5692.0, 7587.0 },
                    new List<double> { 0.82, 1.0, 0.86, 0.99 }),
            },
            new List<string>(),
            1.3,
            70965.3);
    }

    public double Evaluate(WeightedPayslipDraft item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPayslipDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
