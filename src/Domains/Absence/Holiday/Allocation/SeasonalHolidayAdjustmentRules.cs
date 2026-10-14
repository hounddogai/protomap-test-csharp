namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal holiday adjustment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayAdjustmentRule
{
    List<string> Check(SeasonalHolidayAdjustment item);
}

public sealed record SeasonalHolidayAdjustmentCodeLengthRule(int MaxLength) : ISeasonalHolidayAdjustmentRule
{
    public List<string> Check(SeasonalHolidayAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAdjustmentLocaleLengthRule(int MaxLength) : ISeasonalHolidayAdjustmentRule
{
    public List<string> Check(SeasonalHolidayAdjustment item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAdjustmentAllowedLocaleRule(List<string> Allowed) : ISeasonalHolidayAdjustmentRule
{
    public List<string> Check(SeasonalHolidayAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAdjustmentRequiredTitleRule(string Message) : ISeasonalHolidayAdjustmentRule
{
    public List<string> Check(SeasonalHolidayAdjustment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal holiday adjustment.</summary>
public sealed record SeasonalHolidayAdjustmentValidator(List<ISeasonalHolidayAdjustmentRule> Rules)
{
    public List<string> Validate(SeasonalHolidayAdjustment item)
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

    public void RequireValid(SeasonalHolidayAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayAdjustment> items)
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

/// <summary>Functions of seasonal holiday adjustment rules.</summary>
public static class SeasonalHolidayAdjustmentRules
{
    public static SeasonalHolidayAdjustmentValidator DefaultSeasonalHolidayAdjustmentValidator()
    {
        return new SeasonalHolidayAdjustmentValidator(
            new List<ISeasonalHolidayAdjustmentRule>
            {
                new SeasonalHolidayAdjustmentCodeLengthRule(255),
                new SeasonalHolidayAdjustmentLocaleLengthRule(1000),
                new SeasonalHolidayAdjustmentAllowedLocaleRule(new List<string> { "tiered" }),
                new SeasonalHolidayAdjustmentRequiredTitleRule("seasonal holiday adjustment needs an entry"),
            });
    }
}
