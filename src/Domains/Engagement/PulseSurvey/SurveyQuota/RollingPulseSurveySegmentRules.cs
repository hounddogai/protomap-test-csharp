namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>A rule a rolling pulse survey segment must pass. Check returns its problems, or none.</summary>
public interface IRollingPulseSurveySegmentRule
{
    List<string> Check(RollingPulseSurveySegment item);
}

public sealed record RollingPulseSurveySegmentLabelLengthRule(int MaxLength) : IRollingPulseSurveySegmentRule
{
    public List<string> Check(RollingPulseSurveySegment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPulseSurveySegmentMinimumRangeRule(double Min, double Max) : IRollingPulseSurveySegmentRule
{
    public List<string> Check(RollingPulseSurveySegment item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPulseSurveySegmentCodeLengthRule(int MaxLength) : IRollingPulseSurveySegmentRule
{
    public List<string> Check(RollingPulseSurveySegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling pulse survey segment.</summary>
public sealed record RollingPulseSurveySegmentValidator(List<IRollingPulseSurveySegmentRule> Rules)
{
    public List<string> Validate(RollingPulseSurveySegment item)
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

    public void RequireValid(RollingPulseSurveySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPulseSurveySegment> items)
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

/// <summary>Functions of rolling pulse survey segment rules.</summary>
public static class RollingPulseSurveySegmentRules
{
    public static RollingPulseSurveySegmentValidator DefaultRollingPulseSurveySegmentValidator()
    {
        return new RollingPulseSurveySegmentValidator(
            new List<IRollingPulseSurveySegmentRule>
            {
                new RollingPulseSurveySegmentLabelLengthRule(120),
                new RollingPulseSurveySegmentMinimumRangeRule(2.8, 3609.1),
                new RollingPulseSurveySegmentCodeLengthRule(1000),
            });
    }
}
