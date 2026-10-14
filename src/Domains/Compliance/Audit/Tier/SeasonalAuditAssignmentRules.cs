namespace Acme.Workforce.Compliance.Audit.Tier;

/// <summary>A rule a seasonal audit assignment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAuditAssignmentRule
{
    List<string> Check(SeasonalAuditAssignment item);
}

public sealed record SeasonalAuditAssignmentNameLengthRule(int MaxLength) : ISeasonalAuditAssignmentRule
{
    public List<string> Check(SeasonalAuditAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditAssignmentBadgeLengthRule(int MaxLength) : ISeasonalAuditAssignmentRule
{
    public List<string> Check(SeasonalAuditAssignment item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditAssignmentRequiredUnitRule(string Message) : ISeasonalAuditAssignmentRule
{
    public List<string> Check(SeasonalAuditAssignment item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAuditAssignmentRequiredBadgeRule(string Message) : ISeasonalAuditAssignmentRule
{
    public List<string> Check(SeasonalAuditAssignment item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal audit assignment.</summary>
public sealed record SeasonalAuditAssignmentValidator(List<ISeasonalAuditAssignmentRule> Rules)
{
    public List<string> Validate(SeasonalAuditAssignment item)
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

    public void RequireValid(SeasonalAuditAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAuditAssignment> items)
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

/// <summary>Functions of seasonal audit assignment rules.</summary>
public static class SeasonalAuditAssignmentRules
{
    public static SeasonalAuditAssignmentValidator DefaultSeasonalAuditAssignmentValidator()
    {
        return new SeasonalAuditAssignmentValidator(
            new List<ISeasonalAuditAssignmentRule>
            {
                new SeasonalAuditAssignmentNameLengthRule(16),
                new SeasonalAuditAssignmentBadgeLengthRule(16),
                new SeasonalAuditAssignmentRequiredUnitRule("seasonal audit assignment needs a value"),
                new SeasonalAuditAssignmentRequiredBadgeRule("seasonal audit assignment needs an entry"),
            });
    }
}
