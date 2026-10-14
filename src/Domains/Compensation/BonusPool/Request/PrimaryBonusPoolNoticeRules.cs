namespace Acme.Workforce.Compensation.BonusPool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary bonus pool notice must pass. Check returns its problems, or none.</summary>
public interface IPrimaryBonusPoolNoticeRule
{
    List<string> Check(PrimaryBonusPoolNotice item);
}

public sealed record PrimaryBonusPoolNoticeAllowedLabelRule(List<string> Allowed) : IPrimaryBonusPoolNoticeRule
{
    public List<string> Check(PrimaryBonusPoolNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolNoticeCodePrefixRule(string Prefix) : IPrimaryBonusPoolNoticeRule
{
    public List<string> Check(PrimaryBonusPoolNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolNoticeLabelLengthRule(int MaxLength) : IPrimaryBonusPoolNoticeRule
{
    public List<string> Check(PrimaryBonusPoolNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolNoticeRequiredCodeRule(string Message) : IPrimaryBonusPoolNoticeRule
{
    public List<string> Check(PrimaryBonusPoolNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolNoticeTitleLengthRule(int MaxLength) : IPrimaryBonusPoolNoticeRule
{
    public List<string> Check(PrimaryBonusPoolNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryBonusPoolNoticeActiveGuardRule(int Limit) : IPrimaryBonusPoolNoticeRule
{
    public List<string> Check(PrimaryBonusPoolNotice item)
    {
        if (item.Active && item.Capacity > Limit)
        {
            return new List<string> { $"capacity {item.Capacity} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary bonus pool notice.</summary>
public sealed record PrimaryBonusPoolNoticeValidator(List<IPrimaryBonusPoolNoticeRule> Rules)
{
    public List<string> Validate(PrimaryBonusPoolNotice item)
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

    public void RequireValid(PrimaryBonusPoolNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryBonusPoolNotice> items)
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

/// <summary>Functions of primary bonus pool notice rules.</summary>
public static class PrimaryBonusPoolNoticeRules
{
    public static PrimaryBonusPoolNoticeValidator DefaultPrimaryBonusPoolNoticeValidator()
    {
        return new PrimaryBonusPoolNoticeValidator(
            new List<IPrimaryBonusPoolNoticeRule>
            {
                new PrimaryBonusPoolNoticeAllowedLabelRule(new List<string>()),
                new PrimaryBonusPoolNoticeCodePrefixRule("OPS"),
                new PrimaryBonusPoolNoticeLabelLengthRule(32),
                new PrimaryBonusPoolNoticeRequiredCodeRule("primary bonus pool notice needs a value"),
                new PrimaryBonusPoolNoticeTitleLengthRule(64),
                new PrimaryBonusPoolNoticeActiveGuardRule(255),
            });
    }
}
