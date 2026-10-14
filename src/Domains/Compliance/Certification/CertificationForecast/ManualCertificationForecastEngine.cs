namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual certification forecast calculation.</summary>
public interface IManualCertificationForecastFactor
{
    string Describe();
    double Adjust(ManualCertificationForecast item, double valueValue);
}

public sealed record PercentManualCertificationForecastAdjuster(double Percent) : IManualCertificationForecastFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualCertificationForecast item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedManualCertificationForecastAdjuster(Registry Flags, string Flag, double Percent) : IManualCertificationForecastFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualCertificationForecast item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ManualCertificationForecastMarginOffsetAdjuster(double Weight) : IManualCertificationForecastFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ManualCertificationForecast item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record TieredManualCertificationForecastAdjuster(List<double> Bounds, List<double> Rates) : IManualCertificationForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualCertificationForecast item, double valueValue)
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

/// <summary>Runs the manual certification forecast steps in order and keeps a trace of each.</summary>
public sealed class ManualCertificationForecastEngine(List<IManualCertificationForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualCertificationForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualCertificationForecastEngine DefaultManualCertificationForecastEngine(Registry flags)
    {
        return new ManualCertificationForecastEngine(
            new List<IManualCertificationForecastFactor>
            {
                new PercentManualCertificationForecastAdjuster(15.2),
                new FlaggedManualCertificationForecastAdjuster(flags, "manual-certification-forecast-primary", 6.7),
                new ManualCertificationForecastMarginOffsetAdjuster(1.04),
                new TieredManualCertificationForecastAdjuster(
                    new List<double> { 2092.0, 2931.0 },
                    new List<double> { 1.12, 0.92 }),
            },
            new List<string>(),
            0.7,
            44378.1);
    }

    public double Compute(ManualCertificationForecast item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualCertificationForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
