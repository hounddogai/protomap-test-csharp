namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered recognition score must pass. Check returns its problems, or none.</summary>
public interface ITieredRecognitionScoreRule
{
    List<string> Check(TieredRecognitionScore item);
}

public sealed record TieredRecognitionScoreCodePrefixRule(string Prefix) : ITieredRecognitionScoreRule
{
    public List<string> Check(TieredRecognitionScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredRecognitionScoreRequiredCodeRule(string Message) : ITieredRecognitionScoreRule
{
    public List<string> Check(TieredRecognitionScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRecognitionScoreRequiredPhoneRule(string Message) : ITieredRecognitionScoreRule
{
    public List<string> Check(TieredRecognitionScore item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredRecognitionScoreNameLengthRule(int MaxLength) : ITieredRecognitionScoreRule
{
    public List<string> Check(TieredRecognitionScore item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredRecognitionScoreAllowedCodeRule(List<string> Allowed) : ITieredRecognitionScoreRule
{
    public List<string> Check(TieredRecognitionScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TieredRecognitionScoreHoursRangeRule(double Min, double Max) : ITieredRecognitionScoreRule
{
    public List<string> Check(TieredRecognitionScore item)
    {
        var valueValue = item.Hours;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"hours {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered recognition score.</summary>
public sealed record TieredRecognitionScoreValidator(List<ITieredRecognitionScoreRule> Rules)
{
    public List<string> Validate(TieredRecognitionScore item)
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

    public void RequireValid(TieredRecognitionScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredRecognitionScore> items)
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

/// <summary>Functions of tiered recognition score rules.</summary>
public static class TieredRecognitionScoreRules
{
    public static TieredRecognitionScoreValidator DefaultTieredRecognitionScoreValidator()
    {
        return new TieredRecognitionScoreValidator(
            new List<ITieredRecognitionScoreRule>
            {
                new TieredRecognitionScoreCodePrefixRule("Q"),
                new TieredRecognitionScoreRequiredCodeRule("tiered recognition score needs this field"),
                new TieredRecognitionScoreRequiredPhoneRule("tiered recognition score needs a value"),
                new TieredRecognitionScoreNameLengthRule(3600),
                new TieredRecognitionScoreAllowedCodeRule(new List<string> { "weighted", "lenient" }),
                new TieredRecognitionScoreHoursRangeRule(7.4, 4425.5),
            });
    }
}
