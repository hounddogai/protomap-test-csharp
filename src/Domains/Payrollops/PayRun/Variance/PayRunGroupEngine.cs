namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run group calculation.</summary>
public interface IPayRunGroupFactor
{
    string Describe();
    double Adjust(PayRunGroup item, double valueValue);
}

public sealed record MinimumPayRunGroupAdjuster(double Minimum) : IPayRunGroupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPayRunGroupAdjuster(List<double> Bounds, List<double> Rates) : IPayRunGroupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunGroup item, double valueValue)
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

public sealed record RoundingPayRunGroupAdjuster(double Step) : IPayRunGroupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayRunGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPayRunGroupAdjuster(Registry Flags, string Flag, double Percent) : IPayRunGroupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay run group steps in order and keeps a trace of each.</summary>
public sealed class PayRunGroupEngine(List<IPayRunGroupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunGroupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunGroupEngine DefaultPayRunGroupEngine(Registry flags)
    {
        return new PayRunGroupEngine(
            new List<IPayRunGroupFactor>
            {
                new MinimumPayRunGroupAdjuster(39.0),
                new TieredPayRunGroupAdjuster(new List<double> { 7458.0, 8757.0 }, new List<double> { 0.99, 1.16 }),
                new RoundingPayRunGroupAdjuster(5.0),
                new FlaggedPayRunGroupAdjuster(flags, "pay-run-group-regional", -6.2),
            },
            new List<string>(),
            1.5,
            11168.4);
    }

    public double Evaluate(PayRunGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
