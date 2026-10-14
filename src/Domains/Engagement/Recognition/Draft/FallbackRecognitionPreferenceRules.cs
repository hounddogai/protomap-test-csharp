namespace Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>A rule a fallback recognition preference must pass. Check returns its problems, or none.</summary>
public interface IFallbackRecognitionPreferenceRule
{
    List<string> Check(FallbackRecognitionPreference item);
}

public sealed record FallbackRecognitionPreferenceRequiredCountryRule(string Message) : IFallbackRecognitionPreferenceRule
{
    public List<string> Check(FallbackRecognitionPreference item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionPreferenceCodeLengthRule(int MaxLength) : IFallbackRecognitionPreferenceRule
{
    public List<string> Check(FallbackRecognitionPreference item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionPreferenceWindowDaysRangeRule(int Min, int Max) : IFallbackRecognitionPreferenceRule
{
    public List<string> Check(FallbackRecognitionPreference item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionPreferenceNameLengthRule(int MaxLength) : IFallbackRecognitionPreferenceRule
{
    public List<string> Check(FallbackRecognitionPreference item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback recognition preference.</summary>
public sealed record FallbackRecognitionPreferenceValidator(List<IFallbackRecognitionPreferenceRule> Rules)
{
    public List<string> Validate(FallbackRecognitionPreference item)
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

    public void RequireValid(FallbackRecognitionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRecognitionPreference> items)
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

/// <summary>Functions of fallback recognition preference rules.</summary>
public static class FallbackRecognitionPreferenceRules
{
    public static FallbackRecognitionPreferenceValidator DefaultFallbackRecognitionPreferenceValidator()
    {
        return new FallbackRecognitionPreferenceValidator(
            new List<IFallbackRecognitionPreferenceRule>
            {
                new FallbackRecognitionPreferenceRequiredCountryRule("fallback recognition preference needs a value"),
                new FallbackRecognitionPreferenceCodeLengthRule(255),
                new FallbackRecognitionPreferenceWindowDaysRangeRule(16, 120),
                new FallbackRecognitionPreferenceNameLengthRule(1000),
            });
    }
}
