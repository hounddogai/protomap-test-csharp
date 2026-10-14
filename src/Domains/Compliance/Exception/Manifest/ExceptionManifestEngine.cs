namespace Acme.Workforce.Compliance.Exception.Manifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the exception manifest calculation.</summary>
public interface IExceptionManifestStep
{
    string Describe();
    double Adjust(ExceptionManifest item, double valueValue);
}

public sealed record PercentExceptionManifestAdjuster(double Percent) : IExceptionManifestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedExceptionManifestAdjuster(Registry Flags, string Flag, double Percent) : IExceptionManifestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapExceptionManifestAdjuster(double Cap) : IExceptionManifestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ExceptionManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the exception manifest steps in order and keeps a trace of each.</summary>
public sealed class ExceptionManifestEngine(List<IExceptionManifestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionManifestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionManifestEngine DefaultExceptionManifestEngine(Registry flags)
    {
        return new ExceptionManifestEngine(
            new List<IExceptionManifestStep>
            {
                new PercentExceptionManifestAdjuster(-14.4),
                new FlaggedExceptionManifestAdjuster(flags, "exception-manifest-legacy", -5.1),
                new CapExceptionManifestAdjuster(13199.0),
            },
            new List<string>(),
            0.5,
            67499.8);
    }

    public double Price(ExceptionManifest item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
