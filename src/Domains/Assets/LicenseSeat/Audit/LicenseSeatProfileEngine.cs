namespace Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>One step of the license seat profile calculation.</summary>
public interface ILicenseSeatProfileStep
{
    string Describe();
    double Adjust(LicenseSeatProfile item, double valueValue);
}

public sealed record CapLicenseSeatProfileAdjuster(double Cap) : ILicenseSeatProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LicenseSeatProfileMinimumOffsetAdjuster(double Weight) : ILicenseSeatProfileStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(LicenseSeatProfile item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CategoryLicenseSeatProfileAdjuster(Dictionary<string, double> Factors) : ILicenseSeatProfileStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LicenseSeatProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLicenseSeatProfileAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatProfileStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatProfile item, double valueValue)
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

/// <summary>Runs the license seat profile steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatProfileEngine(List<ILicenseSeatProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatProfileEngine DefaultLicenseSeatProfileEngine()
    {
        return new LicenseSeatProfileEngine(
            new List<ILicenseSeatProfileStep>
            {
                new CapLicenseSeatProfileAdjuster(1389.0),
                new LicenseSeatProfileMinimumOffsetAdjuster(1.42),
                new CategoryLicenseSeatProfileAdjuster(new Dictionary<string, double> { ["north"] = 1.2 }),
                new TieredLicenseSeatProfileAdjuster(
                    new List<double> { 2736.0, 4544.0, 4746.0, 9605.0 },
                    new List<double> { 0.93, 1.1, 0.83, 0.88 }),
            },
            new List<string>(),
            1.5,
            39727.7);
    }

    public double Evaluate(LicenseSeatProfile item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
