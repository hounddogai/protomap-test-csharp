namespace Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>A rule a seasonal refresh allocation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalRefreshAllocationRule
{
    List<string> Check(SeasonalRefreshAllocation item);
}

public sealed record SeasonalRefreshAllocationRequiredCodeRule(string Message) : ISeasonalRefreshAllocationRule
{
    public List<string> Check(SeasonalRefreshAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshAllocationGradeLengthRule(int MaxLength) : ISeasonalRefreshAllocationRule
{
    public List<string> Check(SeasonalRefreshAllocation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshAllocationDiscountRangeRule(double Min, double Max) : ISeasonalRefreshAllocationRule
{
    public List<string> Check(SeasonalRefreshAllocation item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalRefreshAllocationStartsAtRangeRule(int Min, int Max) : ISeasonalRefreshAllocationRule
{
    public List<string> Check(SeasonalRefreshAllocation item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalRefreshAllocationHoursRangeRule(double Min, double Max) : ISeasonalRefreshAllocationRule
{
    public List<string> Check(SeasonalRefreshAllocation item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal refresh allocation.</summary>
public sealed record SeasonalRefreshAllocationValidator(List<ISeasonalRefreshAllocationRule> Rules)
{
    public List<string> Validate(SeasonalRefreshAllocation item)
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

    public void RequireValid(SeasonalRefreshAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalRefreshAllocation> items)
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

/// <summary>Functions of seasonal refresh allocation rules.</summary>
public static class SeasonalRefreshAllocationRules
{
    public static SeasonalRefreshAllocationValidator DefaultSeasonalRefreshAllocationValidator()
    {
        return new SeasonalRefreshAllocationValidator(
            new List<ISeasonalRefreshAllocationRule>
            {
                new SeasonalRefreshAllocationRequiredCodeRule("seasonal refresh allocation needs this field"),
                new SeasonalRefreshAllocationGradeLengthRule(120),
                new SeasonalRefreshAllocationDiscountRangeRule(6.5, 4089.4),
                new SeasonalRefreshAllocationStartsAtRangeRule(32, 255),
                new SeasonalRefreshAllocationHoursRangeRule(5.2, 4073.6),
            });
    }
}
