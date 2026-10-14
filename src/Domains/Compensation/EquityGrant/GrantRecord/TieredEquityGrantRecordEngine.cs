namespace Acme.Workforce.Compensation.EquityGrant.GrantRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered equity grant record calculation.</summary>
public interface ITieredEquityGrantRecordFactor
{
    string Describe();
    double Adjust(TieredEquityGrantRecord item, double valueValue);
}

public sealed record PercentTieredEquityGrantRecordAdjuster(double Percent) : ITieredEquityGrantRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredEquityGrantRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredEquityGrantRecordAdjuster(double Step) : ITieredEquityGrantRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredEquityGrantRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryTieredEquityGrantRecordAdjuster(Dictionary<string, double> Factors) : ITieredEquityGrantRecordFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(TieredEquityGrantRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered equity grant record steps in order and keeps a trace of each.</summary>
public sealed class TieredEquityGrantRecordEngine(List<ITieredEquityGrantRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredEquityGrantRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredEquityGrantRecordEngine DefaultTieredEquityGrantRecordEngine()
    {
        return new TieredEquityGrantRecordEngine(
            new List<ITieredEquityGrantRecordFactor>
            {
                new PercentTieredEquityGrantRecordAdjuster(-4.9),
                new RoundingTieredEquityGrantRecordAdjuster(1.0),
                new CountryTieredEquityGrantRecordAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.8, ["east"] = 1.2, ["online"] = 0.88 }),
            },
            new List<string>(),
            0.9,
            74199.1);
    }

    public double Evaluate(TieredEquityGrantRecord item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredEquityGrantRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
