namespace Acme.Workforce.Engagement.Recognition.Score;

/// <summary>A rule a recognition policy must pass. Check returns its problems, or none.</summary>
public interface IRecognitionPolicyRule
{
    List<string> Check(RecognitionPolicy item);
}

public sealed record RecognitionPolicyCodePrefixRule(string Prefix) : IRecognitionPolicyRule
{
    public List<string> Check(RecognitionPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPolicyRequiredCodeRule(string Message) : IRecognitionPolicyRule
{
    public List<string> Check(RecognitionPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPolicyScoreRangeRule(double Min, double Max) : IRecognitionPolicyRule
{
    public List<string> Check(RecognitionPolicy item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RecognitionPolicyRatioRangeRule(double Min, double Max) : IRecognitionPolicyRule
{
    public List<string> Check(RecognitionPolicy item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a recognition policy.</summary>
public sealed record RecognitionPolicyValidator(List<IRecognitionPolicyRule> Rules)
{
    public List<string> Validate(RecognitionPolicy item)
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

    public void RequireValid(RecognitionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RecognitionPolicy> items)
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

/// <summary>Functions of recognition policy rules.</summary>
public static class RecognitionPolicyRules
{
    public static RecognitionPolicyValidator DefaultRecognitionPolicyValidator()
    {
        return new RecognitionPolicyValidator(
            new List<IRecognitionPolicyRule>
            {
                new RecognitionPolicyCodePrefixRule("REF"),
                new RecognitionPolicyRequiredCodeRule("recognition policy needs a value"),
                new RecognitionPolicyScoreRangeRule(4.7, 863.3),
                new RecognitionPolicyRatioRangeRule(7.9, 2496.3),
            });
    }
}
