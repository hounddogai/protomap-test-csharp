namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>A rule a batched payslip claim must pass. Check returns its problems, or none.</summary>
public interface IBatchedPayslipClaimRule
{
    List<string> Check(BatchedPayslipClaim item);
}

public sealed record BatchedPayslipClaimRequiredExternalRefRule(string Message) : IBatchedPayslipClaimRule
{
    public List<string> Check(BatchedPayslipClaim item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayslipClaimCodeLengthRule(int MaxLength) : IBatchedPayslipClaimRule
{
    public List<string> Check(BatchedPayslipClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedPayslipClaimRequiredNameRule(string Message) : IBatchedPayslipClaimRule
{
    public List<string> Check(BatchedPayslipClaim item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched payslip claim.</summary>
public sealed record BatchedPayslipClaimValidator(List<IBatchedPayslipClaimRule> Rules)
{
    public List<string> Validate(BatchedPayslipClaim item)
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

    public void RequireValid(BatchedPayslipClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedPayslipClaim> items)
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

/// <summary>Functions of batched payslip claim rules.</summary>
public static class BatchedPayslipClaimRules
{
    public static BatchedPayslipClaimValidator DefaultBatchedPayslipClaimValidator()
    {
        return new BatchedPayslipClaimValidator(
            new List<IBatchedPayslipClaimRule>
            {
                new BatchedPayslipClaimRequiredExternalRefRule("batched payslip claim needs a value"),
                new BatchedPayslipClaimCodeLengthRule(255),
                new BatchedPayslipClaimRequiredNameRule("batched payslip claim needs a value"),
            });
    }
}
