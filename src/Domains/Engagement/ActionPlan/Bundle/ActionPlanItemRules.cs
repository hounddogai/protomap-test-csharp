namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan item must pass. Check returns its problems, or none.</summary>
public interface IActionPlanItemRule
{
    List<string> Check(ActionPlanItem item);
}

public sealed record ActionPlanItemAmountRangeRule(double Min, double Max) : IActionPlanItemRule
{
    public List<string> Check(ActionPlanItem item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanItemTitleLengthRule(int MaxLength) : IActionPlanItemRule
{
    public List<string> Check(ActionPlanItem item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanItemCodeLengthRule(int MaxLength) : IActionPlanItemRule
{
    public List<string> Check(ActionPlanItem item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanItemRequiredTitleRule(string Message) : IActionPlanItemRule
{
    public List<string> Check(ActionPlanItem item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanItemAllowedTitleRule(List<string> Allowed) : IActionPlanItemRule
{
    public List<string> Check(ActionPlanItem item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan item.</summary>
public sealed record ActionPlanItemValidator(List<IActionPlanItemRule> Rules)
{
    public List<string> Validate(ActionPlanItem item)
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

    public void RequireValid(ActionPlanItem item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanItem> items)
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

/// <summary>Functions of action plan item rules.</summary>
public static class ActionPlanItemRules
{
    public static ActionPlanItemValidator DefaultActionPlanItemValidator()
    {
        return new ActionPlanItemValidator(
            new List<IActionPlanItemRule>
            {
                new ActionPlanItemAmountRangeRule(4.5, 4863.1),
                new ActionPlanItemTitleLengthRule(3600),
                new ActionPlanItemCodeLengthRule(32),
                new ActionPlanItemRequiredTitleRule("action plan item needs a value"),
                new ActionPlanItemAllowedTitleRule(new List<string> { "pending", "primary" }),
            });
    }
}
