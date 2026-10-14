namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>A rule a deduction allocation must pass. Check returns its problems, or none.</summary>
public interface IDeductionAllocationRule
{
    List<string> Check(DeductionAllocation item);
}

public sealed record DeductionAllocationRequiredSalaryBandRule(string Message) : IDeductionAllocationRule
{
    public List<string> Check(DeductionAllocation item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionAllocationCodeLengthRule(int MaxLength) : IDeductionAllocationRule
{
    public List<string> Check(DeductionAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionAllocationRequiredCodeRule(string Message) : IDeductionAllocationRule
{
    public List<string> Check(DeductionAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionAllocationRequiredTitleRule(string Message) : IDeductionAllocationRule
{
    public List<string> Check(DeductionAllocation item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionAllocationCodePrefixRule(string Prefix) : IDeductionAllocationRule
{
    public List<string> Check(DeductionAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction allocation.</summary>
public sealed record DeductionAllocationValidator(List<IDeductionAllocationRule> Rules)
{
    public List<string> Validate(DeductionAllocation item)
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

    public void RequireValid(DeductionAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionAllocation> items)
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

/// <summary>Functions of deduction allocation rules.</summary>
public static class DeductionAllocationRules
{
    public static DeductionAllocationValidator DefaultDeductionAllocationValidator()
    {
        return new DeductionAllocationValidator(
            new List<IDeductionAllocationRule>
            {
                new DeductionAllocationRequiredSalaryBandRule("deduction allocation needs an entry"),
                new DeductionAllocationCodeLengthRule(3600),
                new DeductionAllocationRequiredCodeRule("deduction allocation needs a value"),
                new DeductionAllocationRequiredTitleRule("deduction allocation needs an entry"),
                new DeductionAllocationCodePrefixRule("Q"),
            });
    }
}
