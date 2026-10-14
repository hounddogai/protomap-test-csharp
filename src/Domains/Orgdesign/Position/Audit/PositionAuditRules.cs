namespace Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>A rule a position audit must pass. Check returns its problems, or none.</summary>
public interface IPositionAuditRule
{
    List<string> Check(PositionAudit item);
}

public sealed record PositionAuditReviewerLengthRule(int MaxLength) : IPositionAuditRule
{
    public List<string> Check(PositionAudit item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionAuditRequiredReviewerRule(string Message) : IPositionAuditRule
{
    public List<string> Check(PositionAudit item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionAuditCodePrefixRule(string Prefix) : IPositionAuditRule
{
    public List<string> Check(PositionAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PositionAuditVisibleGuardRule(int Limit) : IPositionAuditRule
{
    public List<string> Check(PositionAudit item)
    {
        if (item.Visible && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PositionAuditRequiredTitleRule(string Message) : IPositionAuditRule
{
    public List<string> Check(PositionAudit item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position audit.</summary>
public sealed record PositionAuditValidator(List<IPositionAuditRule> Rules)
{
    public List<string> Validate(PositionAudit item)
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

    public void RequireValid(PositionAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionAudit> items)
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

/// <summary>Functions of position audit rules.</summary>
public static class PositionAuditRules
{
    public static PositionAuditValidator DefaultPositionAuditValidator()
    {
        return new PositionAuditValidator(
            new List<IPositionAuditRule>
            {
                new PositionAuditReviewerLengthRule(120),
                new PositionAuditRequiredReviewerRule("position audit needs a value"),
                new PositionAuditCodePrefixRule("Q"),
                new PositionAuditVisibleGuardRule(120),
                new PositionAuditRequiredTitleRule("position audit needs a value"),
            });
    }
}
