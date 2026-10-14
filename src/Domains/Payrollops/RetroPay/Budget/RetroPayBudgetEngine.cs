namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the retro pay budget calculation.</summary>
public interface IRetroPayBudgetStep
{
    string Describe();
    double Adjust(RetroPayBudget item, double valueValue);
}

public sealed record RetroPayBudgetCeilingOffsetAdjuster(double Weight) : IRetroPayBudgetStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RetroPayBudget item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record MinimumRetroPayBudgetAdjuster(double Minimum) : IRetroPayBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamRetroPayBudgetAdjuster(Dictionary<string, double> Factors) : IRetroPayBudgetStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RetroPayBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRetroPayBudgetAdjuster(Registry Flags, string Flag, double Percent) : IRetroPayBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RetroPayBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the retro pay budget steps in order and keeps a trace of each.</summary>
public sealed class RetroPayBudgetEngine(List<IRetroPayBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayBudgetEngine DefaultRetroPayBudgetEngine(Registry flags)
    {
        return new RetroPayBudgetEngine(
            new List<IRetroPayBudgetStep>
            {
                new RetroPayBudgetCeilingOffsetAdjuster(1.47),
                new MinimumRetroPayBudgetAdjuster(15.4),
                new TeamRetroPayBudgetAdjuster(new Dictionary<string, double> { ["online"] = 1.22 }),
                new FlaggedRetroPayBudgetAdjuster(flags, "retro-pay-budget-weighted", 8.1),
            },
            new List<string>(),
            2.9,
            48754.6);
    }

    public double Evaluate(RetroPayBudget item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
