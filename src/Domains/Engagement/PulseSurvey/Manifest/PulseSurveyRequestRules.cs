namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>A rule a pulse survey request must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyRequestRule
{
    List<string> Check(PulseSurveyRequest item);
}

public sealed record PulseSurveyRequestCurrencyLengthRule(int MaxLength) : IPulseSurveyRequestRule
{
    public List<string> Check(PulseSurveyRequest item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRequestVisibleGuardRule(int Limit) : IPulseSurveyRequestRule
{
    public List<string> Check(PulseSurveyRequest item)
    {
        if (item.Visible && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRequestRequiredCurrencyRule(string Message) : IPulseSurveyRequestRule
{
    public List<string> Check(PulseSurveyRequest item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyRequestDiscountRangeRule(double Min, double Max) : IPulseSurveyRequestRule
{
    public List<string> Check(PulseSurveyRequest item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey request.</summary>
public sealed record PulseSurveyRequestValidator(List<IPulseSurveyRequestRule> Rules)
{
    public List<string> Validate(PulseSurveyRequest item)
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

    public void RequireValid(PulseSurveyRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyRequest> items)
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

/// <summary>Functions of pulse survey request rules.</summary>
public static class PulseSurveyRequestRules
{
    public static PulseSurveyRequestValidator DefaultPulseSurveyRequestValidator()
    {
        return new PulseSurveyRequestValidator(
            new List<IPulseSurveyRequestRule>
            {
                new PulseSurveyRequestCurrencyLengthRule(16),
                new PulseSurveyRequestVisibleGuardRule(120),
                new PulseSurveyRequestRequiredCurrencyRule("pulse survey request needs an entry"),
                new PulseSurveyRequestDiscountRangeRule(6.8, 3464.6),
            });
    }
}
