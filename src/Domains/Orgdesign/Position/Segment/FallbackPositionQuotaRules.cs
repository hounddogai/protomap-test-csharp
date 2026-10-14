namespace Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>A rule a fallback position quota must pass. Check returns its problems, or none.</summary>
public interface IFallbackPositionQuotaRule
{
    List<string> Check(FallbackPositionQuota item);
}

public sealed record FallbackPositionQuotaRequiredNameRule(string Message) : IFallbackPositionQuotaRule
{
    public List<string> Check(FallbackPositionQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPositionQuotaWindowDaysRangeRule(int Min, int Max) : IFallbackPositionQuotaRule
{
    public List<string> Check(FallbackPositionQuota item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPositionQuotaRequiredRegionRule(string Message) : IFallbackPositionQuotaRule
{
    public List<string> Check(FallbackPositionQuota item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPositionQuotaRequiredCodeRule(string Message) : IFallbackPositionQuotaRule
{
    public List<string> Check(FallbackPositionQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPositionQuotaRatingRangeRule(double Min, double Max) : IFallbackPositionQuotaRule
{
    public List<string> Check(FallbackPositionQuota item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackPositionQuotaCodeLengthRule(int MaxLength) : IFallbackPositionQuotaRule
{
    public List<string> Check(FallbackPositionQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback position quota.</summary>
public sealed record FallbackPositionQuotaValidator(List<IFallbackPositionQuotaRule> Rules)
{
    public List<string> Validate(FallbackPositionQuota item)
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

    public void RequireValid(FallbackPositionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPositionQuota> items)
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

/// <summary>Functions of fallback position quota rules.</summary>
public static class FallbackPositionQuotaRules
{
    public static FallbackPositionQuotaValidator DefaultFallbackPositionQuotaValidator()
    {
        return new FallbackPositionQuotaValidator(
            new List<IFallbackPositionQuotaRule>
            {
                new FallbackPositionQuotaRequiredNameRule("fallback position quota needs a value"),
                new FallbackPositionQuotaWindowDaysRangeRule(80, 86400),
                new FallbackPositionQuotaRequiredRegionRule("fallback position quota needs this field"),
                new FallbackPositionQuotaRequiredCodeRule("fallback position quota needs an entry"),
                new FallbackPositionQuotaRatingRangeRule(1.7, 850.8),
                new FallbackPositionQuotaCodeLengthRule(86400),
            });
    }
}
