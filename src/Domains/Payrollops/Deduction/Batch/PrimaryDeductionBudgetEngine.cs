namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary deduction budget calculation.</summary>
public interface IPrimaryDeductionBudgetStep
{
    string Describe();
    double Adjust(PrimaryDeductionBudget item, double valueValue);
}

public sealed record FlaggedPrimaryDeductionBudgetAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryDeductionBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryDeductionBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryDeductionBudgetAdjuster(double Minimum) : IPrimaryDeductionBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryDeductionBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPrimaryDeductionBudgetAdjuster(double Cap) : IPrimaryDeductionBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryDeductionBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary deduction budget steps in order and keeps a trace of each.</summary>
public sealed class PrimaryDeductionBudgetEngine(List<IPrimaryDeductionBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryDeductionBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryDeductionBudgetEngine DefaultPrimaryDeductionBudgetEngine(Registry flags)
    {
        return new PrimaryDeductionBudgetEngine(
            new List<IPrimaryDeductionBudgetStep>
            {
                new FlaggedPrimaryDeductionBudgetAdjuster(flags, "primary-deduction-budget-manual", 6.6),
                new MinimumPrimaryDeductionBudgetAdjuster(0.2),
                new CapPrimaryDeductionBudgetAdjuster(39020.0),
            },
            new List<string>(),
            1.6,
            61568.7);
    }

    public double Compute(PrimaryDeductionBudget item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryDeductionBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
