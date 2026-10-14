namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled reorg exception must pass. Check returns its problems, or none.</summary>
public interface IScheduledReorgExceptionRule
{
    List<string> Check(ScheduledReorgException item);
}

public sealed record ScheduledReorgExceptionLimitRangeRule(int Min, int Max) : IScheduledReorgExceptionRule
{
    public List<string> Check(ScheduledReorgException item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScheduledReorgExceptionAllowedCodeRule(List<string> Allowed) : IScheduledReorgExceptionRule
{
    public List<string> Check(ScheduledReorgException item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReorgExceptionWindowDaysRangeRule(int Min, int Max) : IScheduledReorgExceptionRule
{
    public List<string> Check(ScheduledReorgException item)
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

public sealed record ScheduledReorgExceptionRequiredRegionRule(string Message) : IScheduledReorgExceptionRule
{
    public List<string> Check(ScheduledReorgException item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReorgExceptionRegionLengthRule(int MaxLength) : IScheduledReorgExceptionRule
{
    public List<string> Check(ScheduledReorgException item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledReorgExceptionCodePrefixRule(string Prefix) : IScheduledReorgExceptionRule
{
    public List<string> Check(ScheduledReorgException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled reorg exception.</summary>
public sealed record ScheduledReorgExceptionValidator(List<IScheduledReorgExceptionRule> Rules)
{
    public List<string> Validate(ScheduledReorgException item)
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

    public void RequireValid(ScheduledReorgException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledReorgException> items)
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

/// <summary>Functions of scheduled reorg exception rules.</summary>
public static class ScheduledReorgExceptionRules
{
    public static ScheduledReorgExceptionValidator DefaultScheduledReorgExceptionValidator()
    {
        return new ScheduledReorgExceptionValidator(
            new List<IScheduledReorgExceptionRule>
            {
                new ScheduledReorgExceptionLimitRangeRule(3600, 86400),
                new ScheduledReorgExceptionAllowedCodeRule(new List<string> { "seasonal", "batched", "fallback" }),
                new ScheduledReorgExceptionWindowDaysRangeRule(32, 255),
                new ScheduledReorgExceptionRequiredRegionRule("scheduled reorg exception needs a value"),
                new ScheduledReorgExceptionRegionLengthRule(3600),
                new ScheduledReorgExceptionCodePrefixRule("X"),
            });
    }
}
