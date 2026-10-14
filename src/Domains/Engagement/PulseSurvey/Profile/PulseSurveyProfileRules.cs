namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey profile must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyProfileRule
{
    List<string> Check(PulseSurveyProfile item);
}

public sealed record PulseSurveyProfileCodePrefixRule(string Prefix) : IPulseSurveyProfileRule
{
    public List<string> Check(PulseSurveyProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyProfileRateRangeRule(double Min, double Max) : IPulseSurveyProfileRule
{
    public List<string> Check(PulseSurveyProfile item)
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

public sealed record PulseSurveyProfileRequiredEmployeeIdRule(string Message) : IPulseSurveyProfileRule
{
    public List<string> Check(PulseSurveyProfile item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyProfileSegmentLengthRule(int MaxLength) : IPulseSurveyProfileRule
{
    public List<string> Check(PulseSurveyProfile item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyProfileAllowedUnitRule(List<string> Allowed) : IPulseSurveyProfileRule
{
    public List<string> Check(PulseSurveyProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Unit)))
        {
            return new List<string> { $"unit {item.Unit} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey profile.</summary>
public sealed record PulseSurveyProfileValidator(List<IPulseSurveyProfileRule> Rules)
{
    public List<string> Validate(PulseSurveyProfile item)
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

    public void RequireValid(PulseSurveyProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyProfile> items)
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

/// <summary>Functions of pulse survey profile rules.</summary>
public static class PulseSurveyProfileRules
{
    public static PulseSurveyProfileValidator DefaultPulseSurveyProfileValidator()
    {
        return new PulseSurveyProfileValidator(
            new List<IPulseSurveyProfileRule>
            {
                new PulseSurveyProfileCodePrefixRule("REF"),
                new PulseSurveyProfileRateRangeRule(8.2, 2315.8),
                new PulseSurveyProfileRequiredEmployeeIdRule("pulse survey profile needs this field"),
                new PulseSurveyProfileSegmentLengthRule(1000),
                new PulseSurveyProfileAllowedUnitRule(new List<string> { "seasonal" }),
            });
    }
}
