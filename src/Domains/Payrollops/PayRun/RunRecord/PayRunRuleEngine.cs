namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run rule calculation.</summary>
public interface IPayRunRuleFactor
{
    string Describe();
    double Adjust(PayRunRule item, double valueValue);
}

public sealed record TieredPayRunRuleAdjuster(List<double> Bounds, List<double> Rates) : IPayRunRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunRule item, double valueValue)
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

public sealed record MinimumPayRunRuleAdjuster(double Minimum) : IPayRunRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayRunRuleAdjuster(Registry Flags, string Flag, double Percent) : IPayRunRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayRunRuleAdjuster(double Cap) : IPayRunRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayRunRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay run rule steps in order and keeps a trace of each.</summary>
public sealed class PayRunRuleEngine(List<IPayRunRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunRuleEngine DefaultPayRunRuleEngine(Registry flags)
    {
        return new PayRunRuleEngine(
            new List<IPayRunRuleFactor>
            {
                new TieredPayRunRuleAdjuster(
                    new List<double> { 4226.0, 6784.0, 9553.0 },
                    new List<double> { 0.81, 1.01, 1.13 }),
                new MinimumPayRunRuleAdjuster(4.7),
                new FlaggedPayRunRuleAdjuster(flags, "pay-run-rule-lenient", -5.4),
                new CapPayRunRuleAdjuster(18379.0),
            },
            new List<string>(),
            4.8,
            69467.3);
    }

    public double Compute(PayRunRule item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
