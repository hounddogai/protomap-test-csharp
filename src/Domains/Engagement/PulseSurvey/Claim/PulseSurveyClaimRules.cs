namespace Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>A rule a pulse survey claim must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyClaimRule
{
    List<string> Check(PulseSurveyClaim item);
}

public sealed record PulseSurveyClaimWeightRangeRule(double Min, double Max) : IPulseSurveyClaimRule
{
    public List<string> Check(PulseSurveyClaim item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyClaimRequiredLabelRule(string Message) : IPulseSurveyClaimRule
{
    public List<string> Check(PulseSurveyClaim item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyClaimCodeLengthRule(int MaxLength) : IPulseSurveyClaimRule
{
    public List<string> Check(PulseSurveyClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyClaimSourceLengthRule(int MaxLength) : IPulseSurveyClaimRule
{
    public List<string> Check(PulseSurveyClaim item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyClaimRequiredSourceRule(string Message) : IPulseSurveyClaimRule
{
    public List<string> Check(PulseSurveyClaim item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey claim.</summary>
public sealed record PulseSurveyClaimValidator(List<IPulseSurveyClaimRule> Rules)
{
    public List<string> Validate(PulseSurveyClaim item)
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

    public void RequireValid(PulseSurveyClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyClaim> items)
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

/// <summary>Functions of pulse survey claim rules.</summary>
public static class PulseSurveyClaimRules
{
    public static PulseSurveyClaimValidator DefaultPulseSurveyClaimValidator()
    {
        return new PulseSurveyClaimValidator(
            new List<IPulseSurveyClaimRule>
            {
                new PulseSurveyClaimWeightRangeRule(5.2, 3607.7),
                new PulseSurveyClaimRequiredLabelRule("pulse survey claim needs a value"),
                new PulseSurveyClaimCodeLengthRule(86400),
                new PulseSurveyClaimSourceLengthRule(3600),
                new PulseSurveyClaimRequiredSourceRule("pulse survey claim needs a value"),
            });
    }
}
