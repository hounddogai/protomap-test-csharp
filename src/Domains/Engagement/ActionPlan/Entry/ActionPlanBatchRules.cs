namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan batch must pass. Check returns its problems, or none.</summary>
public interface IActionPlanBatchRule
{
    List<string> Check(ActionPlanBatch item);
}

public sealed record ActionPlanBatchSkillLengthRule(int MaxLength) : IActionPlanBatchRule
{
    public List<string> Check(ActionPlanBatch item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBatchAllowedCodeRule(List<string> Allowed) : IActionPlanBatchRule
{
    public List<string> Check(ActionPlanBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanBatchActiveGuardRule(int Limit) : IActionPlanBatchRule
{
    public List<string> Check(ActionPlanBatch item)
    {
        if (item.Active && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan batch.</summary>
public sealed record ActionPlanBatchValidator(List<IActionPlanBatchRule> Rules)
{
    public List<string> Validate(ActionPlanBatch item)
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

    public void RequireValid(ActionPlanBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanBatch> items)
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

/// <summary>Functions of action plan batch rules.</summary>
public static class ActionPlanBatchRules
{
    public static ActionPlanBatchValidator DefaultActionPlanBatchValidator()
    {
        return new ActionPlanBatchValidator(
            new List<IActionPlanBatchRule>
            {
                new ActionPlanBatchSkillLengthRule(1000),
                new ActionPlanBatchAllowedCodeRule(new List<string> { "scheduled", "rolling" }),
                new ActionPlanBatchActiveGuardRule(255),
            });
    }
}
