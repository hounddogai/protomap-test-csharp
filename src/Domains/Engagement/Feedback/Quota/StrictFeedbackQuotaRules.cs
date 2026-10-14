namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict feedback quota must pass. Check returns its problems, or none.</summary>
public interface IStrictFeedbackQuotaRule
{
    List<string> Check(StrictFeedbackQuota item);
}

public sealed record StrictFeedbackQuotaRequiredUnitRule(string Message) : IStrictFeedbackQuotaRule
{
    public List<string> Check(StrictFeedbackQuota item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictFeedbackQuotaCodeLengthRule(int MaxLength) : IStrictFeedbackQuotaRule
{
    public List<string> Check(StrictFeedbackQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictFeedbackQuotaUnitLengthRule(int MaxLength) : IStrictFeedbackQuotaRule
{
    public List<string> Check(StrictFeedbackQuota item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictFeedbackQuotaChecksumLengthRule(int MaxLength) : IStrictFeedbackQuotaRule
{
    public List<string> Check(StrictFeedbackQuota item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictFeedbackQuotaAllowedCodeRule(List<string> Allowed) : IStrictFeedbackQuotaRule
{
    public List<string> Check(StrictFeedbackQuota item)
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

public sealed record StrictFeedbackQuotaCapacityRangeRule(int Min, int Max) : IStrictFeedbackQuotaRule
{
    public List<string> Check(StrictFeedbackQuota item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict feedback quota.</summary>
public sealed record StrictFeedbackQuotaValidator(List<IStrictFeedbackQuotaRule> Rules)
{
    public List<string> Validate(StrictFeedbackQuota item)
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

    public void RequireValid(StrictFeedbackQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictFeedbackQuota> items)
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

/// <summary>Functions of strict feedback quota rules.</summary>
public static class StrictFeedbackQuotaRules
{
    public static StrictFeedbackQuotaValidator DefaultStrictFeedbackQuotaValidator()
    {
        return new StrictFeedbackQuotaValidator(
            new List<IStrictFeedbackQuotaRule>
            {
                new StrictFeedbackQuotaRequiredUnitRule("strict feedback quota needs this field"),
                new StrictFeedbackQuotaCodeLengthRule(80),
                new StrictFeedbackQuotaUnitLengthRule(80),
                new StrictFeedbackQuotaChecksumLengthRule(86400),
                new StrictFeedbackQuotaAllowedCodeRule(new List<string> { "batched" }),
                new StrictFeedbackQuotaCapacityRangeRule(64, 86400),
            });
    }
}
