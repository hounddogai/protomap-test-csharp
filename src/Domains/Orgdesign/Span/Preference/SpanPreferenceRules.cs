namespace Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>A rule a span preference must pass. Check returns its problems, or none.</summary>
public interface ISpanPreferenceRule
{
    List<string> Check(SpanPreference item);
}

public sealed record SpanPreferenceRequiredCodeRule(string Message) : ISpanPreferenceRule
{
    public List<string> Check(SpanPreference item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPreferenceChecksumLengthRule(int MaxLength) : ISpanPreferenceRule
{
    public List<string> Check(SpanPreference item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SpanPreferenceArchivedGuardRule(double Limit) : ISpanPreferenceRule
{
    public List<string> Check(SpanPreference item)
    {
        if (item.Archived && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record SpanPreferenceRequiredTitleRule(string Message) : ISpanPreferenceRule
{
    public List<string> Check(SpanPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SpanPreferenceScoreRangeRule(double Min, double Max) : ISpanPreferenceRule
{
    public List<string> Check(SpanPreference item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a span preference.</summary>
public sealed record SpanPreferenceValidator(List<ISpanPreferenceRule> Rules)
{
    public List<string> Validate(SpanPreference item)
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

    public void RequireValid(SpanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SpanPreference> items)
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

/// <summary>Functions of span preference rules.</summary>
public static class SpanPreferenceRules
{
    public static SpanPreferenceValidator DefaultSpanPreferenceValidator()
    {
        return new SpanPreferenceValidator(
            new List<ISpanPreferenceRule>
            {
                new SpanPreferenceRequiredCodeRule("span preference needs an entry"),
                new SpanPreferenceChecksumLengthRule(32),
                new SpanPreferenceArchivedGuardRule(9.1),
                new SpanPreferenceRequiredTitleRule("span preference needs this field"),
                new SpanPreferenceScoreRangeRule(4.7, 4398.0),
            });
    }
}
