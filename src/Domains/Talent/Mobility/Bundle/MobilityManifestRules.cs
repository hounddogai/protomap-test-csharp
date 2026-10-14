namespace Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>A rule a mobility manifest must pass. Check returns its problems, or none.</summary>
public interface IMobilityManifestRule
{
    List<string> Check(MobilityManifest item);
}

public sealed record MobilityManifestDurationMinutesRangeRule(int Min, int Max) : IMobilityManifestRule
{
    public List<string> Check(MobilityManifest item)
    {
        var valueValue = item.DurationMinutes;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"duration minutes {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"duration minutes {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityManifestRequiredNameRule(string Message) : IMobilityManifestRule
{
    public List<string> Check(MobilityManifest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityManifestRequiredManagerIdRule(string Message) : IMobilityManifestRule
{
    public List<string> Check(MobilityManifest item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityManifestBadgeLengthRule(int MaxLength) : IMobilityManifestRule
{
    public List<string> Check(MobilityManifest item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility manifest.</summary>
public sealed record MobilityManifestValidator(List<IMobilityManifestRule> Rules)
{
    public List<string> Validate(MobilityManifest item)
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

    public void RequireValid(MobilityManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityManifest> items)
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

/// <summary>Functions of mobility manifest rules.</summary>
public static class MobilityManifestRules
{
    public static MobilityManifestValidator DefaultMobilityManifestValidator()
    {
        return new MobilityManifestValidator(
            new List<IMobilityManifestRule>
            {
                new MobilityManifestDurationMinutesRangeRule(32, 80),
                new MobilityManifestRequiredNameRule("mobility manifest needs an entry"),
                new MobilityManifestRequiredManagerIdRule("mobility manifest needs a value"),
                new MobilityManifestBadgeLengthRule(80),
            });
    }
}
