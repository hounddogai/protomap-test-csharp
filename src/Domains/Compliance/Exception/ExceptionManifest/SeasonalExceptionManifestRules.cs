namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>A rule a seasonal exception manifest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalExceptionManifestRule
{
    List<string> Check(SeasonalExceptionManifest item);
}

public sealed record SeasonalExceptionManifestWindowDaysRangeRule(int Min, int Max) : ISeasonalExceptionManifestRule
{
    public List<string> Check(SeasonalExceptionManifest item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionManifestRequiredDescriptionRule(string Message) : ISeasonalExceptionManifestRule
{
    public List<string> Check(SeasonalExceptionManifest item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalExceptionManifestRatingRangeRule(double Min, double Max) : ISeasonalExceptionManifestRule
{
    public List<string> Check(SeasonalExceptionManifest item)
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

/// <summary>Runs every rule of a seasonal exception manifest.</summary>
public sealed record SeasonalExceptionManifestValidator(List<ISeasonalExceptionManifestRule> Rules)
{
    public List<string> Validate(SeasonalExceptionManifest item)
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

    public void RequireValid(SeasonalExceptionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalExceptionManifest> items)
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

/// <summary>Functions of seasonal exception manifest rules.</summary>
public static class SeasonalExceptionManifestRules
{
    public static SeasonalExceptionManifestValidator DefaultSeasonalExceptionManifestValidator()
    {
        return new SeasonalExceptionManifestValidator(
            new List<ISeasonalExceptionManifestRule>
            {
                new SeasonalExceptionManifestWindowDaysRangeRule(80, 3600),
                new SeasonalExceptionManifestRequiredDescriptionRule("seasonal exception manifest needs an entry"),
                new SeasonalExceptionManifestRatingRangeRule(1.0, 2949.8),
            });
    }
}
