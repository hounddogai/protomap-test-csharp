namespace Acme.Workforce.Orgdesign.Reorg.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg budget must pass. Check returns its problems, or none.</summary>
public interface IReorgBudgetRule
{
    List<string> Check(ReorgBudget item);
}

public sealed record ReorgBudgetRequiredTimezoneRule(string Message) : IReorgBudgetRule
{
    public List<string> Check(ReorgBudget item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgBudgetAllowedTimezoneRule(List<string> Allowed) : IReorgBudgetRule
{
    public List<string> Check(ReorgBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Timezone)))
        {
            return new List<string> { $"timezone {item.Timezone} is stale" };
        }
        return new List<string>();
    }
}

public sealed record ReorgBudgetRequiredDescriptionRule(string Message) : IReorgBudgetRule
{
    public List<string> Check(ReorgBudget item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgBudgetDescriptionLengthRule(int MaxLength) : IReorgBudgetRule
{
    public List<string> Check(ReorgBudget item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg budget.</summary>
public sealed record ReorgBudgetValidator(List<IReorgBudgetRule> Rules)
{
    public List<string> Validate(ReorgBudget item)
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

    public void RequireValid(ReorgBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgBudget> items)
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

/// <summary>Functions of reorg budget rules.</summary>
public static class ReorgBudgetRules
{
    public static ReorgBudgetValidator DefaultReorgBudgetValidator()
    {
        return new ReorgBudgetValidator(
            new List<IReorgBudgetRule>
            {
                new ReorgBudgetRequiredTimezoneRule("reorg budget needs this field"),
                new ReorgBudgetAllowedTimezoneRule(new List<string>()),
                new ReorgBudgetRequiredDescriptionRule("reorg budget needs this field"),
                new ReorgBudgetDescriptionLengthRule(1000),
            });
    }
}
