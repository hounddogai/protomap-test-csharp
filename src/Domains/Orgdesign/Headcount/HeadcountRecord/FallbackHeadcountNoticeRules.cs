namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>A rule a fallback headcount notice must pass. Check returns its problems, or none.</summary>
public interface IFallbackHeadcountNoticeRule
{
    List<string> Check(FallbackHeadcountNotice item);
}

public sealed record FallbackHeadcountNoticeBatchSizeRangeRule(int Min, int Max) : IFallbackHeadcountNoticeRule
{
    public List<string> Check(FallbackHeadcountNotice item)
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

public sealed record FallbackHeadcountNoticeRequiredCodeRule(string Message) : IFallbackHeadcountNoticeRule
{
    public List<string> Check(FallbackHeadcountNotice item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackHeadcountNoticeCodePrefixRule(string Prefix) : IFallbackHeadcountNoticeRule
{
    public List<string> Check(FallbackHeadcountNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackHeadcountNoticeRequiredTeamRule(string Message) : IFallbackHeadcountNoticeRule
{
    public List<string> Check(FallbackHeadcountNotice item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackHeadcountNoticeRatioRangeRule(double Min, double Max) : IFallbackHeadcountNoticeRule
{
    public List<string> Check(FallbackHeadcountNotice item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback headcount notice.</summary>
public sealed record FallbackHeadcountNoticeValidator(List<IFallbackHeadcountNoticeRule> Rules)
{
    public List<string> Validate(FallbackHeadcountNotice item)
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

    public void RequireValid(FallbackHeadcountNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackHeadcountNotice> items)
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

/// <summary>Functions of fallback headcount notice rules.</summary>
public static class FallbackHeadcountNoticeRules
{
    public static FallbackHeadcountNoticeValidator DefaultFallbackHeadcountNoticeValidator()
    {
        return new FallbackHeadcountNoticeValidator(
            new List<IFallbackHeadcountNoticeRule>
            {
                new FallbackHeadcountNoticeBatchSizeRangeRule(32, 1000),
                new FallbackHeadcountNoticeRequiredCodeRule("fallback headcount notice needs an entry"),
                new FallbackHeadcountNoticeCodePrefixRule("ACM"),
                new FallbackHeadcountNoticeRequiredTeamRule("fallback headcount notice needs an entry"),
                new FallbackHeadcountNoticeRatioRangeRule(3.4, 3100.8),
            });
    }
}
