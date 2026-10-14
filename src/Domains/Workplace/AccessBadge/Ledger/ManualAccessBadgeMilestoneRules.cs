namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual access badge milestone must pass. Check returns its problems, or none.</summary>
public interface IManualAccessBadgeMilestoneRule
{
    List<string> Check(ManualAccessBadgeMilestone item);
}

public sealed record ManualAccessBadgeMilestoneLeaveDaysRangeRule(int Min, int Max) : IManualAccessBadgeMilestoneRule
{
    public List<string> Check(ManualAccessBadgeMilestone item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAccessBadgeMilestoneRequiredTitleRule(string Message) : IManualAccessBadgeMilestoneRule
{
    public List<string> Check(ManualAccessBadgeMilestone item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeMilestoneMarginRangeRule(double Min, double Max) : IManualAccessBadgeMilestoneRule
{
    public List<string> Check(ManualAccessBadgeMilestone item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeMilestoneAllowedTitleRule(List<string> Allowed) : IManualAccessBadgeMilestoneRule
{
    public List<string> Check(ManualAccessBadgeMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeMilestoneCodePrefixRule(string Prefix) : IManualAccessBadgeMilestoneRule
{
    public List<string> Check(ManualAccessBadgeMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeMilestoneTitleLengthRule(int MaxLength) : IManualAccessBadgeMilestoneRule
{
    public List<string> Check(ManualAccessBadgeMilestone item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual access badge milestone.</summary>
public sealed record ManualAccessBadgeMilestoneValidator(List<IManualAccessBadgeMilestoneRule> Rules)
{
    public List<string> Validate(ManualAccessBadgeMilestone item)
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

    public void RequireValid(ManualAccessBadgeMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccessBadgeMilestone> items)
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

/// <summary>Functions of manual access badge milestone rules.</summary>
public static class ManualAccessBadgeMilestoneRules
{
    public static ManualAccessBadgeMilestoneValidator DefaultManualAccessBadgeMilestoneValidator()
    {
        return new ManualAccessBadgeMilestoneValidator(
            new List<IManualAccessBadgeMilestoneRule>
            {
                new ManualAccessBadgeMilestoneLeaveDaysRangeRule(16, 255),
                new ManualAccessBadgeMilestoneRequiredTitleRule("manual access badge milestone needs an entry"),
                new ManualAccessBadgeMilestoneMarginRangeRule(1.3, 2509.0),
                new ManualAccessBadgeMilestoneAllowedTitleRule(new List<string> { "fallback", "strict", "strict" }),
                new ManualAccessBadgeMilestoneCodePrefixRule("ACM"),
                new ManualAccessBadgeMilestoneTitleLengthRule(16),
            });
    }
}
