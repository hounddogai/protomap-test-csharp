namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a timesheet policy must pass. Check returns its problems, or none.</summary>
public interface ITimesheetPolicyRule
{
    List<string> Check(TimesheetPolicy item);
}

public sealed record TimesheetPolicyRequiredLabelRule(string Message) : ITimesheetPolicyRule
{
    public List<string> Check(TimesheetPolicy item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TimesheetPolicyAllowedSkillRule(List<string> Allowed) : ITimesheetPolicyRule
{
    public List<string> Check(TimesheetPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record TimesheetPolicyBudgetRangeRule(double Min, double Max) : ITimesheetPolicyRule
{
    public List<string> Check(TimesheetPolicy item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a timesheet policy.</summary>
public sealed record TimesheetPolicyValidator(List<ITimesheetPolicyRule> Rules)
{
    public List<string> Validate(TimesheetPolicy item)
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

    public void RequireValid(TimesheetPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TimesheetPolicy> items)
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

/// <summary>Functions of timesheet policy rules.</summary>
public static class TimesheetPolicyRules
{
    public static TimesheetPolicyValidator DefaultTimesheetPolicyValidator()
    {
        return new TimesheetPolicyValidator(
            new List<ITimesheetPolicyRule>
            {
                new TimesheetPolicyRequiredLabelRule("timesheet policy needs an entry"),
                new TimesheetPolicyAllowedSkillRule(new List<string> { "rolling", "default" }),
                new TimesheetPolicyBudgetRangeRule(6.2, 2997.0),
            });
    }
}
