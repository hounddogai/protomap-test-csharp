namespace Acme.Workforce.Absence.Accrual.Template;

/// <summary>A rule a seasonal accrual template must pass. Check returns its problems, or none.</summary>
public interface ISeasonalAccrualTemplateRule
{
    List<string> Check(SeasonalAccrualTemplate item);
}

public sealed record SeasonalAccrualTemplateCategoryLengthRule(int MaxLength) : ISeasonalAccrualTemplateRule
{
    public List<string> Check(SeasonalAccrualTemplate item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualTemplateGraceDaysRangeRule(int Min, int Max) : ISeasonalAccrualTemplateRule
{
    public List<string> Check(SeasonalAccrualTemplate item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalAccrualTemplateCodeLengthRule(int MaxLength) : ISeasonalAccrualTemplateRule
{
    public List<string> Check(SeasonalAccrualTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal accrual template.</summary>
public sealed record SeasonalAccrualTemplateValidator(List<ISeasonalAccrualTemplateRule> Rules)
{
    public List<string> Validate(SeasonalAccrualTemplate item)
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

    public void RequireValid(SeasonalAccrualTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalAccrualTemplate> items)
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

/// <summary>Functions of seasonal accrual template rules.</summary>
public static class SeasonalAccrualTemplateRules
{
    public static SeasonalAccrualTemplateValidator DefaultSeasonalAccrualTemplateValidator()
    {
        return new SeasonalAccrualTemplateValidator(
            new List<ISeasonalAccrualTemplateRule>
            {
                new SeasonalAccrualTemplateCategoryLengthRule(64),
                new SeasonalAccrualTemplateGraceDaysRangeRule(16, 255),
                new SeasonalAccrualTemplateCodeLengthRule(64),
            });
    }
}
