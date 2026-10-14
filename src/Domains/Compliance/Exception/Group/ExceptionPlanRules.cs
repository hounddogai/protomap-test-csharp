namespace Acme.Workforce.Compliance.Exception.Group;

/// <summary>A rule a exception plan must pass. Check returns its problems, or none.</summary>
public interface IExceptionPlanRule
{
    List<string> Check(ExceptionPlan item);
}

public sealed record ExceptionPlanRequiredBadgeRule(string Message) : IExceptionPlanRule
{
    public List<string> Check(ExceptionPlan item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionPlanSkillLengthRule(int MaxLength) : IExceptionPlanRule
{
    public List<string> Check(ExceptionPlan item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionPlanLabelLengthRule(int MaxLength) : IExceptionPlanRule
{
    public List<string> Check(ExceptionPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception plan.</summary>
public sealed record ExceptionPlanValidator(List<IExceptionPlanRule> Rules)
{
    public List<string> Validate(ExceptionPlan item)
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

    public void RequireValid(ExceptionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionPlan> items)
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

/// <summary>Functions of exception plan rules.</summary>
public static class ExceptionPlanRules
{
    public static ExceptionPlanValidator DefaultExceptionPlanValidator()
    {
        return new ExceptionPlanValidator(
            new List<IExceptionPlanRule>
            {
                new ExceptionPlanRequiredBadgeRule("exception plan needs a value"),
                new ExceptionPlanSkillLengthRule(1000),
                new ExceptionPlanLabelLengthRule(80),
            });
    }
}
