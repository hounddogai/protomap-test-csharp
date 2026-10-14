namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual retro pay budget calculation.</summary>
public interface IManualRetroPayBudgetStep
{
    string Describe();
    double Adjust(ManualRetroPayBudget item, double valueValue);
}

public sealed record LocaleManualRetroPayBudgetAdjuster(Dictionary<string, double> Factors) : IManualRetroPayBudgetStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ManualRetroPayBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualRetroPayBudgetAdjuster(List<double> Bounds, List<double> Rates) : IManualRetroPayBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualRetroPayBudget item, double valueValue)
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

public sealed record CapManualRetroPayBudgetAdjuster(double Cap) : IManualRetroPayBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualRetroPayBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumManualRetroPayBudgetAdjuster(double Minimum) : IManualRetroPayBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualRetroPayBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual retro pay budget steps in order and keeps a trace of each.</summary>
public sealed class ManualRetroPayBudgetEngine(List<IManualRetroPayBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualRetroPayBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualRetroPayBudgetEngine DefaultManualRetroPayBudgetEngine()
    {
        return new ManualRetroPayBudgetEngine(
            new List<IManualRetroPayBudgetStep>
            {
                new LocaleManualRetroPayBudgetAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.82, ["central"] = 1.32, ["retail"] = 1.4 }),
                new TieredManualRetroPayBudgetAdjuster(
                    new List<double> { 1072.0, 5853.0, 9025.0, 9809.0 },
                    new List<double> { 0.96, 0.91, 1.0, 0.93 }),
                new CapManualRetroPayBudgetAdjuster(10836.0),
                new MinimumManualRetroPayBudgetAdjuster(9.9),
            },
            new List<string>(),
            0.2,
            44509.3);
    }

    public double Evaluate(ManualRetroPayBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualRetroPayBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
