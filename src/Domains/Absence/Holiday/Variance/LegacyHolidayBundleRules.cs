namespace Acme.Workforce.Absence.Holiday.Variance;

/// <summary>A rule a legacy holiday bundle must pass. Check returns its problems, or none.</summary>
public interface ILegacyHolidayBundleRule
{
    List<string> Check(LegacyHolidayBundle item);
}

public sealed record LegacyHolidayBundleMinimumRangeRule(double Min, double Max) : ILegacyHolidayBundleRule
{
    public List<string> Check(LegacyHolidayBundle item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayBundleRequiredCodeRule(string Message) : ILegacyHolidayBundleRule
{
    public List<string> Check(LegacyHolidayBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayBundleRatingRangeRule(double Min, double Max) : ILegacyHolidayBundleRule
{
    public List<string> Check(LegacyHolidayBundle item)
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

public sealed record LegacyHolidayBundleRequiredRegionRule(string Message) : ILegacyHolidayBundleRule
{
    public List<string> Check(LegacyHolidayBundle item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayBundleRateRangeRule(double Min, double Max) : ILegacyHolidayBundleRule
{
    public List<string> Check(LegacyHolidayBundle item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy holiday bundle.</summary>
public sealed record LegacyHolidayBundleValidator(List<ILegacyHolidayBundleRule> Rules)
{
    public List<string> Validate(LegacyHolidayBundle item)
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

    public void RequireValid(LegacyHolidayBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyHolidayBundle> items)
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

/// <summary>Functions of legacy holiday bundle rules.</summary>
public static class LegacyHolidayBundleRules
{
    public static LegacyHolidayBundleValidator DefaultLegacyHolidayBundleValidator()
    {
        return new LegacyHolidayBundleValidator(
            new List<ILegacyHolidayBundleRule>
            {
                new LegacyHolidayBundleMinimumRangeRule(1.7, 1113.3),
                new LegacyHolidayBundleRequiredCodeRule("legacy holiday bundle needs an entry"),
                new LegacyHolidayBundleRatingRangeRule(1.3, 3632.5),
                new LegacyHolidayBundleRequiredRegionRule("legacy holiday bundle needs this field"),
                new LegacyHolidayBundleRateRangeRule(8.7, 1690.4),
            });
    }
}
