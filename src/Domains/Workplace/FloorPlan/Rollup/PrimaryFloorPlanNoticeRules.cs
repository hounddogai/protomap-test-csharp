namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>A rule a primary floor plan notice must pass. Check returns its problems, or none.</summary>
public interface IPrimaryFloorPlanNoticeRule
{
    List<string> Check(PrimaryFloorPlanNotice item);
}

public sealed record PrimaryFloorPlanNoticeLabelLengthRule(int MaxLength) : IPrimaryFloorPlanNoticeRule
{
    public List<string> Check(PrimaryFloorPlanNotice item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanNoticeCodePrefixRule(string Prefix) : IPrimaryFloorPlanNoticeRule
{
    public List<string> Check(PrimaryFloorPlanNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanNoticePinnedGuardRule(int Limit) : IPrimaryFloorPlanNoticeRule
{
    public List<string> Check(PrimaryFloorPlanNotice item)
    {
        if (item.Pinned && item.Priority > Limit)
        {
            return new List<string> { $"priority {item.Priority} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanNoticeCodeLengthRule(int MaxLength) : IPrimaryFloorPlanNoticeRule
{
    public List<string> Check(PrimaryFloorPlanNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryFloorPlanNoticePriorityRangeRule(int Min, int Max) : IPrimaryFloorPlanNoticeRule
{
    public List<string> Check(PrimaryFloorPlanNotice item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary floor plan notice.</summary>
public sealed record PrimaryFloorPlanNoticeValidator(List<IPrimaryFloorPlanNoticeRule> Rules)
{
    public List<string> Validate(PrimaryFloorPlanNotice item)
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

    public void RequireValid(PrimaryFloorPlanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryFloorPlanNotice> items)
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

/// <summary>Functions of primary floor plan notice rules.</summary>
public static class PrimaryFloorPlanNoticeRules
{
    public static PrimaryFloorPlanNoticeValidator DefaultPrimaryFloorPlanNoticeValidator()
    {
        return new PrimaryFloorPlanNoticeValidator(
            new List<IPrimaryFloorPlanNoticeRule>
            {
                new PrimaryFloorPlanNoticeLabelLengthRule(64),
                new PrimaryFloorPlanNoticeCodePrefixRule("X"),
                new PrimaryFloorPlanNoticePinnedGuardRule(120),
                new PrimaryFloorPlanNoticeCodeLengthRule(86400),
                new PrimaryFloorPlanNoticePriorityRangeRule(64, 120),
            });
    }
}
