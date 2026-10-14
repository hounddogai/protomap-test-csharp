namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled mobility entry must pass. Check returns its problems, or none.</summary>
public interface IScheduledMobilityEntryRule
{
    List<string> Check(ScheduledMobilityEntry item);
}

public sealed record ScheduledMobilityEntryTitleLengthRule(int MaxLength) : IScheduledMobilityEntryRule
{
    public List<string> Check(ScheduledMobilityEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityEntryMaximumRangeRule(double Min, double Max) : IScheduledMobilityEntryRule
{
    public List<string> Check(ScheduledMobilityEntry item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityEntryCodePrefixRule(string Prefix) : IScheduledMobilityEntryRule
{
    public List<string> Check(ScheduledMobilityEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledMobilityEntryAllowedChannelRule(List<string> Allowed) : IScheduledMobilityEntryRule
{
    public List<string> Check(ScheduledMobilityEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled mobility entry.</summary>
public sealed record ScheduledMobilityEntryValidator(List<IScheduledMobilityEntryRule> Rules)
{
    public List<string> Validate(ScheduledMobilityEntry item)
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

    public void RequireValid(ScheduledMobilityEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledMobilityEntry> items)
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

/// <summary>Functions of scheduled mobility entry rules.</summary>
public static class ScheduledMobilityEntryRules
{
    public static ScheduledMobilityEntryValidator DefaultScheduledMobilityEntryValidator()
    {
        return new ScheduledMobilityEntryValidator(
            new List<IScheduledMobilityEntryRule>
            {
                new ScheduledMobilityEntryTitleLengthRule(3600),
                new ScheduledMobilityEntryMaximumRangeRule(5.5, 4524.6),
                new ScheduledMobilityEntryCodePrefixRule("X"),
                new ScheduledMobilityEntryAllowedChannelRule(new List<string>()),
            });
    }
}
