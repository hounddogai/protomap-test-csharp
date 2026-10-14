namespace Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>A rule a rolling position manifest must pass. Check returns its problems, or none.</summary>
public interface IRollingPositionManifestRule
{
    List<string> Check(RollingPositionManifest item);
}

public sealed record RollingPositionManifestRequiredSegmentRule(string Message) : IRollingPositionManifestRule
{
    public List<string> Check(RollingPositionManifest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionManifestRequiredReviewerRule(string Message) : IRollingPositionManifestRule
{
    public List<string> Check(RollingPositionManifest item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionManifestReviewerLengthRule(int MaxLength) : IRollingPositionManifestRule
{
    public List<string> Check(RollingPositionManifest item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling position manifest.</summary>
public sealed record RollingPositionManifestValidator(List<IRollingPositionManifestRule> Rules)
{
    public List<string> Validate(RollingPositionManifest item)
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

    public void RequireValid(RollingPositionManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPositionManifest> items)
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

/// <summary>Functions of rolling position manifest rules.</summary>
public static class RollingPositionManifestRules
{
    public static RollingPositionManifestValidator DefaultRollingPositionManifestValidator()
    {
        return new RollingPositionManifestValidator(
            new List<IRollingPositionManifestRule>
            {
                new RollingPositionManifestRequiredSegmentRule("rolling position manifest needs a value"),
                new RollingPositionManifestRequiredReviewerRule("rolling position manifest needs a value"),
                new RollingPositionManifestReviewerLengthRule(32),
            });
    }
}
