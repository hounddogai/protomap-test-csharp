namespace Acme.Workforce.Absence.Holiday.Variance;

/// <summary>A rule a adaptive holiday variance must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveHolidayVarianceRule
{
    List<string> Check(AdaptiveHolidayVariance item);
}

public sealed record AdaptiveHolidayVarianceWindowDaysRangeRule(int Min, int Max) : IAdaptiveHolidayVarianceRule
{
    public List<string> Check(AdaptiveHolidayVariance item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHolidayVarianceRequiredLabelRule(string Message) : IAdaptiveHolidayVarianceRule
{
    public List<string> Check(AdaptiveHolidayVariance item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHolidayVarianceCodeLengthRule(int MaxLength) : IAdaptiveHolidayVarianceRule
{
    public List<string> Check(AdaptiveHolidayVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveHolidayVarianceRequiredCodeRule(string Message) : IAdaptiveHolidayVarianceRule
{
    public List<string> Check(AdaptiveHolidayVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive holiday variance.</summary>
public sealed record AdaptiveHolidayVarianceValidator(List<IAdaptiveHolidayVarianceRule> Rules)
{
    public List<string> Validate(AdaptiveHolidayVariance item)
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

    public void RequireValid(AdaptiveHolidayVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveHolidayVariance> items)
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

/// <summary>Functions of adaptive holiday variance rules.</summary>
public static class AdaptiveHolidayVarianceRules
{
    public static AdaptiveHolidayVarianceValidator DefaultAdaptiveHolidayVarianceValidator()
    {
        return new AdaptiveHolidayVarianceValidator(
            new List<IAdaptiveHolidayVarianceRule>
            {
                new AdaptiveHolidayVarianceWindowDaysRangeRule(80, 120),
                new AdaptiveHolidayVarianceRequiredLabelRule("adaptive holiday variance needs a value"),
                new AdaptiveHolidayVarianceCodeLengthRule(16),
                new AdaptiveHolidayVarianceRequiredCodeRule("adaptive holiday variance needs an entry"),
            });
    }
}
