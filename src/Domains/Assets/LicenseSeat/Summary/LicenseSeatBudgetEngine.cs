namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat budget calculation.</summary>
public interface ILicenseSeatBudgetStep
{
    string Describe();
    double Adjust(LicenseSeatBudget item, double valueValue);
}

public sealed record RoundingLicenseSeatBudgetAdjuster(double Step) : ILicenseSeatBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLicenseSeatBudgetAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatBudget item, double valueValue)
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

public sealed record LicenseSeatBudgetCostOffsetAdjuster(double Weight) : ILicenseSeatBudgetStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LicenseSeatBudget item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

/// <summary>Runs the license seat budget steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatBudgetEngine(List<ILicenseSeatBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatBudgetEngine DefaultLicenseSeatBudgetEngine()
    {
        return new LicenseSeatBudgetEngine(
            new List<ILicenseSeatBudgetStep>
            {
                new RoundingLicenseSeatBudgetAdjuster(1.0),
                new TieredLicenseSeatBudgetAdjuster(
                    new List<double> { 81.0, 917.0, 4980.0 },
                    new List<double> { 1.06, 1.0, 0.92 }),
                new LicenseSeatBudgetCostOffsetAdjuster(0.79),
            },
            new List<string>(),
            4.4,
            50476.3);
    }

    public double Price(LicenseSeatBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
