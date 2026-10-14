namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction batch must pass. Check returns its problems, or none.</summary>
public interface IDeductionBatchRule
{
    List<string> Check(DeductionBatch item);
}

public sealed record DeductionBatchLabelLengthRule(int MaxLength) : IDeductionBatchRule
{
    public List<string> Check(DeductionBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionBatchCodeLengthRule(int MaxLength) : IDeductionBatchRule
{
    public List<string> Check(DeductionBatch item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionBatchAllowedCodeRule(List<string> Allowed) : IDeductionBatchRule
{
    public List<string> Check(DeductionBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction batch.</summary>
public sealed record DeductionBatchValidator(List<IDeductionBatchRule> Rules)
{
    public List<string> Validate(DeductionBatch item)
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

    public void RequireValid(DeductionBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionBatch> items)
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

/// <summary>Functions of deduction batch rules.</summary>
public static class DeductionBatchRules
{
    public static DeductionBatchValidator DefaultDeductionBatchValidator()
    {
        return new DeductionBatchValidator(
            new List<IDeductionBatchRule>
            {
                new DeductionBatchLabelLengthRule(1000),
                new DeductionBatchCodeLengthRule(80),
                new DeductionBatchAllowedCodeRule(new List<string> { "primary", "global", "strict" }),
            });
    }
}
