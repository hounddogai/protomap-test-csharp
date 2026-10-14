namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>A rule a pulse survey segment must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveySegmentRule
{
    List<string> Check(PulseSurveySegment item);
}

public sealed record PulseSurveySegmentCodePrefixRule(string Prefix) : IPulseSurveySegmentRule
{
    public List<string> Check(PulseSurveySegment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySegmentLabelLengthRule(int MaxLength) : IPulseSurveySegmentRule
{
    public List<string> Check(PulseSurveySegment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveySegmentFteRangeRule(double Min, double Max) : IPulseSurveySegmentRule
{
    public List<string> Check(PulseSurveySegment item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a pulse survey segment.</summary>
public sealed record PulseSurveySegmentValidator(List<IPulseSurveySegmentRule> Rules)
{
    public List<string> Validate(PulseSurveySegment item)
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

    public void RequireValid(PulseSurveySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveySegment> items)
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

/// <summary>Functions of pulse survey segment rules.</summary>
public static class PulseSurveySegmentRules
{
    public static PulseSurveySegmentValidator DefaultPulseSurveySegmentValidator()
    {
        return new PulseSurveySegmentValidator(
            new List<IPulseSurveySegmentRule>
            {
                new PulseSurveySegmentCodePrefixRule("ACM"),
                new PulseSurveySegmentLabelLengthRule(32),
                new PulseSurveySegmentFteRangeRule(9.4, 1015.1),
            });
    }
}
