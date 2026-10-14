namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form manifest calculation.</summary>
public interface ITaxFormManifestAdjuster
{
    string Describe();
    double Adjust(TaxFormManifest item, double valueValue);
}

public sealed record MinimumTaxFormManifestAdjuster(double Minimum) : ITaxFormManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TaxFormManifestScoreOffsetAdjuster(double Weight) : ITaxFormManifestAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(TaxFormManifest item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingTaxFormManifestAdjuster(double Step) : ITaxFormManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTaxFormManifestAdjuster(List<double> Bounds, List<double> Rates) : ITaxFormManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TaxFormManifest item, double valueValue)
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

/// <summary>Runs the tax form manifest steps in order and keeps a trace of each.</summary>
public sealed class TaxFormManifestEngine(List<ITaxFormManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormManifestEngine DefaultTaxFormManifestEngine()
    {
        return new TaxFormManifestEngine(
            new List<ITaxFormManifestAdjuster>
            {
                new MinimumTaxFormManifestAdjuster(38.1),
                new TaxFormManifestScoreOffsetAdjuster(0.04),
                new RoundingTaxFormManifestAdjuster(0.25),
                new TieredTaxFormManifestAdjuster(
                    new List<double> { 4991.0, 7397.0, 9675.0 },
                    new List<double> { 1.07, 1.07, 1.11 }),
            },
            new List<string>(),
            1.9,
            32446.9);
    }

    public double Price(TaxFormManifest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
