namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>A rule a capped payslip batch must pass. Check returns its problems, or none.</summary>
public interface ICappedPayslipBatchRule
{
    List<string> Check(CappedPayslipBatch item);
}

public sealed record CappedPayslipBatchTagsLimitRule(int MaxCount, List<string> Banned) : ICappedPayslipBatchRule
{
    public List<string> Check(CappedPayslipBatch item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record CappedPayslipBatchRequiredContactEmailRule(string Message) : ICappedPayslipBatchRule
{
    public List<string> Check(CappedPayslipBatch item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipBatchRequiredExternalRefRule(string Message) : ICappedPayslipBatchRule
{
    public List<string> Check(CappedPayslipBatch item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipBatchRequiredLabelRule(string Message) : ICappedPayslipBatchRule
{
    public List<string> Check(CappedPayslipBatch item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedPayslipBatchExternalRefLengthRule(int MaxLength) : ICappedPayslipBatchRule
{
    public List<string> Check(CappedPayslipBatch item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped payslip batch.</summary>
public sealed record CappedPayslipBatchValidator(List<ICappedPayslipBatchRule> Rules)
{
    public List<string> Validate(CappedPayslipBatch item)
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

    public void RequireValid(CappedPayslipBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedPayslipBatch> items)
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

/// <summary>Functions of capped payslip batch rules.</summary>
public static class CappedPayslipBatchRules
{
    public static CappedPayslipBatchValidator DefaultCappedPayslipBatchValidator()
    {
        return new CappedPayslipBatchValidator(
            new List<ICappedPayslipBatchRule>
            {
                new CappedPayslipBatchTagsLimitRule(255, new List<string> { "legacy", "seasonal", "scheduled" }),
                new CappedPayslipBatchRequiredContactEmailRule("capped payslip batch needs an entry"),
                new CappedPayslipBatchRequiredExternalRefRule("capped payslip batch needs an entry"),
                new CappedPayslipBatchRequiredLabelRule("capped payslip batch needs an entry"),
                new CappedPayslipBatchExternalRefLengthRule(1000),
            });
    }
}
