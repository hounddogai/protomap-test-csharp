namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall budget must pass. Check returns its problems, or none.</summary>
public interface ITownHallBudgetRule
{
    List<string> Check(TownHallBudget item);
}

public sealed record TownHallBudgetDescriptionLengthRule(int MaxLength) : ITownHallBudgetRule
{
    public List<string> Check(TownHallBudget item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBudgetAllowedDescriptionRule(List<string> Allowed) : ITownHallBudgetRule
{
    public List<string> Check(TownHallBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record TownHallBudgetRequiredNameRule(string Message) : ITownHallBudgetRule
{
    public List<string> Check(TownHallBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallBudgetRequiredCostCenterRule(string Message) : ITownHallBudgetRule
{
    public List<string> Check(TownHallBudget item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TownHallBudgetStartsAtRangeRule(int Min, int Max) : ITownHallBudgetRule
{
    public List<string> Check(TownHallBudget item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall budget.</summary>
public sealed record TownHallBudgetValidator(List<ITownHallBudgetRule> Rules)
{
    public List<string> Validate(TownHallBudget item)
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

    public void RequireValid(TownHallBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallBudget> items)
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

/// <summary>Functions of town hall budget rules.</summary>
public static class TownHallBudgetRules
{
    public static TownHallBudgetValidator DefaultTownHallBudgetValidator()
    {
        return new TownHallBudgetValidator(
            new List<ITownHallBudgetRule>
            {
                new TownHallBudgetDescriptionLengthRule(1000),
                new TownHallBudgetAllowedDescriptionRule(new List<string> { "lenient" }),
                new TownHallBudgetRequiredNameRule("town hall budget needs a value"),
                new TownHallBudgetRequiredCostCenterRule("town hall budget needs a value"),
                new TownHallBudgetStartsAtRangeRule(120, 3600),
            });
    }
}
