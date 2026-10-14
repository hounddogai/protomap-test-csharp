namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive adjustment notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveAdjustmentNoticeRule
{
    List<string> Check(AdaptiveAdjustmentNotice item);
}

public sealed record AdaptiveAdjustmentNoticeAllowedChannelRule(List<string> Allowed) : IAdaptiveAdjustmentNoticeRule
{
    public List<string> Check(AdaptiveAdjustmentNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Channel)))
        {
            return new List<string> { $"channel {item.Channel} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAdjustmentNoticeRequiredNameRule(string Message) : IAdaptiveAdjustmentNoticeRule
{
    public List<string> Check(AdaptiveAdjustmentNotice item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAdjustmentNoticeVisibleGuardRule(int Limit) : IAdaptiveAdjustmentNoticeRule
{
    public List<string> Check(AdaptiveAdjustmentNotice item)
    {
        if (item.Visible && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAdjustmentNoticeManagerIdLengthRule(int MaxLength) : IAdaptiveAdjustmentNoticeRule
{
    public List<string> Check(AdaptiveAdjustmentNotice item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveAdjustmentNoticeCodePrefixRule(string Prefix) : IAdaptiveAdjustmentNoticeRule
{
    public List<string> Check(AdaptiveAdjustmentNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive adjustment notice.</summary>
public sealed record AdaptiveAdjustmentNoticeValidator(List<IAdaptiveAdjustmentNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveAdjustmentNotice item)
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

    public void RequireValid(AdaptiveAdjustmentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveAdjustmentNotice> items)
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

/// <summary>Functions of adaptive adjustment notice rules.</summary>
public static class AdaptiveAdjustmentNoticeRules
{
    public static AdaptiveAdjustmentNoticeValidator DefaultAdaptiveAdjustmentNoticeValidator()
    {
        return new AdaptiveAdjustmentNoticeValidator(
            new List<IAdaptiveAdjustmentNoticeRule>
            {
                new AdaptiveAdjustmentNoticeAllowedChannelRule(new List<string> { "legacy", "lenient", "scheduled" }),
                new AdaptiveAdjustmentNoticeRequiredNameRule("adaptive adjustment notice needs a value"),
                new AdaptiveAdjustmentNoticeVisibleGuardRule(3600),
                new AdaptiveAdjustmentNoticeManagerIdLengthRule(16),
                new AdaptiveAdjustmentNoticeCodePrefixRule("X"),
            });
    }
}
