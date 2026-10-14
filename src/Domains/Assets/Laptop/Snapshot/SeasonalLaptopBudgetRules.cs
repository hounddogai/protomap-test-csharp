namespace Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>A rule a seasonal laptop budget must pass. Check returns its problems, or none.</summary>
public interface ISeasonalLaptopBudgetRule
{
    List<string> Check(SeasonalLaptopBudget item);
}

public sealed record SeasonalLaptopBudgetHoursRangeRule(double Min, double Max) : ISeasonalLaptopBudgetRule
{
    public List<string> Check(SeasonalLaptopBudget item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLaptopBudgetRequiredBadgeRule(string Message) : ISeasonalLaptopBudgetRule
{
    public List<string> Check(SeasonalLaptopBudget item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLaptopBudgetRequiredCodeRule(string Message) : ISeasonalLaptopBudgetRule
{
    public List<string> Check(SeasonalLaptopBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLaptopBudgetVisibleGuardRule(double Limit) : ISeasonalLaptopBudgetRule
{
    public List<string> Check(SeasonalLaptopBudget item)
    {
        if (item.Visible && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalLaptopBudgetNameLengthRule(int MaxLength) : ISeasonalLaptopBudgetRule
{
    public List<string> Check(SeasonalLaptopBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal laptop budget.</summary>
public sealed record SeasonalLaptopBudgetValidator(List<ISeasonalLaptopBudgetRule> Rules)
{
    public List<string> Validate(SeasonalLaptopBudget item)
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

    public void RequireValid(SeasonalLaptopBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalLaptopBudget> items)
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

/// <summary>Functions of seasonal laptop budget rules.</summary>
public static class SeasonalLaptopBudgetRules
{
    public static SeasonalLaptopBudgetValidator DefaultSeasonalLaptopBudgetValidator()
    {
        return new SeasonalLaptopBudgetValidator(
            new List<ISeasonalLaptopBudgetRule>
            {
                new SeasonalLaptopBudgetHoursRangeRule(6.0, 1358.5),
                new SeasonalLaptopBudgetRequiredBadgeRule("seasonal laptop budget needs a value"),
                new SeasonalLaptopBudgetRequiredCodeRule("seasonal laptop budget needs an entry"),
                new SeasonalLaptopBudgetVisibleGuardRule(8.4),
                new SeasonalLaptopBudgetNameLengthRule(32),
            });
    }
}
