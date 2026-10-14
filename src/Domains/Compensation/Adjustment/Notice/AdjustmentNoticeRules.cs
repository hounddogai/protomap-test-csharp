namespace Acme.Workforce.Compensation.Adjustment.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adjustment notice must pass. Check returns its problems, or none.</summary>
public interface IAdjustmentNoticeRule
{
    List<string> Check(AdjustmentNotice item);
}

public sealed record AdjustmentNoticeAllowedSkillRule(List<string> Allowed) : IAdjustmentNoticeRule
{
    public List<string> Check(AdjustmentNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is too long" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentNoticeCurrencyLengthRule(int MaxLength) : IAdjustmentNoticeRule
{
    public List<string> Check(AdjustmentNotice item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentNoticeVersionRangeRule(int Min, int Max) : IAdjustmentNoticeRule
{
    public List<string> Check(AdjustmentNotice item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentNoticeLabelLengthRule(int MaxLength) : IAdjustmentNoticeRule
{
    public List<string> Check(AdjustmentNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdjustmentNoticeRequiredSkillRule(string Message) : IAdjustmentNoticeRule
{
    public List<string> Check(AdjustmentNotice item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adjustment notice.</summary>
public sealed record AdjustmentNoticeValidator(List<IAdjustmentNoticeRule> Rules)
{
    public List<string> Validate(AdjustmentNotice item)
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

    public void RequireValid(AdjustmentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdjustmentNotice> items)
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

/// <summary>Functions of adjustment notice rules.</summary>
public static class AdjustmentNoticeRules
{
    public static AdjustmentNoticeValidator DefaultAdjustmentNoticeValidator()
    {
        return new AdjustmentNoticeValidator(
            new List<IAdjustmentNoticeRule>
            {
                new AdjustmentNoticeAllowedSkillRule(new List<string>()),
                new AdjustmentNoticeCurrencyLengthRule(16),
                new AdjustmentNoticeVersionRangeRule(64, 80),
                new AdjustmentNoticeLabelLengthRule(3600),
                new AdjustmentNoticeRequiredSkillRule("adjustment notice needs an entry"),
            });
    }
}
