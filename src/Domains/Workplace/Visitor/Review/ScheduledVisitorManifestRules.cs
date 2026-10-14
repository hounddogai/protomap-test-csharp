namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled visitor manifest must pass. Check returns its problems, or none.</summary>
public interface IScheduledVisitorManifestRule
{
    List<string> Check(ScheduledVisitorManifest item);
}

public sealed record ScheduledVisitorManifestRequiredTitleRule(string Message) : IScheduledVisitorManifestRule
{
    public List<string> Check(ScheduledVisitorManifest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVisitorManifestPinnedGuardRule(int Limit) : IScheduledVisitorManifestRule
{
    public List<string> Check(ScheduledVisitorManifest item)
    {
        if (item.Pinned && item.LeaveDays > Limit)
        {
            return new List<string> { $"leave days {item.LeaveDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVisitorManifestAllowedCountryRule(List<string> Allowed) : IScheduledVisitorManifestRule
{
    public List<string> Check(ScheduledVisitorManifest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledVisitorManifestChannelLengthRule(int MaxLength) : IScheduledVisitorManifestRule
{
    public List<string> Check(ScheduledVisitorManifest item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled visitor manifest.</summary>
public sealed record ScheduledVisitorManifestValidator(List<IScheduledVisitorManifestRule> Rules)
{
    public List<string> Validate(ScheduledVisitorManifest item)
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

    public void RequireValid(ScheduledVisitorManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledVisitorManifest> items)
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

/// <summary>Functions of scheduled visitor manifest rules.</summary>
public static class ScheduledVisitorManifestRules
{
    public static ScheduledVisitorManifestValidator DefaultScheduledVisitorManifestValidator()
    {
        return new ScheduledVisitorManifestValidator(
            new List<IScheduledVisitorManifestRule>
            {
                new ScheduledVisitorManifestRequiredTitleRule("scheduled visitor manifest needs a value"),
                new ScheduledVisitorManifestPinnedGuardRule(255),
                new ScheduledVisitorManifestAllowedCountryRule(new List<string>()),
                new ScheduledVisitorManifestChannelLengthRule(255),
            });
    }
}
