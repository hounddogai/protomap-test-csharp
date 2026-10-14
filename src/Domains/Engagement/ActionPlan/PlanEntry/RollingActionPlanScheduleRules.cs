namespace Acme.Workforce.Engagement.ActionPlan.PlanEntry;

/// <summary>A rule a rolling action plan schedule must pass. Check returns its problems, or none.</summary>
public interface IRollingActionPlanScheduleRule
{
    List<string> Check(RollingActionPlanSchedule item);
}

public sealed record RollingActionPlanScheduleDiscountRangeRule(double Min, double Max) : IRollingActionPlanScheduleRule
{
    public List<string> Check(RollingActionPlanSchedule item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingActionPlanScheduleNameLengthRule(int MaxLength) : IRollingActionPlanScheduleRule
{
    public List<string> Check(RollingActionPlanSchedule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingActionPlanScheduleRequiredNameRule(string Message) : IRollingActionPlanScheduleRule
{
    public List<string> Check(RollingActionPlanSchedule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingActionPlanScheduleCodePrefixRule(string Prefix) : IRollingActionPlanScheduleRule
{
    public List<string> Check(RollingActionPlanSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingActionPlanScheduleRequiredLocationRule(string Message) : IRollingActionPlanScheduleRule
{
    public List<string> Check(RollingActionPlanSchedule item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling action plan schedule.</summary>
public sealed record RollingActionPlanScheduleValidator(List<IRollingActionPlanScheduleRule> Rules)
{
    public List<string> Validate(RollingActionPlanSchedule item)
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

    public void RequireValid(RollingActionPlanSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingActionPlanSchedule> items)
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

/// <summary>Functions of rolling action plan schedule rules.</summary>
public static class RollingActionPlanScheduleRules
{
    public static RollingActionPlanScheduleValidator DefaultRollingActionPlanScheduleValidator()
    {
        return new RollingActionPlanScheduleValidator(
            new List<IRollingActionPlanScheduleRule>
            {
                new RollingActionPlanScheduleDiscountRangeRule(5.8, 904.6),
                new RollingActionPlanScheduleNameLengthRule(1000),
                new RollingActionPlanScheduleRequiredNameRule("rolling action plan schedule needs this field"),
                new RollingActionPlanScheduleCodePrefixRule("Q"),
                new RollingActionPlanScheduleRequiredLocationRule("rolling action plan schedule needs this field"),
            });
    }
}
