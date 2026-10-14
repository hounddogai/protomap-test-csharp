namespace Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>A rule a manual pulse survey quota must pass. Check returns its problems, or none.</summary>
public interface IManualPulseSurveyQuotaRule
{
    List<string> Check(ManualPulseSurveyQuota item);
}

public sealed record ManualPulseSurveyQuotaCodeLengthRule(int MaxLength) : IManualPulseSurveyQuotaRule
{
    public List<string> Check(ManualPulseSurveyQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyQuotaApprovedGuardRule(double Limit) : IManualPulseSurveyQuotaRule
{
    public List<string> Check(ManualPulseSurveyQuota item)
    {
        if (item.Approved && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyQuotaRequiredCodeRule(string Message) : IManualPulseSurveyQuotaRule
{
    public List<string> Check(ManualPulseSurveyQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualPulseSurveyQuotaRegionLengthRule(int MaxLength) : IManualPulseSurveyQuotaRule
{
    public List<string> Check(ManualPulseSurveyQuota item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual pulse survey quota.</summary>
public sealed record ManualPulseSurveyQuotaValidator(List<IManualPulseSurveyQuotaRule> Rules)
{
    public List<string> Validate(ManualPulseSurveyQuota item)
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

    public void RequireValid(ManualPulseSurveyQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualPulseSurveyQuota> items)
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

/// <summary>Functions of manual pulse survey quota rules.</summary>
public static class ManualPulseSurveyQuotaRules
{
    public static ManualPulseSurveyQuotaValidator DefaultManualPulseSurveyQuotaValidator()
    {
        return new ManualPulseSurveyQuotaValidator(
            new List<IManualPulseSurveyQuotaRule>
            {
                new ManualPulseSurveyQuotaCodeLengthRule(3600),
                new ManualPulseSurveyQuotaApprovedGuardRule(8.7),
                new ManualPulseSurveyQuotaRequiredCodeRule("manual pulse survey quota needs an entry"),
                new ManualPulseSurveyQuotaRegionLengthRule(3600),
            });
    }
}
