namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>A rule a tiered offboarding record must pass. Check returns its problems, or none.</summary>
public interface ITieredOffboardingRecordRule
{
    List<string> Check(TieredOffboardingRecord item);
}

public sealed record TieredOffboardingRecordAmountRangeRule(double Min, double Max) : ITieredOffboardingRecordRule
{
    public List<string> Check(TieredOffboardingRecord item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingRecordRequiredCodeRule(string Message) : ITieredOffboardingRecordRule
{
    public List<string> Check(TieredOffboardingRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingRecordRegionLengthRule(int MaxLength) : ITieredOffboardingRecordRule
{
    public List<string> Check(TieredOffboardingRecord item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingRecordArchivedGuardRule(double Limit) : ITieredOffboardingRecordRule
{
    public List<string> Check(TieredOffboardingRecord item)
    {
        if (item.Archived && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record TieredOffboardingRecordCodePrefixRule(string Prefix) : ITieredOffboardingRecordRule
{
    public List<string> Check(TieredOffboardingRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered offboarding record.</summary>
public sealed record TieredOffboardingRecordValidator(List<ITieredOffboardingRecordRule> Rules)
{
    public List<string> Validate(TieredOffboardingRecord item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(TieredOffboardingRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredOffboardingRecord> items)
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

/// <summary>Functions of tiered offboarding record rules.</summary>
public static class TieredOffboardingRecordRules
{
    public static TieredOffboardingRecordValidator DefaultTieredOffboardingRecordValidator()
    {
        return new TieredOffboardingRecordValidator(
            new List<ITieredOffboardingRecordRule>
            {
                new TieredOffboardingRecordAmountRangeRule(1.7, 2493.7),
                new TieredOffboardingRecordRequiredCodeRule("tiered offboarding record needs this field"),
                new TieredOffboardingRecordRegionLengthRule(80),
                new TieredOffboardingRecordArchivedGuardRule(7.3),
                new TieredOffboardingRecordCodePrefixRule("ACM"),
            });
    }
}
