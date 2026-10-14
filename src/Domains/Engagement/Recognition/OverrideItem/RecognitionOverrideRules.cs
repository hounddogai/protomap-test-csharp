namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a recognition override must pass. Check returns its problems, or none.</summary>
public interface IRecognitionOverrideRule
{
    List<string> Check(RecognitionOverride item);
}

public sealed record RecognitionOverrideCodePrefixRule(string Prefix) : IRecognitionOverrideRule
{
    public List<string> Check(RecognitionOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionOverrideAllowedTitleRule(List<string> Allowed) : IRecognitionOverrideRule
{
    public List<string> Check(RecognitionOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionOverrideCodeLengthRule(int MaxLength) : IRecognitionOverrideRule
{
    public List<string> Check(RecognitionOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a recognition override.</summary>
public sealed record RecognitionOverrideValidator(List<IRecognitionOverrideRule> Rules)
{
    public List<string> Validate(RecognitionOverride item)
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

    public void RequireValid(RecognitionOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionOverride> items)
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

/// <summary>Functions of recognition override rules.</summary>
public static class RecognitionOverrideRules
{
    public static RecognitionOverrideValidator DefaultRecognitionOverrideValidator()
    {
        return new RecognitionOverrideValidator(
            new List<IRecognitionOverrideRule>
            {
                new RecognitionOverrideCodePrefixRule("REF"),
                new RecognitionOverrideAllowedTitleRule(new List<string>()),
                new RecognitionOverrideCodeLengthRule(32),
            });
    }
}
