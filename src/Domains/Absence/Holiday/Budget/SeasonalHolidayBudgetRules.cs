namespace Acme.Workforce.Absence.Holiday.Budget;

/// <summary>A rule a seasonal holiday budget must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayBudgetRule
{
    List<string> Check(SeasonalHolidayBudget item);
}

public sealed record SeasonalHolidayBudgetSourceLengthRule(int MaxLength) : ISeasonalHolidayBudgetRule
{
    public List<string> Check(SeasonalHolidayBudget item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayBudgetRequiredTitleRule(string Message) : ISeasonalHolidayBudgetRule
{
    public List<string> Check(SeasonalHolidayBudget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayBudgetTeamLengthRule(int MaxLength) : ISeasonalHolidayBudgetRule
{
    public List<string> Check(SeasonalHolidayBudget item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayBudgetCodePrefixRule(string Prefix) : ISeasonalHolidayBudgetRule
{
    public List<string> Check(SeasonalHolidayBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal holiday budget.</summary>
public sealed record SeasonalHolidayBudgetValidator(List<ISeasonalHolidayBudgetRule> Rules)
{
    public List<string> Validate(SeasonalHolidayBudget item)
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

    public void RequireValid(SeasonalHolidayBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayBudget> items)
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

/// <summary>Functions of seasonal holiday budget rules.</summary>
public static class SeasonalHolidayBudgetRules
{
    public static SeasonalHolidayBudgetValidator DefaultSeasonalHolidayBudgetValidator()
    {
        return new SeasonalHolidayBudgetValidator(
            new List<ISeasonalHolidayBudgetRule>
            {
                new SeasonalHolidayBudgetSourceLengthRule(255),
                new SeasonalHolidayBudgetRequiredTitleRule("seasonal holiday budget needs a value"),
                new SeasonalHolidayBudgetTeamLengthRule(16),
                new SeasonalHolidayBudgetCodePrefixRule("REF"),
            });
    }
}
