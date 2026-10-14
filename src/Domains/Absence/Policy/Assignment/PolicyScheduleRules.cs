namespace Acme.Workforce.Absence.Policy.Assignment;

/// <summary>A rule a policy schedule must pass. Check returns its problems, or none.</summary>
public interface IPolicyScheduleRule
{
    List<string> Check(PolicySchedule item);
}

public sealed record PolicyScheduleRequiredTitleRule(string Message) : IPolicyScheduleRule
{
    public List<string> Check(PolicySchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyScheduleRequiredCodeRule(string Message) : IPolicyScheduleRule
{
    public List<string> Check(PolicySchedule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyScheduleTitleLengthRule(int MaxLength) : IPolicyScheduleRule
{
    public List<string> Check(PolicySchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy schedule.</summary>
public sealed record PolicyScheduleValidator(List<IPolicyScheduleRule> Rules)
{
    public List<string> Validate(PolicySchedule item)
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

    public void RequireValid(PolicySchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicySchedule> items)
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

/// <summary>Functions of policy schedule rules.</summary>
public static class PolicyScheduleRules
{
    public static PolicyScheduleValidator DefaultPolicyScheduleValidator()
    {
        return new PolicyScheduleValidator(
            new List<IPolicyScheduleRule>
            {
                new PolicyScheduleRequiredTitleRule("policy schedule needs a value"),
                new PolicyScheduleRequiredCodeRule("policy schedule needs a value"),
                new PolicyScheduleTitleLengthRule(80),
            });
    }
}
