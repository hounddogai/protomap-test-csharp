namespace Acme.Workforce.Absence.ReturnPlan.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan item must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanItemRule
{
    List<string> Check(ReturnPlanItem item);
}

public sealed record ReturnPlanItemAllowedTitleRule(List<string> Allowed) : IReturnPlanItemRule
{
    public List<string> Check(ReturnPlanItem item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanItemRequiredSkillRule(string Message) : IReturnPlanItemRule
{
    public List<string> Check(ReturnPlanItem item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanItemTitleLengthRule(int MaxLength) : IReturnPlanItemRule
{
    public List<string> Check(ReturnPlanItem item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanItemRequiredCodeRule(string Message) : IReturnPlanItemRule
{
    public List<string> Check(ReturnPlanItem item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan item.</summary>
public sealed record ReturnPlanItemValidator(List<IReturnPlanItemRule> Rules)
{
    public List<string> Validate(ReturnPlanItem item)
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

    public void RequireValid(ReturnPlanItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanItem> items)
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

/// <summary>Functions of return plan item rules.</summary>
public static class ReturnPlanItemRules
{
    public static ReturnPlanItemValidator DefaultReturnPlanItemValidator()
    {
        return new ReturnPlanItemValidator(
            new List<IReturnPlanItemRule>
            {
                new ReturnPlanItemAllowedTitleRule(new List<string> { "scheduled" }),
                new ReturnPlanItemRequiredSkillRule("return plan item needs a value"),
                new ReturnPlanItemTitleLengthRule(32),
                new ReturnPlanItemRequiredCodeRule("return plan item needs an entry"),
            });
    }
}
