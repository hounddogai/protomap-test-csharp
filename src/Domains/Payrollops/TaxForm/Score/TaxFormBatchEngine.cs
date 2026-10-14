namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form batch calculation.</summary>
public interface ITaxFormBatchAdjuster
{
    string Describe();
    double Adjust(TaxFormBatch item, double valueValue);
}

public sealed record LabelTaxFormBatchAdjuster(Dictionary<string, double> Factors) : ITaxFormBatchAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TaxFormBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTaxFormBatchAdjuster(double Percent) : ITaxFormBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTaxFormBatchAdjuster(double Cap) : ITaxFormBatchAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumTaxFormBatchAdjuster(double Minimum) : ITaxFormBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TaxFormBatchTenureMonthsOffsetAdjuster(double Weight) : ITaxFormBatchAdjuster
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(TaxFormBatch item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record RoundingTaxFormBatchAdjuster(double Step) : ITaxFormBatchAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tax form batch steps in order and keeps a trace of each.</summary>
public sealed class TaxFormBatchEngine(List<ITaxFormBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormBatchEngine DefaultTaxFormBatchEngine()
    {
        return new TaxFormBatchEngine(
            new List<ITaxFormBatchAdjuster>
            {
                new LabelTaxFormBatchAdjuster(new Dictionary<string, double> { ["online"] = 0.97 }),
                new PercentTaxFormBatchAdjuster(-21.0),
                new CapTaxFormBatchAdjuster(9304.0),
                new MinimumTaxFormBatchAdjuster(12.8),
                new TaxFormBatchTenureMonthsOffsetAdjuster(0.05),
                new RoundingTaxFormBatchAdjuster(0.1),
            },
            new List<string>(),
            1.4,
            57220.5);
    }

    public double Evaluate(TaxFormBatch item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
