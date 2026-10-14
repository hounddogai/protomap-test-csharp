namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled adjustment summary must pass. Check returns its problems, or none.</summary>
public interface IScheduledAdjustmentSummaryRule
{
    List<string> Check(ScheduledAdjustmentSummary item);
}

public sealed record ScheduledAdjustmentSummaryRequiredCodeRule(string Message) : IScheduledAdjustmentSummaryRule
{
    public List<string> Check(ScheduledAdjustmentSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentSummaryChecksumLengthRule(int MaxLength) : IScheduledAdjustmentSummaryRule
{
    public List<string> Check(ScheduledAdjustmentSummary item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentSummaryCodeLengthRule(int MaxLength) : IScheduledAdjustmentSummaryRule
{
    public List<string> Check(ScheduledAdjustmentSummary item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentSummaryLimitRangeRule(int Min, int Max) : IScheduledAdjustmentSummaryRule
{
    public List<string> Check(ScheduledAdjustmentSummary item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledAdjustmentSummaryAllowedCodeRule(List<string> Allowed) : IScheduledAdjustmentSummaryRule
{
    public List<string> Check(ScheduledAdjustmentSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled adjustment summary.</summary>
public sealed record ScheduledAdjustmentSummaryValidator(List<IScheduledAdjustmentSummaryRule> Rules)
{
    public List<string> Validate(ScheduledAdjustmentSummary item)
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

    public void RequireValid(ScheduledAdjustmentSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledAdjustmentSummary> items)
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

/// <summary>Functions of scheduled adjustment summary rules.</summary>
public static class ScheduledAdjustmentSummaryRules
{
    public static ScheduledAdjustmentSummaryValidator DefaultScheduledAdjustmentSummaryValidator()
    {
        return new ScheduledAdjustmentSummaryValidator(
            new List<IScheduledAdjustmentSummaryRule>
            {
                new ScheduledAdjustmentSummaryRequiredCodeRule("scheduled adjustment summary needs an entry"),
                new ScheduledAdjustmentSummaryChecksumLengthRule(120),
                new ScheduledAdjustmentSummaryCodeLengthRule(120),
                new ScheduledAdjustmentSummaryLimitRangeRule(3600, 86400),
                new ScheduledAdjustmentSummaryAllowedCodeRule(new List<string> { "batched" }),
            });
    }
}
