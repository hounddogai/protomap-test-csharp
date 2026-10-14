namespace Acme.Workforce.Absence.Holiday.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the holiday policy calculation.</summary>
public interface IHolidayPolicyModifier
{
    string Describe();
    double Adjust(HolidayPolicy item, double valueValue);
}

public sealed record TieredHolidayPolicyAdjuster(List<double> Bounds, List<double> Rates) : IHolidayPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HolidayPolicy item, double valueValue)
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

public sealed record MinimumHolidayPolicyAdjuster(double Minimum) : IHolidayPolicyModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HolidayPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentHolidayPolicyAdjuster(double Percent) : IHolidayPolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HolidayPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the holiday policy steps in order and keeps a trace of each.</summary>
public sealed class HolidayPolicyEngine(List<IHolidayPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHolidayPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HolidayPolicyEngine DefaultHolidayPolicyEngine()
    {
        return new HolidayPolicyEngine(
            new List<IHolidayPolicyModifier>
            {
                new TieredHolidayPolicyAdjuster(
                    new List<double> { 1812.0, 6323.0, 8739.0, 9529.0 },
                    new List<double> { 0.92, 0.86, 0.97, 1.08 }),
                new MinimumHolidayPolicyAdjuster(36.1),
                new PercentHolidayPolicyAdjuster(10.4),
            },
            new List<string>(),
            3.6,
            6143.2);
    }

    public double Evaluate(HolidayPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<HolidayPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
