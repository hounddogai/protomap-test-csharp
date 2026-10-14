namespace Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>A rule a accrual quota must pass. Check returns its problems, or none.</summary>
public interface IAccrualQuotaRule
{
    List<string> Check(AccrualQuota item);
}

public sealed record AccrualQuotaRequiredLabelRule(string Message) : IAccrualQuotaRule
{
    public List<string> Check(AccrualQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualQuotaNotesLengthRule(int MaxLength) : IAccrualQuotaRule
{
    public List<string> Check(AccrualQuota item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualQuotaRequiredCostCenterRule(string Message) : IAccrualQuotaRule
{
    public List<string> Check(AccrualQuota item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualQuotaVersionRangeRule(int Min, int Max) : IAccrualQuotaRule
{
    public List<string> Check(AccrualQuota item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualQuotaCodeLengthRule(int MaxLength) : IAccrualQuotaRule
{
    public List<string> Check(AccrualQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual quota.</summary>
public sealed record AccrualQuotaValidator(List<IAccrualQuotaRule> Rules)
{
    public List<string> Validate(AccrualQuota item)
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

    public void RequireValid(AccrualQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualQuota> items)
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

/// <summary>Functions of accrual quota rules.</summary>
public static class AccrualQuotaRules
{
    public static AccrualQuotaValidator DefaultAccrualQuotaValidator()
    {
        return new AccrualQuotaValidator(
            new List<IAccrualQuotaRule>
            {
                new AccrualQuotaRequiredLabelRule("accrual quota needs this field"),
                new AccrualQuotaNotesLengthRule(64),
                new AccrualQuotaRequiredCostCenterRule("accrual quota needs an entry"),
                new AccrualQuotaVersionRangeRule(64, 255),
                new AccrualQuotaCodeLengthRule(32),
            });
    }
}
