namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run budget calculation.</summary>
public interface IPayRunBudgetAdjuster
{
    string Describe();
    double Adjust(PayRunBudget item, double valueValue);
}

public sealed record TieredPayRunBudgetAdjuster(List<double> Bounds, List<double> Rates) : IPayRunBudgetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunBudget item, double valueValue)
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

public sealed record PayRunBudgetAmountOffsetAdjuster(double Weight) : IPayRunBudgetAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(PayRunBudget item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record FlaggedPayRunBudgetAdjuster(Registry Flags, string Flag, double Percent) : IPayRunBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayRunBudgetAdjuster(double Cap) : IPayRunBudgetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPayRunBudgetAdjuster(double Percent) : IPayRunBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay run budget steps in order and keeps a trace of each.</summary>
public sealed class PayRunBudgetEngine(List<IPayRunBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunBudgetEngine DefaultPayRunBudgetEngine(Registry flags)
    {
        return new PayRunBudgetEngine(
            new List<IPayRunBudgetAdjuster>
            {
                new TieredPayRunBudgetAdjuster(
                    new List<double> { 617.0, 2624.0, 3795.0, 7485.0 },
                    new List<double> { 1.02, 1.06, 1.08, 1.11 }),
                new PayRunBudgetAmountOffsetAdjuster(0.26),
                new FlaggedPayRunBudgetAdjuster(flags, "pay-run-budget-strict", -3.6),
                new CapPayRunBudgetAdjuster(14663.0),
                new PercentPayRunBudgetAdjuster(-21.1),
            },
            new List<string>(),
            5.0,
            88924.9);
    }

    public double Evaluate(PayRunBudget item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
