namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict training due quota must pass. Check returns its problems, or none.</summary>
public interface IStrictTrainingDueQuotaRule
{
    List<string> Check(StrictTrainingDueQuota item);
}

public sealed record StrictTrainingDueQuotaRequiredLabelRule(string Message) : IStrictTrainingDueQuotaRule
{
    public List<string> Check(StrictTrainingDueQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictTrainingDueQuotaSummaryLengthRule(int MaxLength) : IStrictTrainingDueQuotaRule
{
    public List<string> Check(StrictTrainingDueQuota item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictTrainingDueQuotaAllowedSummaryRule(List<string> Allowed) : IStrictTrainingDueQuotaRule
{
    public List<string> Check(StrictTrainingDueQuota item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict training due quota.</summary>
public sealed record StrictTrainingDueQuotaValidator(List<IStrictTrainingDueQuotaRule> Rules)
{
    public List<string> Validate(StrictTrainingDueQuota item)
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

    public void RequireValid(StrictTrainingDueQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictTrainingDueQuota> items)
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

/// <summary>Functions of strict training due quota rules.</summary>
public static class StrictTrainingDueQuotaRules
{
    public static StrictTrainingDueQuotaValidator DefaultStrictTrainingDueQuotaValidator()
    {
        return new StrictTrainingDueQuotaValidator(
            new List<IStrictTrainingDueQuotaRule>
            {
                new StrictTrainingDueQuotaRequiredLabelRule("strict training due quota needs this field"),
                new StrictTrainingDueQuotaSummaryLengthRule(3600),
                new StrictTrainingDueQuotaAllowedSummaryRule(new List<string> { "primary", "legacy", "legacy" }),
            });
    }
}
