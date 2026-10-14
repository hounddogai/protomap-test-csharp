namespace Acme.Workforce.Assets.Assignment.Profile;

/// <summary>A rule a global assignment adjustment must pass. Check returns its problems, or none.</summary>
public interface IGlobalAssignmentAdjustmentRule
{
    List<string> Check(GlobalAssignmentAdjustment item);
}

public sealed record GlobalAssignmentAdjustmentActiveGuardRule(int Limit) : IGlobalAssignmentAdjustmentRule
{
    public List<string> Check(GlobalAssignmentAdjustment item)
    {
        if (item.Active && item.TenureMonths > Limit)
        {
            return new List<string> { $"tenure months {item.TenureMonths} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssignmentAdjustmentRequiredCodeRule(string Message) : IGlobalAssignmentAdjustmentRule
{
    public List<string> Check(GlobalAssignmentAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalAssignmentAdjustmentRequiredSkillRule(string Message) : IGlobalAssignmentAdjustmentRule
{
    public List<string> Check(GlobalAssignmentAdjustment item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global assignment adjustment.</summary>
public sealed record GlobalAssignmentAdjustmentValidator(List<IGlobalAssignmentAdjustmentRule> Rules)
{
    public List<string> Validate(GlobalAssignmentAdjustment item)
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

    public void RequireValid(GlobalAssignmentAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalAssignmentAdjustment> items)
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

/// <summary>Functions of global assignment adjustment rules.</summary>
public static class GlobalAssignmentAdjustmentRules
{
    public static GlobalAssignmentAdjustmentValidator DefaultGlobalAssignmentAdjustmentValidator()
    {
        return new GlobalAssignmentAdjustmentValidator(
            new List<IGlobalAssignmentAdjustmentRule>
            {
                new GlobalAssignmentAdjustmentActiveGuardRule(120),
                new GlobalAssignmentAdjustmentRequiredCodeRule("global assignment adjustment needs an entry"),
                new GlobalAssignmentAdjustmentRequiredSkillRule("global assignment adjustment needs a value"),
            });
    }
}
