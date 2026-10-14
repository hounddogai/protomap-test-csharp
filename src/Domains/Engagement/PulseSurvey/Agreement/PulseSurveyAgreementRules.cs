namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>A rule a pulse survey agreement must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyAgreementRule
{
    List<string> Check(PulseSurveyAgreement item);
}

public sealed record PulseSurveyAgreementCodeLengthRule(int MaxLength) : IPulseSurveyAgreementRule
{
    public List<string> Check(PulseSurveyAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAgreementCodePrefixRule(string Prefix) : IPulseSurveyAgreementRule
{
    public List<string> Check(PulseSurveyAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAgreementManagerIdLengthRule(int MaxLength) : IPulseSurveyAgreementRule
{
    public List<string> Check(PulseSurveyAgreement item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAgreementTitleLengthRule(int MaxLength) : IPulseSurveyAgreementRule
{
    public List<string> Check(PulseSurveyAgreement item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyAgreementAmountRangeRule(double Min, double Max) : IPulseSurveyAgreementRule
{
    public List<string> Check(PulseSurveyAgreement item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey agreement.</summary>
public sealed record PulseSurveyAgreementValidator(List<IPulseSurveyAgreementRule> Rules)
{
    public List<string> Validate(PulseSurveyAgreement item)
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

    public void RequireValid(PulseSurveyAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyAgreement> items)
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

/// <summary>Functions of pulse survey agreement rules.</summary>
public static class PulseSurveyAgreementRules
{
    public static PulseSurveyAgreementValidator DefaultPulseSurveyAgreementValidator()
    {
        return new PulseSurveyAgreementValidator(
            new List<IPulseSurveyAgreementRule>
            {
                new PulseSurveyAgreementCodeLengthRule(64),
                new PulseSurveyAgreementCodePrefixRule("OPS"),
                new PulseSurveyAgreementManagerIdLengthRule(32),
                new PulseSurveyAgreementTitleLengthRule(1000),
                new PulseSurveyAgreementAmountRangeRule(8.3, 2305.0),
            });
    }
}
