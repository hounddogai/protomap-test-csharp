namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling adjustment notice must pass. Check returns its problems, or none.</summary>
public interface IRollingAdjustmentNoticeRule
{
    List<string> Check(RollingAdjustmentNotice item);
}

public sealed record RollingAdjustmentNoticeRequiredManagerIdRule(string Message) : IRollingAdjustmentNoticeRule
{
    public List<string> Check(RollingAdjustmentNotice item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentNoticeTitleLengthRule(int MaxLength) : IRollingAdjustmentNoticeRule
{
    public List<string> Check(RollingAdjustmentNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentNoticeAllowedTitleRule(List<string> Allowed) : IRollingAdjustmentNoticeRule
{
    public List<string> Check(RollingAdjustmentNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentNoticeRequiredCodeRule(string Message) : IRollingAdjustmentNoticeRule
{
    public List<string> Check(RollingAdjustmentNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingAdjustmentNoticeManagerIdLengthRule(int MaxLength) : IRollingAdjustmentNoticeRule
{
    public List<string> Check(RollingAdjustmentNotice item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling adjustment notice.</summary>
public sealed record RollingAdjustmentNoticeValidator(List<IRollingAdjustmentNoticeRule> Rules)
{
    public List<string> Validate(RollingAdjustmentNotice item)
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

    public void RequireValid(RollingAdjustmentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingAdjustmentNotice> items)
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

/// <summary>Functions of rolling adjustment notice rules.</summary>
public static class RollingAdjustmentNoticeRules
{
    public static RollingAdjustmentNoticeValidator DefaultRollingAdjustmentNoticeValidator()
    {
        return new RollingAdjustmentNoticeValidator(
            new List<IRollingAdjustmentNoticeRule>
            {
                new RollingAdjustmentNoticeRequiredManagerIdRule("rolling adjustment notice needs this field"),
                new RollingAdjustmentNoticeTitleLengthRule(16),
                new RollingAdjustmentNoticeAllowedTitleRule(new List<string> { "capped", "batched", "legacy" }),
                new RollingAdjustmentNoticeRequiredCodeRule("rolling adjustment notice needs a value"),
                new RollingAdjustmentNoticeManagerIdLengthRule(16),
            });
    }
}
