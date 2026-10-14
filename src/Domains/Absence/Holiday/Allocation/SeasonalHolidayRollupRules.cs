namespace Acme.Workforce.Absence.Holiday.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal holiday rollup must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayRollupRule
{
    List<string> Check(SeasonalHolidayRollup item);
}

public sealed record SeasonalHolidayRollupTeamLengthRule(int MaxLength) : ISeasonalHolidayRollupRule
{
    public List<string> Check(SeasonalHolidayRollup item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayRollupRequiredCodeRule(string Message) : ISeasonalHolidayRollupRule
{
    public List<string> Check(SeasonalHolidayRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayRollupAllowedLabelRule(List<string> Allowed) : ISeasonalHolidayRollupRule
{
    public List<string> Check(SeasonalHolidayRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayRollupEndsAtRangeRule(int Min, int Max) : ISeasonalHolidayRollupRule
{
    public List<string> Check(SeasonalHolidayRollup item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayRollupContactEmailLengthRule(int MaxLength) : ISeasonalHolidayRollupRule
{
    public List<string> Check(SeasonalHolidayRollup item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayRollupCostRangeRule(double Min, double Max) : ISeasonalHolidayRollupRule
{
    public List<string> Check(SeasonalHolidayRollup item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a seasonal holiday rollup.</summary>
public sealed record SeasonalHolidayRollupValidator(List<ISeasonalHolidayRollupRule> Rules)
{
    public List<string> Validate(SeasonalHolidayRollup item)
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

    public void RequireValid(SeasonalHolidayRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayRollup> items)
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

/// <summary>Functions of seasonal holiday rollup rules.</summary>
public static class SeasonalHolidayRollupRules
{
    public static SeasonalHolidayRollupValidator DefaultSeasonalHolidayRollupValidator()
    {
        return new SeasonalHolidayRollupValidator(
            new List<ISeasonalHolidayRollupRule>
            {
                new SeasonalHolidayRollupTeamLengthRule(120),
                new SeasonalHolidayRollupRequiredCodeRule("seasonal holiday rollup needs a value"),
                new SeasonalHolidayRollupAllowedLabelRule(new List<string> { "manual", "seasonal" }),
                new SeasonalHolidayRollupEndsAtRangeRule(120, 3600),
                new SeasonalHolidayRollupContactEmailLengthRule(3600),
                new SeasonalHolidayRollupCostRangeRule(9.5, 4740.3),
            });
    }
}
