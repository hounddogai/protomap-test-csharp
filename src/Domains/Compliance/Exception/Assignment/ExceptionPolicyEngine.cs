namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception policy calculation.</summary>
public interface IExceptionPolicyFactor
{
    string Describe();
    double Adjust(ExceptionPolicy item, double valueValue);
}

public sealed record TieredExceptionPolicyAdjuster(List<double> Bounds, List<double> Rates) : IExceptionPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ExceptionPolicy item, double valueValue)
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

public sealed record NameExceptionPolicyAdjuster(Dictionary<string, double> Factors) : IExceptionPolicyFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ExceptionPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingExceptionPolicyAdjuster(double Step) : IExceptionPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedExceptionPolicyAdjuster(Registry Flags, string Flag, double Percent) : IExceptionPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumExceptionPolicyAdjuster(double Minimum) : IExceptionPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the exception policy steps in order and keeps a trace of each.</summary>
public sealed class ExceptionPolicyEngine(List<IExceptionPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionPolicyEngine DefaultExceptionPolicyEngine(Registry flags)
    {
        return new ExceptionPolicyEngine(
            new List<IExceptionPolicyFactor>
            {
                new TieredExceptionPolicyAdjuster(
                    new List<double> { 7480.0, 9438.0 },
                    new List<double> { 1.06, 1.06 }),
                new NameExceptionPolicyAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.88, ["west"] = 0.82, ["online"] = 1.39 }),
                new RoundingExceptionPolicyAdjuster(0.1),
                new FlaggedExceptionPolicyAdjuster(flags, "exception-policy-pending", 0.2),
                new MinimumExceptionPolicyAdjuster(22.6),
            },
            new List<string>(),
            1.0,
            81050.0);
    }

    public double Price(ExceptionPolicy item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
