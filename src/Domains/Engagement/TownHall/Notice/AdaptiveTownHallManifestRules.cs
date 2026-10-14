namespace Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>A rule a adaptive town hall manifest must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTownHallManifestRule
{
    List<string> Check(AdaptiveTownHallManifest item);
}

public sealed record AdaptiveTownHallManifestRequiredTeamRule(string Message) : IAdaptiveTownHallManifestRule
{
    public List<string> Check(AdaptiveTownHallManifest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallManifestLocationLengthRule(int MaxLength) : IAdaptiveTownHallManifestRule
{
    public List<string> Check(AdaptiveTownHallManifest item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallManifestMarginRangeRule(double Min, double Max) : IAdaptiveTownHallManifestRule
{
    public List<string> Check(AdaptiveTownHallManifest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a adaptive town hall manifest.</summary>
public sealed record AdaptiveTownHallManifestValidator(List<IAdaptiveTownHallManifestRule> Rules)
{
    public List<string> Validate(AdaptiveTownHallManifest item)
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

    public void RequireValid(AdaptiveTownHallManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTownHallManifest> items)
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

/// <summary>Functions of adaptive town hall manifest rules.</summary>
public static class AdaptiveTownHallManifestRules
{
    public static AdaptiveTownHallManifestValidator DefaultAdaptiveTownHallManifestValidator()
    {
        return new AdaptiveTownHallManifestValidator(
            new List<IAdaptiveTownHallManifestRule>
            {
                new AdaptiveTownHallManifestRequiredTeamRule("adaptive town hall manifest needs this field"),
                new AdaptiveTownHallManifestLocationLengthRule(1000),
                new AdaptiveTownHallManifestMarginRangeRule(3.6, 3216.0),
            });
    }
}
