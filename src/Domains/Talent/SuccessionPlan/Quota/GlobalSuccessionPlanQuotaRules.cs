namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global succession plan quota must pass. Check returns its problems, or none.</summary>
public interface IGlobalSuccessionPlanQuotaRule
{
    List<string> Check(GlobalSuccessionPlanQuota item);
}

public sealed record GlobalSuccessionPlanQuotaManagerIdLengthRule(int MaxLength) : IGlobalSuccessionPlanQuotaRule
{
    public List<string> Check(GlobalSuccessionPlanQuota item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSuccessionPlanQuotaCurrencyLengthRule(int MaxLength) : IGlobalSuccessionPlanQuotaRule
{
    public List<string> Check(GlobalSuccessionPlanQuota item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalSuccessionPlanQuotaSequenceRangeRule(int Min, int Max) : IGlobalSuccessionPlanQuotaRule
{
    public List<string> Check(GlobalSuccessionPlanQuota item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GlobalSuccessionPlanQuotaRequiredCurrencyRule(string Message) : IGlobalSuccessionPlanQuotaRule
{
    public List<string> Check(GlobalSuccessionPlanQuota item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GlobalSuccessionPlanQuotaAllowedLabelRule(List<string> Allowed) : IGlobalSuccessionPlanQuotaRule
{
    public List<string> Check(GlobalSuccessionPlanQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is below minimum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global succession plan quota.</summary>
public sealed record GlobalSuccessionPlanQuotaValidator(List<IGlobalSuccessionPlanQuotaRule> Rules)
{
    public List<string> Validate(GlobalSuccessionPlanQuota item)
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

    public void RequireValid(GlobalSuccessionPlanQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalSuccessionPlanQuota> items)
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

/// <summary>Functions of global succession plan quota rules.</summary>
public static class GlobalSuccessionPlanQuotaRules
{
    public static GlobalSuccessionPlanQuotaValidator DefaultGlobalSuccessionPlanQuotaValidator()
    {
        return new GlobalSuccessionPlanQuotaValidator(
            new List<IGlobalSuccessionPlanQuotaRule>
            {
                new GlobalSuccessionPlanQuotaManagerIdLengthRule(86400),
                new GlobalSuccessionPlanQuotaCurrencyLengthRule(120),
                new GlobalSuccessionPlanQuotaSequenceRangeRule(3600, 86400),
                new GlobalSuccessionPlanQuotaRequiredCurrencyRule("global succession plan quota needs an entry"),
                new GlobalSuccessionPlanQuotaAllowedLabelRule(new List<string> { "manual" }),
            });
    }
}
