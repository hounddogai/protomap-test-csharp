namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>A rule a default training due audit must pass. Check returns its problems, or none.</summary>
public interface IDefaultTrainingDueAuditRule
{
    List<string> Check(DefaultTrainingDueAudit item);
}

public sealed record DefaultTrainingDueAuditUpdatedAtRangeRule(int Min, int Max) : IDefaultTrainingDueAuditRule
{
    public List<string> Check(DefaultTrainingDueAudit item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultTrainingDueAuditRequiredCodeRule(string Message) : IDefaultTrainingDueAuditRule
{
    public List<string> Check(DefaultTrainingDueAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultTrainingDueAuditRequiredUnitRule(string Message) : IDefaultTrainingDueAuditRule
{
    public List<string> Check(DefaultTrainingDueAudit item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default training due audit.</summary>
public sealed record DefaultTrainingDueAuditValidator(List<IDefaultTrainingDueAuditRule> Rules)
{
    public List<string> Validate(DefaultTrainingDueAudit item)
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

    public void RequireValid(DefaultTrainingDueAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultTrainingDueAudit> items)
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

/// <summary>Functions of default training due audit rules.</summary>
public static class DefaultTrainingDueAuditRules
{
    public static DefaultTrainingDueAuditValidator DefaultDefaultTrainingDueAuditValidator()
    {
        return new DefaultTrainingDueAuditValidator(
            new List<IDefaultTrainingDueAuditRule>
            {
                new DefaultTrainingDueAuditUpdatedAtRangeRule(64, 255),
                new DefaultTrainingDueAuditRequiredCodeRule("default training due audit needs this field"),
                new DefaultTrainingDueAuditRequiredUnitRule("default training due audit needs an entry"),
            });
    }
}
