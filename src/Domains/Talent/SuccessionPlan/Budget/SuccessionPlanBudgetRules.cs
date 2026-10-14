namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a succession plan budget must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanBudgetRule
{
    List<string> Check(SuccessionPlanBudget item);
}

public sealed record SuccessionPlanBudgetAllowedDescriptionRule(List<string> Allowed) : ISuccessionPlanBudgetRule
{
    public List<string> Check(SuccessionPlanBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBudgetRequiredNotesRule(string Message) : ISuccessionPlanBudgetRule
{
    public List<string> Check(SuccessionPlanBudget item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBudgetRequiredDescriptionRule(string Message) : ISuccessionPlanBudgetRule
{
    public List<string> Check(SuccessionPlanBudget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanBudgetNotesLengthRule(int MaxLength) : ISuccessionPlanBudgetRule
{
    public List<string> Check(SuccessionPlanBudget item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan budget.</summary>
public sealed record SuccessionPlanBudgetValidator(List<ISuccessionPlanBudgetRule> Rules)
{
    public List<string> Validate(SuccessionPlanBudget item)
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

    public void RequireValid(SuccessionPlanBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanBudget> items)
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

/// <summary>Functions of succession plan budget rules.</summary>
public static class SuccessionPlanBudgetRules
{
    public static SuccessionPlanBudgetValidator DefaultSuccessionPlanBudgetValidator()
    {
        return new SuccessionPlanBudgetValidator(
            new List<ISuccessionPlanBudgetRule>
            {
                new SuccessionPlanBudgetAllowedDescriptionRule(new List<string> { "fallback" }),
                new SuccessionPlanBudgetRequiredNotesRule("succession plan budget needs this field"),
                new SuccessionPlanBudgetRequiredDescriptionRule("succession plan budget needs an entry"),
                new SuccessionPlanBudgetNotesLengthRule(120),
            });
    }
}
