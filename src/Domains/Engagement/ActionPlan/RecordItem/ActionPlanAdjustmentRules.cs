namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>A rule a action plan adjustment must pass. Check returns its problems, or none.</summary>
public interface IActionPlanAdjustmentRule
{
    List<string> Check(ActionPlanAdjustment item);
}

public sealed record ActionPlanAdjustmentCountryLengthRule(int MaxLength) : IActionPlanAdjustmentRule
{
    public List<string> Check(ActionPlanAdjustment item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAdjustmentTitleLengthRule(int MaxLength) : IActionPlanAdjustmentRule
{
    public List<string> Check(ActionPlanAdjustment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanAdjustmentEndsAtRangeRule(int Min, int Max) : IActionPlanAdjustmentRule
{
    public List<string> Check(ActionPlanAdjustment item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a action plan adjustment.</summary>
public sealed record ActionPlanAdjustmentValidator(List<IActionPlanAdjustmentRule> Rules)
{
    public List<string> Validate(ActionPlanAdjustment item)
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

    public void RequireValid(ActionPlanAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanAdjustment> items)
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

/// <summary>Functions of action plan adjustment rules.</summary>
public static class ActionPlanAdjustmentRules
{
    public static ActionPlanAdjustmentValidator DefaultActionPlanAdjustmentValidator()
    {
        return new ActionPlanAdjustmentValidator(
            new List<IActionPlanAdjustmentRule>
            {
                new ActionPlanAdjustmentCountryLengthRule(1000),
                new ActionPlanAdjustmentTitleLengthRule(86400),
                new ActionPlanAdjustmentEndsAtRangeRule(64, 64),
            });
    }
}
