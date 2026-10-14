namespace Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>A rule a access badge summary must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeSummaryRule
{
    List<string> Check(AccessBadgeSummary item);
}

public sealed record AccessBadgeSummaryNameLengthRule(int MaxLength) : IAccessBadgeSummaryRule
{
    public List<string> Check(AccessBadgeSummary item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeSummaryCodePrefixRule(string Prefix) : IAccessBadgeSummaryRule
{
    public List<string> Check(AccessBadgeSummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeSummaryRequiredCodeRule(string Message) : IAccessBadgeSummaryRule
{
    public List<string> Check(AccessBadgeSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeSummaryRequiredNameRule(string Message) : IAccessBadgeSummaryRule
{
    public List<string> Check(AccessBadgeSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge summary.</summary>
public sealed record AccessBadgeSummaryValidator(List<IAccessBadgeSummaryRule> Rules)
{
    public List<string> Validate(AccessBadgeSummary item)
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

    public void RequireValid(AccessBadgeSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeSummary> items)
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

/// <summary>Functions of access badge summary rules.</summary>
public static class AccessBadgeSummaryRules
{
    public static AccessBadgeSummaryValidator DefaultAccessBadgeSummaryValidator()
    {
        return new AccessBadgeSummaryValidator(
            new List<IAccessBadgeSummaryRule>
            {
                new AccessBadgeSummaryNameLengthRule(32),
                new AccessBadgeSummaryCodePrefixRule("Q"),
                new AccessBadgeSummaryRequiredCodeRule("access badge summary needs an entry"),
                new AccessBadgeSummaryRequiredNameRule("access badge summary needs this field"),
            });
    }
}
