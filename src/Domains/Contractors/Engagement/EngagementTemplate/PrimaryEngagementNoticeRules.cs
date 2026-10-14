namespace Acme.Workforce.Contractors.Engagement.EngagementTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary engagement notice must pass. Check returns its problems, or none.</summary>
public interface IPrimaryEngagementNoticeRule
{
    List<string> Check(PrimaryEngagementNotice item);
}

public sealed record PrimaryEngagementNoticeLabelLengthRule(int MaxLength) : IPrimaryEngagementNoticeRule
{
    public List<string> Check(PrimaryEngagementNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementNoticeEndsAtRangeRule(int Min, int Max) : IPrimaryEngagementNoticeRule
{
    public List<string> Check(PrimaryEngagementNotice item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementNoticeAllowedSourceRule(List<string> Allowed) : IPrimaryEngagementNoticeRule
{
    public List<string> Check(PrimaryEngagementNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementNoticePinnedGuardRule(int Limit) : IPrimaryEngagementNoticeRule
{
    public List<string> Check(PrimaryEngagementNotice item)
    {
        if (item.Pinned && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementNoticeRequiredNameRule(string Message) : IPrimaryEngagementNoticeRule
{
    public List<string> Check(PrimaryEngagementNotice item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary engagement notice.</summary>
public sealed record PrimaryEngagementNoticeValidator(List<IPrimaryEngagementNoticeRule> Rules)
{
    public List<string> Validate(PrimaryEngagementNotice item)
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

    public void RequireValid(PrimaryEngagementNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryEngagementNotice> items)
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

/// <summary>Functions of primary engagement notice rules.</summary>
public static class PrimaryEngagementNoticeRules
{
    public static PrimaryEngagementNoticeValidator DefaultPrimaryEngagementNoticeValidator()
    {
        return new PrimaryEngagementNoticeValidator(
            new List<IPrimaryEngagementNoticeRule>
            {
                new PrimaryEngagementNoticeLabelLengthRule(80),
                new PrimaryEngagementNoticeEndsAtRangeRule(32, 64),
                new PrimaryEngagementNoticeAllowedSourceRule(new List<string> { "global", "scheduled", "primary" }),
                new PrimaryEngagementNoticePinnedGuardRule(16),
                new PrimaryEngagementNoticeRequiredNameRule("primary engagement notice needs a value"),
            });
    }
}
