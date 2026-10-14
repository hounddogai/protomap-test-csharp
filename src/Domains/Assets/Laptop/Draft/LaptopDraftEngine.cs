namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop draft calculation.</summary>
public interface ILaptopDraftStep
{
    string Describe();
    double Adjust(LaptopDraft item, double valueValue);
}

public sealed record CapLaptopDraftAdjuster(double Cap) : ILaptopDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LaptopDraftTenureMonthsOffsetAdjuster(double Weight) : ILaptopDraftStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(LaptopDraft item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record RoundingLaptopDraftAdjuster(double Step) : ILaptopDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLaptopDraftAdjuster(double Percent) : ILaptopDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the laptop draft steps in order and keeps a trace of each.</summary>
public sealed class LaptopDraftEngine(List<ILaptopDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopDraftEngine DefaultLaptopDraftEngine()
    {
        return new LaptopDraftEngine(
            new List<ILaptopDraftStep>
            {
                new CapLaptopDraftAdjuster(14234.0),
                new LaptopDraftTenureMonthsOffsetAdjuster(1.94),
                new RoundingLaptopDraftAdjuster(5.0),
                new PercentLaptopDraftAdjuster(2.9),
            },
            new List<string>(),
            1.8,
            60472.4);
    }

    public double Compute(LaptopDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
