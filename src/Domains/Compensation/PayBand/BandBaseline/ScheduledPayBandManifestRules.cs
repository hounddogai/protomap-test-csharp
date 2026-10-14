namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pay band manifest must pass. Check returns its problems, or none.</summary>
public interface IScheduledPayBandManifestRule
{
    List<string> Check(ScheduledPayBandManifest item);
}

public sealed record ScheduledPayBandManifestAllowedChecksumRule(List<string> Allowed) : IScheduledPayBandManifestRule
{
    public List<string> Check(ScheduledPayBandManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandManifestExpiresAtRangeRule(int Min, int Max) : IScheduledPayBandManifestRule
{
    public List<string> Check(ScheduledPayBandManifest item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandManifestTitleLengthRule(int MaxLength) : IScheduledPayBandManifestRule
{
    public List<string> Check(ScheduledPayBandManifest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandManifestRequiredChecksumRule(string Message) : IScheduledPayBandManifestRule
{
    public List<string> Check(ScheduledPayBandManifest item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPayBandManifestWindowDaysRangeRule(int Min, int Max) : IScheduledPayBandManifestRule
{
    public List<string> Check(ScheduledPayBandManifest item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledPayBandManifestStartsAtRangeRule(int Min, int Max) : IScheduledPayBandManifestRule
{
    public List<string> Check(ScheduledPayBandManifest item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a scheduled pay band manifest.</summary>
public sealed record ScheduledPayBandManifestValidator(List<IScheduledPayBandManifestRule> Rules)
{
    public List<string> Validate(ScheduledPayBandManifest item)
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

    public void RequireValid(ScheduledPayBandManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPayBandManifest> items)
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

/// <summary>Functions of scheduled pay band manifest rules.</summary>
public static class ScheduledPayBandManifestRules
{
    public static ScheduledPayBandManifestValidator DefaultScheduledPayBandManifestValidator()
    {
        return new ScheduledPayBandManifestValidator(
            new List<IScheduledPayBandManifestRule>
            {
                new ScheduledPayBandManifestAllowedChecksumRule(new List<string>()),
                new ScheduledPayBandManifestExpiresAtRangeRule(255, 3600),
                new ScheduledPayBandManifestTitleLengthRule(1000),
                new ScheduledPayBandManifestRequiredChecksumRule("scheduled pay band manifest needs a value"),
                new ScheduledPayBandManifestWindowDaysRangeRule(80, 1000),
                new ScheduledPayBandManifestStartsAtRangeRule(64, 86400),
            });
    }
}
