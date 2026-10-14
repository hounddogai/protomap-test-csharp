namespace Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>A rule a equity grant claim must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantClaimRule
{
    List<string> Check(EquityGrantClaim item);
}

public sealed record EquityGrantClaimCodePrefixRule(string Prefix) : IEquityGrantClaimRule
{
    public List<string> Check(EquityGrantClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantClaimStartsAtRangeRule(int Min, int Max) : IEquityGrantClaimRule
{
    public List<string> Check(EquityGrantClaim item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantClaimRequiredCategoryRule(string Message) : IEquityGrantClaimRule
{
    public List<string> Check(EquityGrantClaim item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantClaimCodeLengthRule(int MaxLength) : IEquityGrantClaimRule
{
    public List<string> Check(EquityGrantClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant claim.</summary>
public sealed record EquityGrantClaimValidator(List<IEquityGrantClaimRule> Rules)
{
    public List<string> Validate(EquityGrantClaim item)
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

    public void RequireValid(EquityGrantClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantClaim> items)
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

/// <summary>Functions of equity grant claim rules.</summary>
public static class EquityGrantClaimRules
{
    public static EquityGrantClaimValidator DefaultEquityGrantClaimValidator()
    {
        return new EquityGrantClaimValidator(
            new List<IEquityGrantClaimRule>
            {
                new EquityGrantClaimCodePrefixRule("ACM"),
                new EquityGrantClaimStartsAtRangeRule(80, 80),
                new EquityGrantClaimRequiredCategoryRule("equity grant claim needs a value"),
                new EquityGrantClaimCodeLengthRule(16),
            });
    }
}
