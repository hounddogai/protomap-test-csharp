namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>A rule a lenient merit cycle claim must pass. Check returns its problems, or none.</summary>
public interface ILenientMeritCycleClaimRule
{
    List<string> Check(LenientMeritCycleClaim item);
}

public sealed record LenientMeritCycleClaimRequiredTitleRule(string Message) : ILenientMeritCycleClaimRule
{
    public List<string> Check(LenientMeritCycleClaim item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleClaimPinnedGuardRule(int Limit) : ILenientMeritCycleClaimRule
{
    public List<string> Check(LenientMeritCycleClaim item)
    {
        if (item.Pinned && item.Retries > Limit)
        {
            return new List<string> { $"retries {item.Retries} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleClaimRetriesRangeRule(int Min, int Max) : ILenientMeritCycleClaimRule
{
    public List<string> Check(LenientMeritCycleClaim item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleClaimCodeLengthRule(int MaxLength) : ILenientMeritCycleClaimRule
{
    public List<string> Check(LenientMeritCycleClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleClaimTitleLengthRule(int MaxLength) : ILenientMeritCycleClaimRule
{
    public List<string> Check(LenientMeritCycleClaim item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientMeritCycleClaimCodePrefixRule(string Prefix) : ILenientMeritCycleClaimRule
{
    public List<string> Check(LenientMeritCycleClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient merit cycle claim.</summary>
public sealed record LenientMeritCycleClaimValidator(List<ILenientMeritCycleClaimRule> Rules)
{
    public List<string> Validate(LenientMeritCycleClaim item)
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

    public void RequireValid(LenientMeritCycleClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientMeritCycleClaim> items)
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

/// <summary>Functions of lenient merit cycle claim rules.</summary>
public static class LenientMeritCycleClaimRules
{
    public static LenientMeritCycleClaimValidator DefaultLenientMeritCycleClaimValidator()
    {
        return new LenientMeritCycleClaimValidator(
            new List<ILenientMeritCycleClaimRule>
            {
                new LenientMeritCycleClaimRequiredTitleRule("lenient merit cycle claim needs an entry"),
                new LenientMeritCycleClaimPinnedGuardRule(1000),
                new LenientMeritCycleClaimRetriesRangeRule(16, 80),
                new LenientMeritCycleClaimCodeLengthRule(64),
                new LenientMeritCycleClaimTitleLengthRule(86400),
                new LenientMeritCycleClaimCodePrefixRule("ACM"),
            });
    }
}
