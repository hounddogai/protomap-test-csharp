namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

/// <summary>A rule a return plan schedule must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanScheduleRule
{
    List<string> Check(ReturnPlanSchedule item);
}

public sealed record ReturnPlanScheduleRequiredNameRule(string Message) : IReturnPlanScheduleRule
{
    public List<string> Check(ReturnPlanSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanScheduleRequiredExternalRefRule(string Message) : IReturnPlanScheduleRule
{
    public List<string> Check(ReturnPlanSchedule item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanScheduleRequiredChannelRule(string Message) : IReturnPlanScheduleRule
{
    public List<string> Check(ReturnPlanSchedule item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanScheduleNameLengthRule(int MaxLength) : IReturnPlanScheduleRule
{
    public List<string> Check(ReturnPlanSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanScheduleExternalRefLengthRule(int MaxLength) : IReturnPlanScheduleRule
{
    public List<string> Check(ReturnPlanSchedule item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanScheduleRevisionRangeRule(int Min, int Max) : IReturnPlanScheduleRule
{
    public List<string> Check(ReturnPlanSchedule item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a return plan schedule.</summary>
public sealed record ReturnPlanScheduleValidator(List<IReturnPlanScheduleRule> Rules)
{
    public List<string> Validate(ReturnPlanSchedule item)
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

    public void RequireValid(ReturnPlanSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanSchedule> items)
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

/// <summary>Functions of return plan schedule rules.</summary>
public static class ReturnPlanScheduleRules
{
    public static ReturnPlanScheduleValidator DefaultReturnPlanScheduleValidator()
    {
        return new ReturnPlanScheduleValidator(
            new List<IReturnPlanScheduleRule>
            {
                new ReturnPlanScheduleRequiredNameRule("return plan schedule needs a value"),
                new ReturnPlanScheduleRequiredExternalRefRule("return plan schedule needs a value"),
                new ReturnPlanScheduleRequiredChannelRule("return plan schedule needs a value"),
                new ReturnPlanScheduleNameLengthRule(64),
                new ReturnPlanScheduleExternalRefLengthRule(255),
                new ReturnPlanScheduleRevisionRangeRule(1000, 1000),
            });
    }
}
