namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>A rule a seasonal garnishment allocation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalGarnishmentAllocationRule
{
    List<string> Check(SeasonalGarnishmentAllocation item);
}

public sealed record SeasonalGarnishmentAllocationGradeLengthRule(int MaxLength) : ISeasonalGarnishmentAllocationRule
{
    public List<string> Check(SeasonalGarnishmentAllocation item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalGarnishmentAllocationCodeLengthRule(int MaxLength) : ISeasonalGarnishmentAllocationRule
{
    public List<string> Check(SeasonalGarnishmentAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalGarnishmentAllocationGraceDaysRangeRule(int Min, int Max) : ISeasonalGarnishmentAllocationRule
{
    public List<string> Check(SeasonalGarnishmentAllocation item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalGarnishmentAllocationUnitLengthRule(int MaxLength) : ISeasonalGarnishmentAllocationRule
{
    public List<string> Check(SeasonalGarnishmentAllocation item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalGarnishmentAllocationRequiredCodeRule(string Message) : ISeasonalGarnishmentAllocationRule
{
    public List<string> Check(SeasonalGarnishmentAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal garnishment allocation.</summary>
public sealed record SeasonalGarnishmentAllocationValidator(List<ISeasonalGarnishmentAllocationRule> Rules)
{
    public List<string> Validate(SeasonalGarnishmentAllocation item)
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

    public void RequireValid(SeasonalGarnishmentAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalGarnishmentAllocation> items)
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

/// <summary>Functions of seasonal garnishment allocation rules.</summary>
public static class SeasonalGarnishmentAllocationRules
{
    public static SeasonalGarnishmentAllocationValidator DefaultSeasonalGarnishmentAllocationValidator()
    {
        return new SeasonalGarnishmentAllocationValidator(
            new List<ISeasonalGarnishmentAllocationRule>
            {
                new SeasonalGarnishmentAllocationGradeLengthRule(255),
                new SeasonalGarnishmentAllocationCodeLengthRule(3600),
                new SeasonalGarnishmentAllocationGraceDaysRangeRule(64, 86400),
                new SeasonalGarnishmentAllocationUnitLengthRule(16),
                new SeasonalGarnishmentAllocationRequiredCodeRule("seasonal garnishment allocation needs an entry"),
            });
    }
}
