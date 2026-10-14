namespace Acme.Workforce.Talent.Mobility.Preference;

/// <summary>A rule a seasonal mobility manifest must pass. Check returns its problems, or none.</summary>
public interface ISeasonalMobilityManifestRule
{
    List<string> Check(SeasonalMobilityManifest item);
}

public sealed record SeasonalMobilityManifestRequiredNameRule(string Message) : ISeasonalMobilityManifestRule
{
    public List<string> Check(SeasonalMobilityManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityManifestCeilingRangeRule(double Min, double Max) : ISeasonalMobilityManifestRule
{
    public List<string> Check(SeasonalMobilityManifest item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalMobilityManifestRequiredCodeRule(string Message) : ISeasonalMobilityManifestRule
{
    public List<string> Check(SeasonalMobilityManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal mobility manifest.</summary>
public sealed record SeasonalMobilityManifestValidator(List<ISeasonalMobilityManifestRule> Rules)
{
    public List<string> Validate(SeasonalMobilityManifest item)
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

    public void RequireValid(SeasonalMobilityManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalMobilityManifest> items)
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

/// <summary>Functions of seasonal mobility manifest rules.</summary>
public static class SeasonalMobilityManifestRules
{
    public static SeasonalMobilityManifestValidator DefaultSeasonalMobilityManifestValidator()
    {
        return new SeasonalMobilityManifestValidator(
            new List<ISeasonalMobilityManifestRule>
            {
                new SeasonalMobilityManifestRequiredNameRule("seasonal mobility manifest needs an entry"),
                new SeasonalMobilityManifestCeilingRangeRule(9.6, 805.4),
                new SeasonalMobilityManifestRequiredCodeRule("seasonal mobility manifest needs this field"),
            });
    }
}
