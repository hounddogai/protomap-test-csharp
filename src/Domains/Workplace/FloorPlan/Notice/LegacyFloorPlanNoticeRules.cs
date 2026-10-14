namespace Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>A rule a legacy floor plan notice must pass. Check returns its problems, or none.</summary>
public interface ILegacyFloorPlanNoticeRule
{
    List<string> Check(LegacyFloorPlanNotice item);
}

public sealed record LegacyFloorPlanNoticeRequiredTitleRule(string Message) : ILegacyFloorPlanNoticeRule
{
    public List<string> Check(LegacyFloorPlanNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanNoticeCodeLengthRule(int MaxLength) : ILegacyFloorPlanNoticeRule
{
    public List<string> Check(LegacyFloorPlanNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanNoticeSummaryLengthRule(int MaxLength) : ILegacyFloorPlanNoticeRule
{
    public List<string> Check(LegacyFloorPlanNotice item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanNoticeCodePrefixRule(string Prefix) : ILegacyFloorPlanNoticeRule
{
    public List<string> Check(LegacyFloorPlanNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyFloorPlanNoticeTitleLengthRule(int MaxLength) : ILegacyFloorPlanNoticeRule
{
    public List<string> Check(LegacyFloorPlanNotice item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy floor plan notice.</summary>
public sealed record LegacyFloorPlanNoticeValidator(List<ILegacyFloorPlanNoticeRule> Rules)
{
    public List<string> Validate(LegacyFloorPlanNotice item)
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

    public void RequireValid(LegacyFloorPlanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyFloorPlanNotice> items)
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

/// <summary>Functions of legacy floor plan notice rules.</summary>
public static class LegacyFloorPlanNoticeRules
{
    public static LegacyFloorPlanNoticeValidator DefaultLegacyFloorPlanNoticeValidator()
    {
        return new LegacyFloorPlanNoticeValidator(
            new List<ILegacyFloorPlanNoticeRule>
            {
                new LegacyFloorPlanNoticeRequiredTitleRule("legacy floor plan notice needs a value"),
                new LegacyFloorPlanNoticeCodeLengthRule(1000),
                new LegacyFloorPlanNoticeSummaryLengthRule(120),
                new LegacyFloorPlanNoticeCodePrefixRule("Q"),
                new LegacyFloorPlanNoticeTitleLengthRule(16),
            });
    }
}
