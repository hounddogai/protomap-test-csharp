namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pulse survey quota must pass. Check returns its problems, or none.</summary>
public interface IPulseSurveyQuotaRule
{
    List<string> Check(PulseSurveyQuota item);
}

public sealed record PulseSurveyQuotaCodeLengthRule(int MaxLength) : IPulseSurveyQuotaRule
{
    public List<string> Check(PulseSurveyQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyQuotaTimezoneLengthRule(int MaxLength) : IPulseSurveyQuotaRule
{
    public List<string> Check(PulseSurveyQuota item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyQuotaAmountRangeRule(double Min, double Max) : IPulseSurveyQuotaRule
{
    public List<string> Check(PulseSurveyQuota item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PulseSurveyQuotaRequiredTimezoneRule(string Message) : IPulseSurveyQuotaRule
{
    public List<string> Check(PulseSurveyQuota item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyQuotaLocationLengthRule(int MaxLength) : IPulseSurveyQuotaRule
{
    public List<string> Check(PulseSurveyQuota item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PulseSurveyQuotaAllowedNameRule(List<string> Allowed) : IPulseSurveyQuotaRule
{
    public List<string> Check(PulseSurveyQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pulse survey quota.</summary>
public sealed record PulseSurveyQuotaValidator(List<IPulseSurveyQuotaRule> Rules)
{
    public List<string> Validate(PulseSurveyQuota item)
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

    public void RequireValid(PulseSurveyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PulseSurveyQuota> items)
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

/// <summary>Functions of pulse survey quota rules.</summary>
public static class PulseSurveyQuotaRules
{
    public static PulseSurveyQuotaValidator DefaultPulseSurveyQuotaValidator()
    {
        return new PulseSurveyQuotaValidator(
            new List<IPulseSurveyQuotaRule>
            {
                new PulseSurveyQuotaCodeLengthRule(120),
                new PulseSurveyQuotaTimezoneLengthRule(3600),
                new PulseSurveyQuotaAmountRangeRule(7.4, 3776.6),
                new PulseSurveyQuotaRequiredTimezoneRule("pulse survey quota needs a value"),
                new PulseSurveyQuotaLocationLengthRule(3600),
                new PulseSurveyQuotaAllowedNameRule(new List<string> { "legacy", "legacy" }),
            });
    }
}
