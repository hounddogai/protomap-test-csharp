namespace Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>A rule a regional occupancy digest must pass. Check returns its problems, or none.</summary>
public interface IRegionalOccupancyDigestRule
{
    List<string> Check(RegionalOccupancyDigest item);
}

public sealed record RegionalOccupancyDigestChannelLengthRule(int MaxLength) : IRegionalOccupancyDigestRule
{
    public List<string> Check(RegionalOccupancyDigest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalOccupancyDigestRequiredChannelRule(string Message) : IRegionalOccupancyDigestRule
{
    public List<string> Check(RegionalOccupancyDigest item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalOccupancyDigestBadgeLengthRule(int MaxLength) : IRegionalOccupancyDigestRule
{
    public List<string> Check(RegionalOccupancyDigest item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalOccupancyDigestArchivedGuardRule(double Limit) : IRegionalOccupancyDigestRule
{
    public List<string> Check(RegionalOccupancyDigest item)
    {
        if (item.Archived && item.Weight > Limit)
        {
            return new List<string> { $"weight {item.Weight} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional occupancy digest.</summary>
public sealed record RegionalOccupancyDigestValidator(List<IRegionalOccupancyDigestRule> Rules)
{
    public List<string> Validate(RegionalOccupancyDigest item)
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

    public void RequireValid(RegionalOccupancyDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalOccupancyDigest> items)
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

/// <summary>Functions of regional occupancy digest rules.</summary>
public static class RegionalOccupancyDigestRules
{
    public static RegionalOccupancyDigestValidator DefaultRegionalOccupancyDigestValidator()
    {
        return new RegionalOccupancyDigestValidator(
            new List<IRegionalOccupancyDigestRule>
            {
                new RegionalOccupancyDigestChannelLengthRule(16),
                new RegionalOccupancyDigestRequiredChannelRule("regional occupancy digest needs a value"),
                new RegionalOccupancyDigestBadgeLengthRule(255),
                new RegionalOccupancyDigestArchivedGuardRule(4.5),
            });
    }
}
