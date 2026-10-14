namespace Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>A rule a fallback pay band snapshot must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayBandSnapshotRule
{
    List<string> Check(FallbackPayBandSnapshot item);
}

public sealed record FallbackPayBandSnapshotVersionRangeRule(int Min, int Max) : IFallbackPayBandSnapshotRule
{
    public List<string> Check(FallbackPayBandSnapshot item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPayBandSnapshotArchivedGuardRule(int Limit) : IFallbackPayBandSnapshotRule
{
    public List<string> Check(FallbackPayBandSnapshot item)
    {
        if (item.Archived && item.Version > Limit)
        {
            return new List<string> { $"version {item.Version} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandSnapshotCurrencyLengthRule(int MaxLength) : IFallbackPayBandSnapshotRule
{
    public List<string> Check(FallbackPayBandSnapshot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandSnapshotRateRangeRule(double Min, double Max) : IFallbackPayBandSnapshotRule
{
    public List<string> Check(FallbackPayBandSnapshot item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayBandSnapshotWeightRangeRule(double Min, double Max) : IFallbackPayBandSnapshotRule
{
    public List<string> Check(FallbackPayBandSnapshot item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback pay band snapshot.</summary>
public sealed record FallbackPayBandSnapshotValidator(List<IFallbackPayBandSnapshotRule> Rules)
{
    public List<string> Validate(FallbackPayBandSnapshot item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(FallbackPayBandSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayBandSnapshot> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of fallback pay band snapshot rules.</summary>
public static class FallbackPayBandSnapshotRules
{
    public static FallbackPayBandSnapshotValidator DefaultFallbackPayBandSnapshotValidator()
    {
        return new FallbackPayBandSnapshotValidator(
            new List<IFallbackPayBandSnapshotRule>
            {
                new FallbackPayBandSnapshotVersionRangeRule(64, 1000),
                new FallbackPayBandSnapshotArchivedGuardRule(80),
                new FallbackPayBandSnapshotCurrencyLengthRule(16),
                new FallbackPayBandSnapshotRateRangeRule(4.3, 2901.8),
                new FallbackPayBandSnapshotWeightRangeRule(2.7, 2131.1),
            });
    }
}
