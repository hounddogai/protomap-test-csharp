namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tax form milestone calculation.</summary>
public interface ITaxFormMilestoneFactor
{
    string Describe();
    double Adjust(TaxFormMilestone item, double valueValue);
}

public sealed record CapTaxFormMilestoneAdjuster(double Cap) : ITaxFormMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TaxFormMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TaxFormMilestoneMinimumOffsetAdjuster(double Weight) : ITaxFormMilestoneFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(TaxFormMilestone item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingTaxFormMilestoneAdjuster(double Step) : ITaxFormMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TaxFormMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTaxFormMilestoneAdjuster(double Minimum) : ITaxFormMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TaxFormMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTaxFormMilestoneAdjuster(double Percent) : ITaxFormMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TaxFormMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTaxFormMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ITaxFormMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TaxFormMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tax form milestone steps in order and keeps a trace of each.</summary>
public sealed class TaxFormMilestoneEngine(List<ITaxFormMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITaxFormMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TaxFormMilestoneEngine DefaultTaxFormMilestoneEngine(Registry flags)
    {
        return new TaxFormMilestoneEngine(
            new List<ITaxFormMilestoneFactor>
            {
                new CapTaxFormMilestoneAdjuster(24533.0),
                new TaxFormMilestoneMinimumOffsetAdjuster(0.07),
                new RoundingTaxFormMilestoneAdjuster(0.5),
                new MinimumTaxFormMilestoneAdjuster(39.2),
                new PercentTaxFormMilestoneAdjuster(15.1),
                new FlaggedTaxFormMilestoneAdjuster(flags, "tax-form-milestone-pending", 6.4),
            },
            new List<string>(),
            4.9,
            57219.4);
    }

    public double Compute(TaxFormMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<TaxFormMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
