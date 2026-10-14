namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan entry must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanEntryRule
{
    List<string> Check(ReturnPlanEntry item);
}

public sealed record ReturnPlanEntryAllowedSkillRule(List<string> Allowed) : IReturnPlanEntryRule
{
    public List<string> Check(ReturnPlanEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanEntryLeaveDaysRangeRule(int Min, int Max) : IReturnPlanEntryRule
{
    public List<string> Check(ReturnPlanEntry item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanEntryRequiredCodeRule(string Message) : IReturnPlanEntryRule
{
    public List<string> Check(ReturnPlanEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan entry.</summary>
public sealed record ReturnPlanEntryValidator(List<IReturnPlanEntryRule> Rules)
{
    public List<string> Validate(ReturnPlanEntry item)
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

    public void RequireValid(ReturnPlanEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanEntry> items)
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

/// <summary>Functions of return plan entry rules.</summary>
public static class ReturnPlanEntryRules
{
    public static ReturnPlanEntryValidator DefaultReturnPlanEntryValidator()
    {
        return new ReturnPlanEntryValidator(
            new List<IReturnPlanEntryRule>
            {
                new ReturnPlanEntryAllowedSkillRule(new List<string>()),
                new ReturnPlanEntryLeaveDaysRangeRule(255, 86400),
                new ReturnPlanEntryRequiredCodeRule("return plan entry needs a value"),
            });
    }
}
