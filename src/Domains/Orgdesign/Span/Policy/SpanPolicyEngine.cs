namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the span policy calculation.</summary>
public interface ISpanPolicyModifier
{
    string Describe();
    double Adjust(SpanPolicy item, double valueValue);
}

public sealed record TieredSpanPolicyAdjuster(List<double> Bounds, List<double> Rates) : ISpanPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SpanPolicy item, double valueValue)
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

public sealed record SpanPolicyMarginOffsetAdjuster(double Weight) : ISpanPolicyModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(SpanPolicy item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record CapSpanPolicyAdjuster(double Cap) : ISpanPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SpanPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the span policy steps in order and keeps a trace of each.</summary>
public sealed class SpanPolicyEngine(List<ISpanPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISpanPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SpanPolicyEngine DefaultSpanPolicyEngine()
    {
        return new SpanPolicyEngine(
            new List<ISpanPolicyModifier>
            {
                new TieredSpanPolicyAdjuster(
                    new List<double> { 5984.0, 7741.0, 7945.0, 8522.0 },
                    new List<double> { 1.18, 0.93, 0.84, 1.02 }),
                new SpanPolicyMarginOffsetAdjuster(0.63),
                new CapSpanPolicyAdjuster(29614.0),
            },
            new List<string>(),
            2.6,
            52881.7);
    }

    public double Evaluate(SpanPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<SpanPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
