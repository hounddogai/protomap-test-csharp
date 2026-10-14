namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive tax form record calculation.</summary>
public interface IAdaptiveTaxFormRecordStep
{
    string Describe();
    double Adjust(AdaptiveTaxFormRecord item, double valueValue);
}

public sealed record RoundingAdaptiveTaxFormRecordAdjuster(double Step) : IAdaptiveTaxFormRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTaxFormRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelAdaptiveTaxFormRecordAdjuster(Dictionary<string, double> Factors) : IAdaptiveTaxFormRecordStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AdaptiveTaxFormRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAdaptiveTaxFormRecordAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveTaxFormRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveTaxFormRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveTaxFormRecordAmountOffsetAdjuster(double Weight) : IAdaptiveTaxFormRecordStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(AdaptiveTaxFormRecord item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the adaptive tax form record steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTaxFormRecordEngine(List<IAdaptiveTaxFormRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTaxFormRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTaxFormRecordEngine DefaultAdaptiveTaxFormRecordEngine(Registry flags)
    {
        return new AdaptiveTaxFormRecordEngine(
            new List<IAdaptiveTaxFormRecordStep>
            {
                new RoundingAdaptiveTaxFormRecordAdjuster(0.5),
                new ChannelAdaptiveTaxFormRecordAdjuster(new Dictionary<string, double> { ["north"] = 0.96 }),
                new FlaggedAdaptiveTaxFormRecordAdjuster(flags, "adaptive-tax-form-record-scheduled", -6.6),
                new AdaptiveTaxFormRecordAmountOffsetAdjuster(0.94),
            },
            new List<string>(),
            4.7,
            50888.2);
    }

    public double Run(AdaptiveTaxFormRecord item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTaxFormRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
