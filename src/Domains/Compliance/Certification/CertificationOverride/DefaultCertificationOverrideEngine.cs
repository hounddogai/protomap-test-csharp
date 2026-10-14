namespace Acme.Workforce.Compliance.Certification.CertificationOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default certification override calculation.</summary>
public interface IDefaultCertificationOverrideModifier
{
    string Describe();
    double Adjust(DefaultCertificationOverride item, double valueValue);
}

public sealed record TitleDefaultCertificationOverrideAdjuster(Dictionary<string, double> Factors) : IDefaultCertificationOverrideModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(DefaultCertificationOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultCertificationOverrideAdjuster(double Step) : IDefaultCertificationOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultCertificationOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDefaultCertificationOverrideAdjuster(double Percent) : IDefaultCertificationOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultCertificationOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultCertificationOverrideAdjuster(double Minimum) : IDefaultCertificationOverrideModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultCertificationOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default certification override steps in order and keeps a trace of each.</summary>
public sealed class DefaultCertificationOverrideEngine(List<IDefaultCertificationOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultCertificationOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultCertificationOverrideEngine DefaultDefaultCertificationOverrideEngine()
    {
        return new DefaultCertificationOverrideEngine(
            new List<IDefaultCertificationOverrideModifier>
            {
                new TitleDefaultCertificationOverrideAdjuster(new Dictionary<string, double> { ["central"] = 0.89 }),
                new RoundingDefaultCertificationOverrideAdjuster(0.1),
                new PercentDefaultCertificationOverrideAdjuster(17.1),
                new MinimumDefaultCertificationOverrideAdjuster(15.7),
            },
            new List<string>(),
            4.3,
            50106.2);
    }

    public double Price(DefaultCertificationOverride item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultCertificationOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
