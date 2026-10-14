namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification forecast calculation.</summary>
public interface ICertificationForecastFactor
{
    string Describe();
    double Adjust(CertificationForecast item, double valueValue);
}

public sealed record TieredCertificationForecastAdjuster(List<double> Bounds, List<double> Rates) : ICertificationForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationForecast item, double valueValue)
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

public sealed record CertificationForecastAmountOffsetAdjuster(double Weight) : ICertificationForecastFactor
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(CertificationForecast item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record CapCertificationForecastAdjuster(double Cap) : ICertificationForecastFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CertificationForecast item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCertificationForecastAdjuster(double Step) : ICertificationForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the certification forecast steps in order and keeps a trace of each.</summary>
public sealed class CertificationForecastEngine(List<ICertificationForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationForecastEngine DefaultCertificationForecastEngine()
    {
        return new CertificationForecastEngine(
            new List<ICertificationForecastFactor>
            {
                new TieredCertificationForecastAdjuster(
                    new List<double> { 1422.0, 2734.0 },
                    new List<double> { 0.99, 1.12 }),
                new CertificationForecastAmountOffsetAdjuster(1.38),
                new CapCertificationForecastAdjuster(3045.0),
                new RoundingCertificationForecastAdjuster(0.5),
            },
            new List<string>(),
            0.5,
            95125.2);
    }

    public double Evaluate(CertificationForecast item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
