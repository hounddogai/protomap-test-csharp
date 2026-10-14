namespace Acme.Workforce.Payrollops.Payslip.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default payslip budget calculation.</summary>
public interface IDefaultPayslipBudgetStep
{
    string Describe();
    double Adjust(DefaultPayslipBudget item, double valueValue);
}

public sealed record DefaultPayslipBudgetStartsAtOffsetAdjuster(double Weight) : IDefaultPayslipBudgetStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(DefaultPayslipBudget item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record MinimumDefaultPayslipBudgetAdjuster(double Minimum) : IDefaultPayslipBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayslipBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingDefaultPayslipBudgetAdjuster(double Step) : IDefaultPayslipBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultPayslipBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultPayslipBudgetAdjuster(double Percent) : IDefaultPayslipBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayslipBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedDefaultPayslipBudgetAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPayslipBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPayslipBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleDefaultPayslipBudgetAdjuster(Dictionary<string, double> Factors) : IDefaultPayslipBudgetStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultPayslipBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default payslip budget steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayslipBudgetEngine(List<IDefaultPayslipBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayslipBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayslipBudgetEngine DefaultDefaultPayslipBudgetEngine(Registry flags)
    {
        return new DefaultPayslipBudgetEngine(
            new List<IDefaultPayslipBudgetStep>
            {
                new DefaultPayslipBudgetStartsAtOffsetAdjuster(1.15),
                new MinimumDefaultPayslipBudgetAdjuster(28.6),
                new RoundingDefaultPayslipBudgetAdjuster(0.25),
                new PercentDefaultPayslipBudgetAdjuster(-12.9),
                new FlaggedDefaultPayslipBudgetAdjuster(flags, "default-payslip-budget-primary", -0.5),
                new TitleDefaultPayslipBudgetAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.74, ["central"] = 1.27 }),
            },
            new List<string>(),
            4.9,
            40209.7);
    }

    public double Price(DefaultPayslipBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayslipBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
