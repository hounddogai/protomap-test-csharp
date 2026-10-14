namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pay run record calculation.</summary>
public interface ICappedPayRunRecordStep
{
    string Describe();
    double Adjust(CappedPayRunRecord item, double valueValue);
}

public sealed record TieredCappedPayRunRecordAdjuster(List<double> Bounds, List<double> Rates) : ICappedPayRunRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPayRunRecord item, double valueValue)
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

public sealed record CappedPayRunRecordScoreOffsetAdjuster(double Weight) : ICappedPayRunRecordStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(CappedPayRunRecord item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record MinimumCappedPayRunRecordAdjuster(double Minimum) : ICappedPayRunRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedPayRunRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentCappedPayRunRecordAdjuster(double Percent) : ICappedPayRunRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPayRunRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped pay run record steps in order and keeps a trace of each.</summary>
public sealed class CappedPayRunRecordEngine(List<ICappedPayRunRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayRunRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayRunRecordEngine DefaultCappedPayRunRecordEngine()
    {
        return new CappedPayRunRecordEngine(
            new List<ICappedPayRunRecordStep>
            {
                new TieredCappedPayRunRecordAdjuster(
                    new List<double> { 113.0, 2370.0, 6108.0 },
                    new List<double> { 0.99, 0.95, 0.96 }),
                new CappedPayRunRecordScoreOffsetAdjuster(0.64),
                new MinimumCappedPayRunRecordAdjuster(3.9),
                new PercentCappedPayRunRecordAdjuster(-8.7),
            },
            new List<string>(),
            4.2,
            60297.2);
    }

    public double Price(CappedPayRunRecord item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayRunRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
