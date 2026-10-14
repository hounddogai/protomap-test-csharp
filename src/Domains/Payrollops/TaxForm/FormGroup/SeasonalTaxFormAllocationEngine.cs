namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal tax form allocation calculation.</summary>
public interface ISeasonalTaxFormAllocationStep
{
    string Describe();
    double Adjust(SeasonalTaxFormAllocation item, double valueValue);
}

public sealed record TieredSeasonalTaxFormAllocationAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalTaxFormAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalTaxFormAllocation item, double valueValue)
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

public sealed record FlaggedSeasonalTaxFormAllocationAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalTaxFormAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalTaxFormAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalTaxFormAllocationAdjuster(double Cap) : ISeasonalTaxFormAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalTaxFormAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalTaxFormAllocationHoursOffsetAdjuster(double Weight) : ISeasonalTaxFormAllocationStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(SeasonalTaxFormAllocation item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record DepartmentSeasonalTaxFormAllocationAdjuster(Dictionary<string, double> Factors) : ISeasonalTaxFormAllocationStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SeasonalTaxFormAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSeasonalTaxFormAllocationAdjuster(double Step) : ISeasonalTaxFormAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalTaxFormAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal tax form allocation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalTaxFormAllocationEngine(List<ISeasonalTaxFormAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalTaxFormAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalTaxFormAllocationEngine DefaultSeasonalTaxFormAllocationEngine(Registry flags)
    {
        return new SeasonalTaxFormAllocationEngine(
            new List<ISeasonalTaxFormAllocationStep>
            {
                new TieredSeasonalTaxFormAllocationAdjuster(
                    new List<double> { 5866.0, 9720.0 },
                    new List<double> { 1.14, 0.97 }),
                new FlaggedSeasonalTaxFormAllocationAdjuster(flags, "seasonal-tax-form-allocation-default", -1.6),
                new CapSeasonalTaxFormAllocationAdjuster(1897.0),
                new SeasonalTaxFormAllocationHoursOffsetAdjuster(1.61),
                new DepartmentSeasonalTaxFormAllocationAdjuster(new Dictionary<string, double> { ["west"] = 1.13 }),
                new RoundingSeasonalTaxFormAllocationAdjuster(0.25),
            },
            new List<string>(),
            3.8,
            55865.0);
    }

    public double Run(SeasonalTaxFormAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalTaxFormAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
