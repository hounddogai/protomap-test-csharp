namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a access badge digest must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeDigestRule
{
    List<string> Check(AccessBadgeDigest item);
}

public sealed record AccessBadgeDigestAllowedNameRule(List<string> Allowed) : IAccessBadgeDigestRule
{
    public List<string> Check(AccessBadgeDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is expired" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeDigestRequiredNameRule(string Message) : IAccessBadgeDigestRule
{
    public List<string> Check(AccessBadgeDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeDigestHoursRangeRule(double Min, double Max) : IAccessBadgeDigestRule
{
    public List<string> Check(AccessBadgeDigest item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeDigestSummaryLengthRule(int MaxLength) : IAccessBadgeDigestRule
{
    public List<string> Check(AccessBadgeDigest item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeDigestCapacityRangeRule(int Min, int Max) : IAccessBadgeDigestRule
{
    public List<string> Check(AccessBadgeDigest item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a access badge digest.</summary>
public sealed record AccessBadgeDigestValidator(List<IAccessBadgeDigestRule> Rules)
{
    public List<string> Validate(AccessBadgeDigest item)
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

    public void RequireValid(AccessBadgeDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeDigest> items)
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

/// <summary>Functions of access badge digest rules.</summary>
public static class AccessBadgeDigestRules
{
    public static AccessBadgeDigestValidator DefaultAccessBadgeDigestValidator()
    {
        return new AccessBadgeDigestValidator(
            new List<IAccessBadgeDigestRule>
            {
                new AccessBadgeDigestAllowedNameRule(new List<string> { "legacy", "strict" }),
                new AccessBadgeDigestRequiredNameRule("access badge digest needs a value"),
                new AccessBadgeDigestHoursRangeRule(2.8, 185.9),
                new AccessBadgeDigestSummaryLengthRule(1000),
                new AccessBadgeDigestCapacityRangeRule(120, 3600),
            });
    }
}
