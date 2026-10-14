namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge allocation must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeAllocationRule
{
    List<string> Check(AccessBadgeAllocation item);
}

public sealed record AccessBadgeAllocationRequiredCodeRule(string Message) : IAccessBadgeAllocationRule
{
    public List<string> Check(AccessBadgeAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAllocationContactEmailLengthRule(int MaxLength) : IAccessBadgeAllocationRule
{
    public List<string> Check(AccessBadgeAllocation item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAllocationCodeLengthRule(int MaxLength) : IAccessBadgeAllocationRule
{
    public List<string> Check(AccessBadgeAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAllocationRequiredContactEmailRule(string Message) : IAccessBadgeAllocationRule
{
    public List<string> Check(AccessBadgeAllocation item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAllocationAllowedCodeRule(List<string> Allowed) : IAccessBadgeAllocationRule
{
    public List<string> Check(AccessBadgeAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeAllocationRetriesRangeRule(int Min, int Max) : IAccessBadgeAllocationRule
{
    public List<string> Check(AccessBadgeAllocation item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a access badge allocation.</summary>
public sealed record AccessBadgeAllocationValidator(List<IAccessBadgeAllocationRule> Rules)
{
    public List<string> Validate(AccessBadgeAllocation item)
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

    public void RequireValid(AccessBadgeAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeAllocation> items)
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

/// <summary>Functions of access badge allocation rules.</summary>
public static class AccessBadgeAllocationRules
{
    public static AccessBadgeAllocationValidator DefaultAccessBadgeAllocationValidator()
    {
        return new AccessBadgeAllocationValidator(
            new List<IAccessBadgeAllocationRule>
            {
                new AccessBadgeAllocationRequiredCodeRule("access badge allocation needs this field"),
                new AccessBadgeAllocationContactEmailLengthRule(1000),
                new AccessBadgeAllocationCodeLengthRule(255),
                new AccessBadgeAllocationRequiredContactEmailRule("access badge allocation needs this field"),
                new AccessBadgeAllocationAllowedCodeRule(new List<string>()),
                new AccessBadgeAllocationRetriesRangeRule(32, 64),
            });
    }
}
