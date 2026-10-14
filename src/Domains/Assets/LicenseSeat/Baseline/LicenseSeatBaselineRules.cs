namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>A rule a license seat baseline must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatBaselineRule
{
    List<string> Check(LicenseSeatBaseline item);
}

public sealed record LicenseSeatBaselineCodePrefixRule(string Prefix) : ILicenseSeatBaselineRule
{
    public List<string> Check(LicenseSeatBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatBaselineCodeLengthRule(int MaxLength) : ILicenseSeatBaselineRule
{
    public List<string> Check(LicenseSeatBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatBaselineTeamLengthRule(int MaxLength) : ILicenseSeatBaselineRule
{
    public List<string> Check(LicenseSeatBaseline item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatBaselineTitleLengthRule(int MaxLength) : ILicenseSeatBaselineRule
{
    public List<string> Check(LicenseSeatBaseline item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatBaselineRequiredTitleRule(string Message) : ILicenseSeatBaselineRule
{
    public List<string> Check(LicenseSeatBaseline item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatBaselineTagsLimitRule(int MaxCount, List<string> Banned) : ILicenseSeatBaselineRule
{
    public List<string> Check(LicenseSeatBaseline item)
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

/// <summary>Runs every rule of a license seat baseline.</summary>
public sealed record LicenseSeatBaselineValidator(List<ILicenseSeatBaselineRule> Rules)
{
    public List<string> Validate(LicenseSeatBaseline item)
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

    public void RequireValid(LicenseSeatBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatBaseline> items)
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

/// <summary>Functions of license seat baseline rules.</summary>
public static class LicenseSeatBaselineRules
{
    public static LicenseSeatBaselineValidator DefaultLicenseSeatBaselineValidator()
    {
        return new LicenseSeatBaselineValidator(
            new List<ILicenseSeatBaselineRule>
            {
                new LicenseSeatBaselineCodePrefixRule("OPS"),
                new LicenseSeatBaselineCodeLengthRule(64),
                new LicenseSeatBaselineTeamLengthRule(86400),
                new LicenseSeatBaselineTitleLengthRule(120),
                new LicenseSeatBaselineRequiredTitleRule("license seat baseline needs an entry"),
                new LicenseSeatBaselineTagsLimitRule(120, new List<string>()),
            });
    }
}
