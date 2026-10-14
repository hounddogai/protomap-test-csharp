namespace Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>A rule a manual access badge notice must pass. Check returns its problems, or none.</summary>
public interface IManualAccessBadgeNoticeRule
{
    List<string> Check(ManualAccessBadgeNotice item);
}

public sealed record ManualAccessBadgeNoticePriorityRangeRule(int Min, int Max) : IManualAccessBadgeNoticeRule
{
    public List<string> Check(ManualAccessBadgeNotice item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAccessBadgeNoticeRequiredSalaryBandRule(string Message) : IManualAccessBadgeNoticeRule
{
    public List<string> Check(ManualAccessBadgeNotice item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeNoticeRequiredCostCenterRule(string Message) : IManualAccessBadgeNoticeRule
{
    public List<string> Check(ManualAccessBadgeNotice item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeNoticeCodePrefixRule(string Prefix) : IManualAccessBadgeNoticeRule
{
    public List<string> Check(ManualAccessBadgeNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeNoticeCodeLengthRule(int MaxLength) : IManualAccessBadgeNoticeRule
{
    public List<string> Check(ManualAccessBadgeNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual access badge notice.</summary>
public sealed record ManualAccessBadgeNoticeValidator(List<IManualAccessBadgeNoticeRule> Rules)
{
    public List<string> Validate(ManualAccessBadgeNotice item)
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

    public void RequireValid(ManualAccessBadgeNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccessBadgeNotice> items)
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

/// <summary>Functions of manual access badge notice rules.</summary>
public static class ManualAccessBadgeNoticeRules
{
    public static ManualAccessBadgeNoticeValidator DefaultManualAccessBadgeNoticeValidator()
    {
        return new ManualAccessBadgeNoticeValidator(
            new List<IManualAccessBadgeNoticeRule>
            {
                new ManualAccessBadgeNoticePriorityRangeRule(1000, 3600),
                new ManualAccessBadgeNoticeRequiredSalaryBandRule("manual access badge notice needs this field"),
                new ManualAccessBadgeNoticeRequiredCostCenterRule("manual access badge notice needs a value"),
                new ManualAccessBadgeNoticeCodePrefixRule("REF"),
                new ManualAccessBadgeNoticeCodeLengthRule(32),
            });
    }
}
