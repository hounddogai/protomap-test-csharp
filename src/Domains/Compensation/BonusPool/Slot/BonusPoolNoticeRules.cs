namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a bonus pool notice must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolNoticeRule
{
    List<string> Check(BonusPoolNotice item);
}

public sealed record BonusPoolNoticeTitleLengthRule(int MaxLength) : IBonusPoolNoticeRule
{
    public List<string> Check(BonusPoolNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolNoticeCodeLengthRule(int MaxLength) : IBonusPoolNoticeRule
{
    public List<string> Check(BonusPoolNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolNoticeAllowedOwnerIdRule(List<string> Allowed) : IBonusPoolNoticeRule
{
    public List<string> Check(BonusPoolNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool notice.</summary>
public sealed record BonusPoolNoticeValidator(List<IBonusPoolNoticeRule> Rules)
{
    public List<string> Validate(BonusPoolNotice item)
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

    public void RequireValid(BonusPoolNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolNotice> items)
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

/// <summary>Functions of bonus pool notice rules.</summary>
public static class BonusPoolNoticeRules
{
    public static BonusPoolNoticeValidator DefaultBonusPoolNoticeValidator()
    {
        return new BonusPoolNoticeValidator(
            new List<IBonusPoolNoticeRule>
            {
                new BonusPoolNoticeTitleLengthRule(86400),
                new BonusPoolNoticeCodeLengthRule(80),
                new BonusPoolNoticeAllowedOwnerIdRule(new List<string> { "strict", "lenient" }),
            });
    }
}
