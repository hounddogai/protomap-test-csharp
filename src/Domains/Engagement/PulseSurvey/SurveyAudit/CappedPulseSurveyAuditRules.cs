namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>A rule a capped pulse survey audit must pass. Check returns its problems, or none.</summary>
public interface ICappedPulseSurveyAuditRule
{
    List<string> Check(CappedPulseSurveyAudit item);
}

public sealed record CappedPulseSurveyAuditRequiredCodeRule(string Message) : ICappedPulseSurveyAuditRule
{
    public List<string> Check(CappedPulseSurveyAudit item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPulseSurveyAuditCodePrefixRule(string Prefix) : ICappedPulseSurveyAuditRule
{
    public List<string> Check(CappedPulseSurveyAudit item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPulseSurveyAuditQuantityRangeRule(int Min, int Max) : ICappedPulseSurveyAuditRule
{
    public List<string> Check(CappedPulseSurveyAudit item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedPulseSurveyAuditTimezoneLengthRule(int MaxLength) : ICappedPulseSurveyAuditRule
{
    public List<string> Check(CappedPulseSurveyAudit item)
    {
        var size = item.Timezone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"timezone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped pulse survey audit.</summary>
public sealed record CappedPulseSurveyAuditValidator(List<ICappedPulseSurveyAuditRule> Rules)
{
    public List<string> Validate(CappedPulseSurveyAudit item)
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

    public void RequireValid(CappedPulseSurveyAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPulseSurveyAudit> items)
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

/// <summary>Functions of capped pulse survey audit rules.</summary>
public static class CappedPulseSurveyAuditRules
{
    public static CappedPulseSurveyAuditValidator DefaultCappedPulseSurveyAuditValidator()
    {
        return new CappedPulseSurveyAuditValidator(
            new List<ICappedPulseSurveyAuditRule>
            {
                new CappedPulseSurveyAuditRequiredCodeRule("capped pulse survey audit needs an entry"),
                new CappedPulseSurveyAuditCodePrefixRule("X"),
                new CappedPulseSurveyAuditQuantityRangeRule(120, 255),
                new CappedPulseSurveyAuditTimezoneLengthRule(120),
            });
    }
}
