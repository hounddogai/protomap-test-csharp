namespace Acme.Workforce.Engagement.ActionPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient action plan notice must pass. Check returns its problems, or none.</summary>
public interface ILenientActionPlanNoticeRule
{
    List<string> Check(LenientActionPlanNotice item);
}

public sealed record LenientActionPlanNoticeAllowedLocationRule(List<string> Allowed) : ILenientActionPlanNoticeRule
{
    public List<string> Check(LenientActionPlanNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Location)))
        {
            return new List<string> { $"location {item.Location} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanNoticeCodePrefixRule(string Prefix) : ILenientActionPlanNoticeRule
{
    public List<string> Check(LenientActionPlanNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanNoticeLimitRangeRule(int Min, int Max) : ILenientActionPlanNoticeRule
{
    public List<string> Check(LenientActionPlanNotice item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanNoticeSegmentLengthRule(int MaxLength) : ILenientActionPlanNoticeRule
{
    public List<string> Check(LenientActionPlanNotice item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient action plan notice.</summary>
public sealed record LenientActionPlanNoticeValidator(List<ILenientActionPlanNoticeRule> Rules)
{
    public List<string> Validate(LenientActionPlanNotice item)
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

    public void RequireValid(LenientActionPlanNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientActionPlanNotice> items)
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

/// <summary>Functions of lenient action plan notice rules.</summary>
public static class LenientActionPlanNoticeRules
{
    public static LenientActionPlanNoticeValidator DefaultLenientActionPlanNoticeValidator()
    {
        return new LenientActionPlanNoticeValidator(
            new List<ILenientActionPlanNoticeRule>
            {
                new LenientActionPlanNoticeAllowedLocationRule(new List<string> { "adaptive" }),
                new LenientActionPlanNoticeCodePrefixRule("REF"),
                new LenientActionPlanNoticeLimitRangeRule(64, 1000),
                new LenientActionPlanNoticeSegmentLengthRule(16),
            });
    }
}
