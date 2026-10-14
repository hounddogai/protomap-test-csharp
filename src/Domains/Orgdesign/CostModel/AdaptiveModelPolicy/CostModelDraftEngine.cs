namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the cost model draft calculation.</summary>
public interface ICostModelDraftModifier
{
    string Describe();
    double Adjust(CostModelDraft item, double valueValue);
}

public sealed record CapCostModelDraftAdjuster(double Cap) : ICostModelDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCostModelDraftAdjuster(double Percent) : ICostModelDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCostModelDraftAdjuster(Registry Flags, string Flag, double Percent) : ICostModelDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCostModelDraftAdjuster(List<double> Bounds, List<double> Rates) : ICostModelDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelDraft item, double valueValue)
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

public sealed record MinimumCostModelDraftAdjuster(double Minimum) : ICostModelDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the cost model draft steps in order and keeps a trace of each.</summary>
public sealed class CostModelDraftEngine(List<ICostModelDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelDraftEngine DefaultCostModelDraftEngine(Registry flags)
    {
        return new CostModelDraftEngine(
            new List<ICostModelDraftModifier>
            {
                new CapCostModelDraftAdjuster(29358.0),
                new PercentCostModelDraftAdjuster(7.6),
                new FlaggedCostModelDraftAdjuster(flags, "cost-model-draft-global", 0.2),
                new TieredCostModelDraftAdjuster(
                    new List<double> { 368.0, 612.0, 9401.0, 9592.0 },
                    new List<double> { 1.03, 0.92, 0.91, 0.86 }),
                new MinimumCostModelDraftAdjuster(29.2),
            },
            new List<string>(),
            3.9,
            31721.6);
    }

    public double Compute(CostModelDraft item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
