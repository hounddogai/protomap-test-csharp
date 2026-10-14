namespace Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>A rule a regional laptop budget must pass. Check returns its problems, or none.</summary>
public interface IRegionalLaptopBudgetRule
{
    List<string> Check(RegionalLaptopBudget item);
}

public sealed record RegionalLaptopBudgetSequenceRangeRule(int Min, int Max) : IRegionalLaptopBudgetRule
{
    public List<string> Check(RegionalLaptopBudget item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalLaptopBudgetRequiredNameRule(string Message) : IRegionalLaptopBudgetRule
{
    public List<string> Check(RegionalLaptopBudget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLaptopBudgetTenureMonthsRangeRule(int Min, int Max) : IRegionalLaptopBudgetRule
{
    public List<string> Check(RegionalLaptopBudget item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional laptop budget.</summary>
public sealed record RegionalLaptopBudgetValidator(List<IRegionalLaptopBudgetRule> Rules)
{
    public List<string> Validate(RegionalLaptopBudget item)
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

    public void RequireValid(RegionalLaptopBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalLaptopBudget> items)
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

/// <summary>Functions of regional laptop budget rules.</summary>
public static class RegionalLaptopBudgetRules
{
    public static RegionalLaptopBudgetValidator DefaultRegionalLaptopBudgetValidator()
    {
        return new RegionalLaptopBudgetValidator(
            new List<IRegionalLaptopBudgetRule>
            {
                new RegionalLaptopBudgetSequenceRangeRule(16, 3600),
                new RegionalLaptopBudgetRequiredNameRule("regional laptop budget needs a value"),
                new RegionalLaptopBudgetTenureMonthsRangeRule(255, 1000),
            });
    }
}
