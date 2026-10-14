namespace Acme.Workforce.Payrollops.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the regional deduction template calculation.</summary>
public interface IRegionalDeductionTemplateModifier
{
    string Describe();
    double Adjust(RegionalDeductionTemplate item, double valueValue);
}

public sealed record CapRegionalDeductionTemplateAdjuster(double Cap) : IRegionalDeductionTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalDeductionTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RegionalDeductionTemplateRateOffsetAdjuster(double Weight) : IRegionalDeductionTemplateModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RegionalDeductionTemplate item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedRegionalDeductionTemplateAdjuster(Registry Flags, string Flag, double Percent) : IRegionalDeductionTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalDeductionTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRegionalDeductionTemplateAdjuster(double Percent) : IRegionalDeductionTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalDeductionTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalDeductionTemplateAdjuster(double Minimum) : IRegionalDeductionTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalDeductionTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the regional deduction template steps in order and keeps a trace of each.</summary>
public sealed class RegionalDeductionTemplateEngine(List<IRegionalDeductionTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalDeductionTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalDeductionTemplateEngine DefaultRegionalDeductionTemplateEngine(Registry flags)
    {
        return new RegionalDeductionTemplateEngine(
            new List<IRegionalDeductionTemplateModifier>
            {
                new CapRegionalDeductionTemplateAdjuster(46072.0),
                new RegionalDeductionTemplateRateOffsetAdjuster(1.82),
                new FlaggedRegionalDeductionTemplateAdjuster(flags, "regional-deduction-template-default", -8.9),
                new PercentRegionalDeductionTemplateAdjuster(12.2),
                new MinimumRegionalDeductionTemplateAdjuster(24.1),
            },
            new List<string>(),
            0.2,
            30992.5);
    }

    public double Evaluate(RegionalDeductionTemplate item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalDeductionTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
