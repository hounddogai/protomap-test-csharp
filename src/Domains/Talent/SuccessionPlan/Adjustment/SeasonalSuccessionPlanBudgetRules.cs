namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>A rule a seasonal succession plan budget must pass. Check returns its problems, or none.</summary>
public interface ISeasonalSuccessionPlanBudgetRule
{
    List<string> Check(SeasonalSuccessionPlanBudget item);
}

public sealed record SeasonalSuccessionPlanBudgetSourceLengthRule(int MaxLength) : ISeasonalSuccessionPlanBudgetRule
{
    public List<string> Check(SeasonalSuccessionPlanBudget item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanBudgetRatingRangeRule(double Min, double Max) : ISeasonalSuccessionPlanBudgetRule
{
    public List<string> Check(SeasonalSuccessionPlanBudget item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalSuccessionPlanBudgetCurrencyLengthRule(int MaxLength) : ISeasonalSuccessionPlanBudgetRule
{
    public List<string> Check(SeasonalSuccessionPlanBudget item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalSuccessionPlanBudgetRatioRangeRule(double Min, double Max) : ISeasonalSuccessionPlanBudgetRule
{
    public List<string> Check(SeasonalSuccessionPlanBudget item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal succession plan budget.</summary>
public sealed record SeasonalSuccessionPlanBudgetValidator(List<ISeasonalSuccessionPlanBudgetRule> Rules)
{
    public List<string> Validate(SeasonalSuccessionPlanBudget item)
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

    public void RequireValid(SeasonalSuccessionPlanBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalSuccessionPlanBudget> items)
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

/// <summary>Functions of seasonal succession plan budget rules.</summary>
public static class SeasonalSuccessionPlanBudgetRules
{
    public static SeasonalSuccessionPlanBudgetValidator DefaultSeasonalSuccessionPlanBudgetValidator()
    {
        return new SeasonalSuccessionPlanBudgetValidator(
            new List<ISeasonalSuccessionPlanBudgetRule>
            {
                new SeasonalSuccessionPlanBudgetSourceLengthRule(80),
                new SeasonalSuccessionPlanBudgetRatingRangeRule(5.6, 2492.6),
                new SeasonalSuccessionPlanBudgetCurrencyLengthRule(32),
                new SeasonalSuccessionPlanBudgetRatioRangeRule(6.3, 465.7),
            });
    }
}
