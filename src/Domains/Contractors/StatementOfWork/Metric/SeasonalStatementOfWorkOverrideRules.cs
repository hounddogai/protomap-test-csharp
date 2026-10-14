namespace Acme.Workforce.Contractors.StatementOfWork.Metric;

/// <summary>A rule a seasonal statement of work override must pass. Check returns its problems, or none.</summary>
public interface ISeasonalStatementOfWorkOverrideRule
{
    List<string> Check(SeasonalStatementOfWorkOverride item);
}

public sealed record SeasonalStatementOfWorkOverrideRequiredCodeRule(string Message) : ISeasonalStatementOfWorkOverrideRule
{
    public List<string> Check(SeasonalStatementOfWorkOverride item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalStatementOfWorkOverrideRequiredExternalRefRule(string Message) : ISeasonalStatementOfWorkOverrideRule
{
    public List<string> Check(SeasonalStatementOfWorkOverride item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalStatementOfWorkOverrideRatioRangeRule(double Min, double Max) : ISeasonalStatementOfWorkOverrideRule
{
    public List<string> Check(SeasonalStatementOfWorkOverride item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalStatementOfWorkOverrideTitleLengthRule(int MaxLength) : ISeasonalStatementOfWorkOverrideRule
{
    public List<string> Check(SeasonalStatementOfWorkOverride item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalStatementOfWorkOverrideRequiredJobCodeRule(string Message) : ISeasonalStatementOfWorkOverrideRule
{
    public List<string> Check(SeasonalStatementOfWorkOverride item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalStatementOfWorkOverrideAmountRangeRule(double Min, double Max) : ISeasonalStatementOfWorkOverrideRule
{
    public List<string> Check(SeasonalStatementOfWorkOverride item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal statement of work override.</summary>
public sealed record SeasonalStatementOfWorkOverrideValidator(List<ISeasonalStatementOfWorkOverrideRule> Rules)
{
    public List<string> Validate(SeasonalStatementOfWorkOverride item)
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

    public void RequireValid(SeasonalStatementOfWorkOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalStatementOfWorkOverride> items)
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

/// <summary>Functions of seasonal statement of work override rules.</summary>
public static class SeasonalStatementOfWorkOverrideRules
{
    public static SeasonalStatementOfWorkOverrideValidator DefaultSeasonalStatementOfWorkOverrideValidator()
    {
        return new SeasonalStatementOfWorkOverrideValidator(
            new List<ISeasonalStatementOfWorkOverrideRule>
            {
                new SeasonalStatementOfWorkOverrideRequiredCodeRule(
                    "seasonal statement of work override needs an entry"),
                new SeasonalStatementOfWorkOverrideRequiredExternalRefRule(
                    "seasonal statement of work override needs an entry"),
                new SeasonalStatementOfWorkOverrideRatioRangeRule(4.7, 2363.6),
                new SeasonalStatementOfWorkOverrideTitleLengthRule(16),
                new SeasonalStatementOfWorkOverrideRequiredJobCodeRule(
                    "seasonal statement of work override needs this field"),
                new SeasonalStatementOfWorkOverrideAmountRangeRule(7.0, 913.6),
            });
    }
}
