namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling pay run exception calculation.</summary>
public interface IRollingPayRunExceptionFactor
{
    string Describe();
    double Adjust(RollingPayRunException item, double valueValue);
}

public sealed record RoundingRollingPayRunExceptionAdjuster(double Step) : IRollingPayRunExceptionFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPayRunException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingPayRunExceptionAdjuster(double Minimum) : IRollingPayRunExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingPayRunException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRollingPayRunExceptionAdjuster(Registry Flags, string Flag, double Percent) : IRollingPayRunExceptionFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPayRunException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling pay run exception steps in order and keeps a trace of each.</summary>
public sealed class RollingPayRunExceptionEngine(List<IRollingPayRunExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPayRunExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPayRunExceptionEngine DefaultRollingPayRunExceptionEngine(Registry flags)
    {
        return new RollingPayRunExceptionEngine(
            new List<IRollingPayRunExceptionFactor>
            {
                new RoundingRollingPayRunExceptionAdjuster(0.25),
                new MinimumRollingPayRunExceptionAdjuster(35.8),
                new FlaggedRollingPayRunExceptionAdjuster(flags, "rolling-pay-run-exception-lenient", 3.6),
            },
            new List<string>(),
            1.7,
            8673.6);
    }

    public double Run(RollingPayRunException item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingPayRunException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
