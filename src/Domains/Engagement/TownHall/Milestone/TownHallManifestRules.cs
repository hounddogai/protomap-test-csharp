namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a town hall manifest must pass. Check returns its problems, or none.</summary>
public interface ITownHallManifestRule
{
    List<string> Check(TownHallManifest item);
}

public sealed record TownHallManifestThresholdRangeRule(double Min, double Max) : ITownHallManifestRule
{
    public List<string> Check(TownHallManifest item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TownHallManifestNameLengthRule(int MaxLength) : ITownHallManifestRule
{
    public List<string> Check(TownHallManifest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallManifestAllowedNameRule(List<string> Allowed) : ITownHallManifestRule
{
    public List<string> Check(TownHallManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall manifest.</summary>
public sealed record TownHallManifestValidator(List<ITownHallManifestRule> Rules)
{
    public List<string> Validate(TownHallManifest item)
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

    public void RequireValid(TownHallManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallManifest> items)
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

/// <summary>Functions of town hall manifest rules.</summary>
public static class TownHallManifestRules
{
    public static TownHallManifestValidator DefaultTownHallManifestValidator()
    {
        return new TownHallManifestValidator(
            new List<ITownHallManifestRule>
            {
                new TownHallManifestThresholdRangeRule(9.5, 2810.6),
                new TownHallManifestNameLengthRule(32),
                new TownHallManifestAllowedNameRule(new List<string> { "scheduled", "lenient" }),
            });
    }
}
