namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default headcount schedule must pass. Check returns its problems, or none.</summary>
public interface IDefaultHeadcountScheduleRule
{
    List<string> Check(DefaultHeadcountSchedule item);
}

public sealed record DefaultHeadcountScheduleCodePrefixRule(string Prefix) : IDefaultHeadcountScheduleRule
{
    public List<string> Check(DefaultHeadcountSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountScheduleUpdatedAtRangeRule(int Min, int Max) : IDefaultHeadcountScheduleRule
{
    public List<string> Check(DefaultHeadcountSchedule item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultHeadcountScheduleBatchSizeRangeRule(int Min, int Max) : IDefaultHeadcountScheduleRule
{
    public List<string> Check(DefaultHeadcountSchedule item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultHeadcountScheduleRequiredTitleRule(string Message) : IDefaultHeadcountScheduleRule
{
    public List<string> Check(DefaultHeadcountSchedule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountScheduleTitleLengthRule(int MaxLength) : IDefaultHeadcountScheduleRule
{
    public List<string> Check(DefaultHeadcountSchedule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountScheduleAllowedTitleRule(List<string> Allowed) : IDefaultHeadcountScheduleRule
{
    public List<string> Check(DefaultHeadcountSchedule item)
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

/// <summary>Runs every rule of a default headcount schedule.</summary>
public sealed record DefaultHeadcountScheduleValidator(List<IDefaultHeadcountScheduleRule> Rules)
{
    public List<string> Validate(DefaultHeadcountSchedule item)
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

    public void RequireValid(DefaultHeadcountSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHeadcountSchedule> items)
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

/// <summary>Functions of default headcount schedule rules.</summary>
public static class DefaultHeadcountScheduleRules
{
    public static DefaultHeadcountScheduleValidator DefaultDefaultHeadcountScheduleValidator()
    {
        return new DefaultHeadcountScheduleValidator(
            new List<IDefaultHeadcountScheduleRule>
            {
                new DefaultHeadcountScheduleCodePrefixRule("OPS"),
                new DefaultHeadcountScheduleUpdatedAtRangeRule(255, 86400),
                new DefaultHeadcountScheduleBatchSizeRangeRule(64, 80),
                new DefaultHeadcountScheduleRequiredTitleRule("default headcount schedule needs this field"),
                new DefaultHeadcountScheduleTitleLengthRule(16),
                new DefaultHeadcountScheduleAllowedTitleRule(new List<string>()),
            });
    }
}
