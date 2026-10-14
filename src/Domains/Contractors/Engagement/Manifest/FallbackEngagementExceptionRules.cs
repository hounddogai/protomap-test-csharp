namespace Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>A rule a fallback engagement exception must pass. Check returns its problems, or none.</summary>
public interface IFallbackEngagementExceptionRule
{
    List<string> Check(FallbackEngagementException item);
}

public sealed record FallbackEngagementExceptionRequiredCodeRule(string Message) : IFallbackEngagementExceptionRule
{
    public List<string> Check(FallbackEngagementException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementExceptionLeaveDaysRangeRule(int Min, int Max) : IFallbackEngagementExceptionRule
{
    public List<string> Check(FallbackEngagementException item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementExceptionRateRangeRule(double Min, double Max) : IFallbackEngagementExceptionRule
{
    public List<string> Check(FallbackEngagementException item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FallbackEngagementExceptionRequiredSegmentRule(string Message) : IFallbackEngagementExceptionRule
{
    public List<string> Check(FallbackEngagementException item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback engagement exception.</summary>
public sealed record FallbackEngagementExceptionValidator(List<IFallbackEngagementExceptionRule> Rules)
{
    public List<string> Validate(FallbackEngagementException item)
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

    public void RequireValid(FallbackEngagementException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackEngagementException> items)
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

/// <summary>Functions of fallback engagement exception rules.</summary>
public static class FallbackEngagementExceptionRules
{
    public static FallbackEngagementExceptionValidator DefaultFallbackEngagementExceptionValidator()
    {
        return new FallbackEngagementExceptionValidator(
            new List<IFallbackEngagementExceptionRule>
            {
                new FallbackEngagementExceptionRequiredCodeRule("fallback engagement exception needs this field"),
                new FallbackEngagementExceptionLeaveDaysRangeRule(32, 120),
                new FallbackEngagementExceptionRateRangeRule(7.8, 4601.9),
                new FallbackEngagementExceptionRequiredSegmentRule("fallback engagement exception needs an entry"),
            });
    }
}
