namespace Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>A rule a deduction group must pass. Check returns its problems, or none.</summary>
public interface IDeductionGroupRule
{
    List<string> Check(DeductionGroup item);
}

public sealed record DeductionGroupCodeLengthRule(int MaxLength) : IDeductionGroupRule
{
    public List<string> Check(DeductionGroup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionGroupRequiredCodeRule(string Message) : IDeductionGroupRule
{
    public List<string> Check(DeductionGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionGroupCodePrefixRule(string Prefix) : IDeductionGroupRule
{
    public List<string> Check(DeductionGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionGroupBadgeLengthRule(int MaxLength) : IDeductionGroupRule
{
    public List<string> Check(DeductionGroup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionGroupRequiredBadgeRule(string Message) : IDeductionGroupRule
{
    public List<string> Check(DeductionGroup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionGroupRequiredTitleRule(string Message) : IDeductionGroupRule
{
    public List<string> Check(DeductionGroup item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction group.</summary>
public sealed record DeductionGroupValidator(List<IDeductionGroupRule> Rules)
{
    public List<string> Validate(DeductionGroup item)
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

    public void RequireValid(DeductionGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionGroup> items)
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

/// <summary>Functions of deduction group rules.</summary>
public static class DeductionGroupRules
{
    public static DeductionGroupValidator DefaultDeductionGroupValidator()
    {
        return new DeductionGroupValidator(
            new List<IDeductionGroupRule>
            {
                new DeductionGroupCodeLengthRule(64),
                new DeductionGroupRequiredCodeRule("deduction group needs this field"),
                new DeductionGroupCodePrefixRule("ACM"),
                new DeductionGroupBadgeLengthRule(32),
                new DeductionGroupRequiredBadgeRule("deduction group needs an entry"),
                new DeductionGroupRequiredTitleRule("deduction group needs an entry"),
            });
    }
}
