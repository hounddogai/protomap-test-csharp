namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive town hall notice must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveTownHallNoticeRule
{
    List<string> Check(AdaptiveTownHallNotice item);
}

public sealed record AdaptiveTownHallNoticeAllowedCodeRule(List<string> Allowed) : IAdaptiveTownHallNoticeRule
{
    public List<string> Check(AdaptiveTownHallNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallNoticeCostCenterLengthRule(int MaxLength) : IAdaptiveTownHallNoticeRule
{
    public List<string> Check(AdaptiveTownHallNotice item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallNoticeCostRangeRule(double Min, double Max) : IAdaptiveTownHallNoticeRule
{
    public List<string> Check(AdaptiveTownHallNotice item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallNoticeSegmentLengthRule(int MaxLength) : IAdaptiveTownHallNoticeRule
{
    public List<string> Check(AdaptiveTownHallNotice item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallNoticeRequiredTitleRule(string Message) : IAdaptiveTownHallNoticeRule
{
    public List<string> Check(AdaptiveTownHallNotice item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveTownHallNoticeRequiredSegmentRule(string Message) : IAdaptiveTownHallNoticeRule
{
    public List<string> Check(AdaptiveTownHallNotice item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive town hall notice.</summary>
public sealed record AdaptiveTownHallNoticeValidator(List<IAdaptiveTownHallNoticeRule> Rules)
{
    public List<string> Validate(AdaptiveTownHallNotice item)
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

    public void RequireValid(AdaptiveTownHallNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveTownHallNotice> items)
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

/// <summary>Functions of adaptive town hall notice rules.</summary>
public static class AdaptiveTownHallNoticeRules
{
    public static AdaptiveTownHallNoticeValidator DefaultAdaptiveTownHallNoticeValidator()
    {
        return new AdaptiveTownHallNoticeValidator(
            new List<IAdaptiveTownHallNoticeRule>
            {
                new AdaptiveTownHallNoticeAllowedCodeRule(new List<string> { "global", "strict" }),
                new AdaptiveTownHallNoticeCostCenterLengthRule(80),
                new AdaptiveTownHallNoticeCostRangeRule(1.3, 4012.3),
                new AdaptiveTownHallNoticeSegmentLengthRule(86400),
                new AdaptiveTownHallNoticeRequiredTitleRule("adaptive town hall notice needs this field"),
                new AdaptiveTownHallNoticeRequiredSegmentRule("adaptive town hall notice needs this field"),
            });
    }
}
