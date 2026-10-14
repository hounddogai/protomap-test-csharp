namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>A rule a pulse survey estimate must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyEstimateRule
{
    List<string> Check(PulseSurveyEstimate item);
}

public sealed record PulseSurveyEstimateRequiredNameRule(string Message) : IPulseSurveyEstimateRule
{
    public List<string> Check(PulseSurveyEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyEstimateNameLengthRule(int MaxLength) : IPulseSurveyEstimateRule
{
    public List<string> Check(PulseSurveyEstimate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyEstimateCodePrefixRule(string Prefix) : IPulseSurveyEstimateRule
{
    public List<string> Check(PulseSurveyEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey estimate.</summary>
public sealed record PulseSurveyEstimateValidator(List<IPulseSurveyEstimateRule> Rules)
{
    public List<string> Validate(PulseSurveyEstimate item)
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

    public void RequireValid(PulseSurveyEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyEstimate> items)
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

/// <summary>Functions of pulse survey estimate rules.</summary>
public static class PulseSurveyEstimateRules
{
    public static PulseSurveyEstimateValidator DefaultPulseSurveyEstimateValidator()
    {
        return new PulseSurveyEstimateValidator(
            new List<IPulseSurveyEstimateRule>
            {
                new PulseSurveyEstimateRequiredNameRule("pulse survey estimate needs a value"),
                new PulseSurveyEstimateNameLengthRule(120),
                new PulseSurveyEstimateCodePrefixRule("Q"),
            });
    }
}
