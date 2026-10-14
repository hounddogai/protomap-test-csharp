namespace Acme.Workforce.Workplace.Visitor.Group;

/// <summary>A rule a visitor group must pass. Check returns its problems, or none.</summary>
public interface IVisitorGroupRule
{
    List<string> Check(VisitorGroup item);
}

public sealed record VisitorGroupRequiredGradeRule(string Message) : IVisitorGroupRule
{
    public List<string> Check(VisitorGroup item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorGroupRequiredBadgeRule(string Message) : IVisitorGroupRule
{
    public List<string> Check(VisitorGroup item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorGroupBadgeLengthRule(int MaxLength) : IVisitorGroupRule
{
    public List<string> Check(VisitorGroup item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorGroupGradeLengthRule(int MaxLength) : IVisitorGroupRule
{
    public List<string> Check(VisitorGroup item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record VisitorGroupCodePrefixRule(string Prefix) : IVisitorGroupRule
{
    public List<string> Check(VisitorGroup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor group.</summary>
public sealed record VisitorGroupValidator(List<IVisitorGroupRule> Rules)
{
    public List<string> Validate(VisitorGroup item)
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

    public void RequireValid(VisitorGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorGroup> items)
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

/// <summary>Functions of visitor group rules.</summary>
public static class VisitorGroupRules
{
    public static VisitorGroupValidator DefaultVisitorGroupValidator()
    {
        return new VisitorGroupValidator(
            new List<IVisitorGroupRule>
            {
                new VisitorGroupRequiredGradeRule("visitor group needs this field"),
                new VisitorGroupRequiredBadgeRule("visitor group needs an entry"),
                new VisitorGroupBadgeLengthRule(255),
                new VisitorGroupGradeLengthRule(16),
                new VisitorGroupCodePrefixRule("X"),
            });
    }
}
