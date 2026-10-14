namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan mapping must pass. Check returns its problems, or none.</summary>
public interface IActionPlanMappingRule
{
    List<string> Check(ActionPlanMapping item);
}

public sealed record ActionPlanMappingCodeLengthRule(int MaxLength) : IActionPlanMappingRule
{
    public List<string> Check(ActionPlanMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMappingTitleLengthRule(int MaxLength) : IActionPlanMappingRule
{
    public List<string> Check(ActionPlanMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanMappingLeaveDaysRangeRule(int Min, int Max) : IActionPlanMappingRule
{
    public List<string> Check(ActionPlanMapping item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanMappingAllowedTitleRule(List<string> Allowed) : IActionPlanMappingRule
{
    public List<string> Check(ActionPlanMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan mapping.</summary>
public sealed record ActionPlanMappingValidator(List<IActionPlanMappingRule> Rules)
{
    public List<string> Validate(ActionPlanMapping item)
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

    public void RequireValid(ActionPlanMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanMapping> items)
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

/// <summary>Functions of action plan mapping rules.</summary>
public static class ActionPlanMappingRules
{
    public static ActionPlanMappingValidator DefaultActionPlanMappingValidator()
    {
        return new ActionPlanMappingValidator(
            new List<IActionPlanMappingRule>
            {
                new ActionPlanMappingCodeLengthRule(1000),
                new ActionPlanMappingTitleLengthRule(255),
                new ActionPlanMappingLeaveDaysRangeRule(3600, 86400),
                new ActionPlanMappingAllowedTitleRule(new List<string> { "capped", "default" }),
            });
    }
}
