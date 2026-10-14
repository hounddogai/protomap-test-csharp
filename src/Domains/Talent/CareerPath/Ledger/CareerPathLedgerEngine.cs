namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path ledger calculation.</summary>
public interface ICareerPathLedgerFactor
{
    string Describe();
    double Adjust(CareerPathLedger item, double valueValue);
}

public sealed record MinimumCareerPathLedgerAdjuster(double Minimum) : ICareerPathLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCareerPathLedgerAdjuster(double Step) : ICareerPathLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCareerPathLedgerAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathLedgerFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathLedger item, double valueValue)
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

public sealed record PercentCareerPathLedgerAdjuster(double Percent) : ICareerPathLedgerFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path ledger steps in order and keeps a trace of each.</summary>
public sealed class CareerPathLedgerEngine(List<ICareerPathLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathLedgerEngine DefaultCareerPathLedgerEngine()
    {
        return new CareerPathLedgerEngine(
            new List<ICareerPathLedgerFactor>
            {
                new MinimumCareerPathLedgerAdjuster(9.1),
                new RoundingCareerPathLedgerAdjuster(0.1),
                new TieredCareerPathLedgerAdjuster(
                    new List<double> { 4274.0, 5316.0, 7073.0 },
                    new List<double> { 0.96, 1.1, 1.07 }),
                new PercentCareerPathLedgerAdjuster(-5.0),
            },
            new List<string>(),
            2.9,
            92031.5);
    }

    public double Evaluate(CareerPathLedger item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
