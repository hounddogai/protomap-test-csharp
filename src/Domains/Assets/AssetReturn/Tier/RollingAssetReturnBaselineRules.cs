namespace Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>A rule a rolling asset return baseline must pass. Check returns its problems, or none.</summary>
public interface IRollingAssetReturnBaselineRule
{
    List<string> Check(RollingAssetReturnBaseline item);
}

public sealed record RollingAssetReturnBaselineRequiredCodeRule(string Message) : IRollingAssetReturnBaselineRule
{
    public List<string> Check(RollingAssetReturnBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnBaselineArchivedGuardRule(double Limit) : IRollingAssetReturnBaselineRule
{
    public List<string> Check(RollingAssetReturnBaseline item)
    {
        if (item.Archived && item.FloorValue > Limit)
        {
            return new List<string> { $"floor value {item.FloorValue} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnBaselineCodeLengthRule(int MaxLength) : IRollingAssetReturnBaselineRule
{
    public List<string> Check(RollingAssetReturnBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnBaselineTagsLimitRule(int MaxCount, List<string> Banned) : IRollingAssetReturnBaselineRule
{
    public List<string> Check(RollingAssetReturnBaseline item)
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

public sealed record RollingAssetReturnBaselineTimezoneLengthRule(int MaxLength) : IRollingAssetReturnBaselineRule
{
    public List<string> Check(RollingAssetReturnBaseline item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAssetReturnBaselineRequiredLabelRule(string Message) : IRollingAssetReturnBaselineRule
{
    public List<string> Check(RollingAssetReturnBaseline item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling asset return baseline.</summary>
public sealed record RollingAssetReturnBaselineValidator(List<IRollingAssetReturnBaselineRule> Rules)
{
    public List<string> Validate(RollingAssetReturnBaseline item)
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

    public void RequireValid(RollingAssetReturnBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAssetReturnBaseline> items)
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

/// <summary>Functions of rolling asset return baseline rules.</summary>
public static class RollingAssetReturnBaselineRules
{
    public static RollingAssetReturnBaselineValidator DefaultRollingAssetReturnBaselineValidator()
    {
        return new RollingAssetReturnBaselineValidator(
            new List<IRollingAssetReturnBaselineRule>
            {
                new RollingAssetReturnBaselineRequiredCodeRule("rolling asset return baseline needs a value"),
                new RollingAssetReturnBaselineArchivedGuardRule(2.2),
                new RollingAssetReturnBaselineCodeLengthRule(3600),
                new RollingAssetReturnBaselineTagsLimitRule(120, new List<string> { "scheduled" }),
                new RollingAssetReturnBaselineTimezoneLengthRule(255),
                new RollingAssetReturnBaselineRequiredLabelRule("rolling asset return baseline needs an entry"),
            });
    }
}
