namespace Acme.Workforce.Compliance.Certification.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification checkpoint calculation.</summary>
public interface ICertificationCheckpointModifier
{
    string Describe();
    double Adjust(CertificationCheckpoint item, double valueValue);
}

public sealed record RoundingCertificationCheckpointAdjuster(double Step) : ICertificationCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CertificationCheckpointRevisionOffsetAdjuster(double Weight) : ICertificationCheckpointModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(CertificationCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record TieredCertificationCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ICertificationCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CertificationCheckpoint item, double valueValue)
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

public sealed record LabelCertificationCheckpointAdjuster(Dictionary<string, double> Factors) : ICertificationCheckpointModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CertificationCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the certification checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CertificationCheckpointEngine(List<ICertificationCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationCheckpointEngine DefaultCertificationCheckpointEngine()
    {
        return new CertificationCheckpointEngine(
            new List<ICertificationCheckpointModifier>
            {
                new RoundingCertificationCheckpointAdjuster(0.05),
                new CertificationCheckpointRevisionOffsetAdjuster(1.05),
                new TieredCertificationCheckpointAdjuster(
                    new List<double> { 325.0, 9547.0 },
                    new List<double> { 1.13, 0.97 }),
                new LabelCertificationCheckpointAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.87, ["retail"] = 1.27 }),
            },
            new List<string>(),
            1.4,
            89325.4);
    }

    public double Evaluate(CertificationCheckpoint item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
