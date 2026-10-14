namespace Acme.Workforce.Absence.Policy.Assignment;

/// <summary>A rule a primary policy item must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPolicyItemRule
{
    List<string> Check(PrimaryPolicyItem item);
}

public sealed record PrimaryPolicyItemRequiredNameRule(string Message) : IPrimaryPolicyItemRule
{
    public List<string> Check(PrimaryPolicyItem item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyItemDepartmentLengthRule(int MaxLength) : IPrimaryPolicyItemRule
{
    public List<string> Check(PrimaryPolicyItem item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyItemCodePrefixRule(string Prefix) : IPrimaryPolicyItemRule
{
    public List<string> Check(PrimaryPolicyItem item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyItemRequiredCodeRule(string Message) : IPrimaryPolicyItemRule
{
    public List<string> Check(PrimaryPolicyItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyItemRequiredDepartmentRule(string Message) : IPrimaryPolicyItemRule
{
    public List<string> Check(PrimaryPolicyItem item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPolicyItemBadgeLengthRule(int MaxLength) : IPrimaryPolicyItemRule
{
    public List<string> Check(PrimaryPolicyItem item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary policy item.</summary>
public sealed record PrimaryPolicyItemValidator(List<IPrimaryPolicyItemRule> Rules)
{
    public List<string> Validate(PrimaryPolicyItem item)
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

    public void RequireValid(PrimaryPolicyItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPolicyItem> items)
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

/// <summary>Functions of primary policy item rules.</summary>
public static class PrimaryPolicyItemRules
{
    public static PrimaryPolicyItemValidator DefaultPrimaryPolicyItemValidator()
    {
        return new PrimaryPolicyItemValidator(
            new List<IPrimaryPolicyItemRule>
            {
                new PrimaryPolicyItemRequiredNameRule("primary policy item needs an entry"),
                new PrimaryPolicyItemDepartmentLengthRule(86400),
                new PrimaryPolicyItemCodePrefixRule("ACM"),
                new PrimaryPolicyItemRequiredCodeRule("primary policy item needs an entry"),
                new PrimaryPolicyItemRequiredDepartmentRule("primary policy item needs this field"),
                new PrimaryPolicyItemBadgeLengthRule(120),
            });
    }
}
