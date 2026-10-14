namespace Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>A rule a access badge score must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeScoreRule
{
    List<string> Check(AccessBadgeScore item);
}

public sealed record AccessBadgeScoreRequiredNameRule(string Message) : IAccessBadgeScoreRule
{
    public List<string> Check(AccessBadgeScore item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeScoreCodeLengthRule(int MaxLength) : IAccessBadgeScoreRule
{
    public List<string> Check(AccessBadgeScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeScorePinnedGuardRule(int Limit) : IAccessBadgeScoreRule
{
    public List<string> Check(AccessBadgeScore item)
    {
        if (item.Pinned && item.GraceDays > Limit)
        {
            return new List<string> { $"grace days {item.GraceDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge score.</summary>
public sealed record AccessBadgeScoreValidator(List<IAccessBadgeScoreRule> Rules)
{
    public List<string> Validate(AccessBadgeScore item)
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

    public void RequireValid(AccessBadgeScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeScore> items)
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

/// <summary>Functions of access badge score rules.</summary>
public static class AccessBadgeScoreRules
{
    public static AccessBadgeScoreValidator DefaultAccessBadgeScoreValidator()
    {
        return new AccessBadgeScoreValidator(
            new List<IAccessBadgeScoreRule>
            {
                new AccessBadgeScoreRequiredNameRule("access badge score needs a value"),
                new AccessBadgeScoreCodeLengthRule(255),
                new AccessBadgeScorePinnedGuardRule(64),
            });
    }
}
