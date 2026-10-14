namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>A rule a weighted return plan override must pass. Check returns its problems, or none.</summary>
public interface IWeightedReturnPlanOverrideRule
{
    List<string> Check(WeightedReturnPlanOverride item);
}

public sealed record WeightedReturnPlanOverrideRequiredJobCodeRule(string Message) : IWeightedReturnPlanOverrideRule
{
    public List<string> Check(WeightedReturnPlanOverride item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedReturnPlanOverrideLabelLengthRule(int MaxLength) : IWeightedReturnPlanOverrideRule
{
    public List<string> Check(WeightedReturnPlanOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedReturnPlanOverrideRequiredLabelRule(string Message) : IWeightedReturnPlanOverrideRule
{
    public List<string> Check(WeightedReturnPlanOverride item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedReturnPlanOverrideRequiredCategoryRule(string Message) : IWeightedReturnPlanOverrideRule
{
    public List<string> Check(WeightedReturnPlanOverride item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted return plan override.</summary>
public sealed record WeightedReturnPlanOverrideValidator(List<IWeightedReturnPlanOverrideRule> Rules)
{
    public List<string> Validate(WeightedReturnPlanOverride item)
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

    public void RequireValid(WeightedReturnPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedReturnPlanOverride> items)
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

/// <summary>Functions of weighted return plan override rules.</summary>
public static class WeightedReturnPlanOverrideRules
{
    public static WeightedReturnPlanOverrideValidator DefaultWeightedReturnPlanOverrideValidator()
    {
        return new WeightedReturnPlanOverrideValidator(
            new List<IWeightedReturnPlanOverrideRule>
            {
                new WeightedReturnPlanOverrideRequiredJobCodeRule("weighted return plan override needs a value"),
                new WeightedReturnPlanOverrideLabelLengthRule(120),
                new WeightedReturnPlanOverrideRequiredLabelRule("weighted return plan override needs this field"),
                new WeightedReturnPlanOverrideRequiredCategoryRule("weighted return plan override needs an entry"),
            });
    }
}
