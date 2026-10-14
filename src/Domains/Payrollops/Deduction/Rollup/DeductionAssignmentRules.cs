namespace Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>A rule a deduction assignment must pass. Check returns its problems, or none.</summary>
public interface IDeductionAssignmentRule
{
    List<string> Check(DeductionAssignment item);
}

public sealed record DeductionAssignmentCreatedAtRangeRule(int Min, int Max) : IDeductionAssignmentRule
{
    public List<string> Check(DeductionAssignment item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeductionAssignmentChecksumLengthRule(int MaxLength) : IDeductionAssignmentRule
{
    public List<string> Check(DeductionAssignment item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionAssignmentCodePrefixRule(string Prefix) : IDeductionAssignmentRule
{
    public List<string> Check(DeductionAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionAssignmentRequiredCodeRule(string Message) : IDeductionAssignmentRule
{
    public List<string> Check(DeductionAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction assignment.</summary>
public sealed record DeductionAssignmentValidator(List<IDeductionAssignmentRule> Rules)
{
    public List<string> Validate(DeductionAssignment item)
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

    public void RequireValid(DeductionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionAssignment> items)
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

/// <summary>Functions of deduction assignment rules.</summary>
public static class DeductionAssignmentRules
{
    public static DeductionAssignmentValidator DefaultDeductionAssignmentValidator()
    {
        return new DeductionAssignmentValidator(
            new List<IDeductionAssignmentRule>
            {
                new DeductionAssignmentCreatedAtRangeRule(255, 3600),
                new DeductionAssignmentChecksumLengthRule(32),
                new DeductionAssignmentCodePrefixRule("REF"),
                new DeductionAssignmentRequiredCodeRule("deduction assignment needs an entry"),
            });
    }
}
