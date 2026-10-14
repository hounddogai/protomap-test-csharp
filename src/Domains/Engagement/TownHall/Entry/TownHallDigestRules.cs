namespace Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>A rule a town hall digest must pass. Check returns its problems, or none.</summary>
public interface ITownHallDigestRule
{
    List<string> Check(TownHallDigest item);
}

public sealed record TownHallDigestVersionRangeRule(int Min, int Max) : ITownHallDigestRule
{
    public List<string> Check(TownHallDigest item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallDigestMarginRangeRule(double Min, double Max) : ITownHallDigestRule
{
    public List<string> Check(TownHallDigest item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallDigestManagerIdLengthRule(int MaxLength) : ITownHallDigestRule
{
    public List<string> Check(TownHallDigest item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallDigestGradeLengthRule(int MaxLength) : ITownHallDigestRule
{
    public List<string> Check(TownHallDigest item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallDigestTagsLimitRule(int MaxCount, List<string> Banned) : ITownHallDigestRule
{
    public List<string> Check(TownHallDigest item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

/// <summary>Runs every rule of a town hall digest.</summary>
public sealed record TownHallDigestValidator(List<ITownHallDigestRule> Rules)
{
    public List<string> Validate(TownHallDigest item)
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

    public void RequireValid(TownHallDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallDigest> items)
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

/// <summary>Functions of town hall digest rules.</summary>
public static class TownHallDigestRules
{
    public static TownHallDigestValidator DefaultTownHallDigestValidator()
    {
        return new TownHallDigestValidator(
            new List<ITownHallDigestRule>
            {
                new TownHallDigestVersionRangeRule(64, 80),
                new TownHallDigestMarginRangeRule(4.0, 2943.5),
                new TownHallDigestManagerIdLengthRule(255),
                new TownHallDigestGradeLengthRule(80),
                new TownHallDigestTagsLimitRule(86400, new List<string>()),
            });
    }
}
