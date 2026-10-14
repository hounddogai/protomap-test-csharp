namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>A rule a lenient pay band score must pass. Check returns its problems, or none.</summary>
public interface ILenientPayBandScoreRule
{
    List<string> Check(LenientPayBandScore item);
}

public sealed record LenientPayBandScoreDescriptionLengthRule(int MaxLength) : ILenientPayBandScoreRule
{
    public List<string> Check(LenientPayBandScore item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandScoreBadgeLengthRule(int MaxLength) : ILenientPayBandScoreRule
{
    public List<string> Check(LenientPayBandScore item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandScoreRequiredBadgeRule(string Message) : ILenientPayBandScoreRule
{
    public List<string> Check(LenientPayBandScore item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandScoreRequiredLabelRule(string Message) : ILenientPayBandScoreRule
{
    public List<string> Check(LenientPayBandScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandScoreRequiredCodeRule(string Message) : ILenientPayBandScoreRule
{
    public List<string> Check(LenientPayBandScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPayBandScoreLimitRangeRule(int Min, int Max) : ILenientPayBandScoreRule
{
    public List<string> Check(LenientPayBandScore item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a lenient pay band score.</summary>
public sealed record LenientPayBandScoreValidator(List<ILenientPayBandScoreRule> Rules)
{
    public List<string> Validate(LenientPayBandScore item)
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

    public void RequireValid(LenientPayBandScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPayBandScore> items)
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

/// <summary>Functions of lenient pay band score rules.</summary>
public static class LenientPayBandScoreRules
{
    public static LenientPayBandScoreValidator DefaultLenientPayBandScoreValidator()
    {
        return new LenientPayBandScoreValidator(
            new List<ILenientPayBandScoreRule>
            {
                new LenientPayBandScoreDescriptionLengthRule(3600),
                new LenientPayBandScoreBadgeLengthRule(80),
                new LenientPayBandScoreRequiredBadgeRule("lenient pay band score needs this field"),
                new LenientPayBandScoreRequiredLabelRule("lenient pay band score needs this field"),
                new LenientPayBandScoreRequiredCodeRule("lenient pay band score needs this field"),
                new LenientPayBandScoreLimitRangeRule(16, 80),
            });
    }
}
