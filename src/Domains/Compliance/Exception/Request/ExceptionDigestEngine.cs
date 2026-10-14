namespace Acme.Workforce.Compliance.Exception.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception digest calculation.</summary>
public interface IExceptionDigestFactor
{
    string Describe();
    double Adjust(ExceptionDigest item, double valueValue);
}

public sealed record RoundingExceptionDigestAdjuster(double Step) : IExceptionDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ExceptionDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ExceptionDigestWeightOffsetAdjuster(double Weight) : IExceptionDigestFactor
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(ExceptionDigest item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumExceptionDigestAdjuster(double Minimum) : IExceptionDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ExceptionDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedExceptionDigestAdjuster(Registry Flags, string Flag, double Percent) : IExceptionDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception digest steps in order and keeps a trace of each.</summary>
public sealed class ExceptionDigestEngine(List<IExceptionDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionDigestEngine DefaultExceptionDigestEngine(Registry flags)
    {
        return new ExceptionDigestEngine(
            new List<IExceptionDigestFactor>
            {
                new RoundingExceptionDigestAdjuster(0.1),
                new ExceptionDigestWeightOffsetAdjuster(0.26),
                new MinimumExceptionDigestAdjuster(13.1),
                new FlaggedExceptionDigestAdjuster(flags, "exception-digest-capped", 8.8),
            },
            new List<string>(),
            2.6,
            75432.2);
    }

    public double Run(ExceptionDigest item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
