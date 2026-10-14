namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>A rule a batched tax form assignment must pass. Check returns its problems, or none.</summary>
public interface IBatchedTaxFormAssignmentRule
{
    List<string> Check(BatchedTaxFormAssignment item);
}

public sealed record BatchedTaxFormAssignmentCodeLengthRule(int MaxLength) : IBatchedTaxFormAssignmentRule
{
    public List<string> Check(BatchedTaxFormAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedTaxFormAssignmentRequiredPhoneRule(string Message) : IBatchedTaxFormAssignmentRule
{
    public List<string> Check(BatchedTaxFormAssignment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTaxFormAssignmentRequiredCodeRule(string Message) : IBatchedTaxFormAssignmentRule
{
    public List<string> Check(BatchedTaxFormAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedTaxFormAssignmentMaximumRangeRule(double Min, double Max) : IBatchedTaxFormAssignmentRule
{
    public List<string> Check(BatchedTaxFormAssignment item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched tax form assignment.</summary>
public sealed record BatchedTaxFormAssignmentValidator(List<IBatchedTaxFormAssignmentRule> Rules)
{
    public List<string> Validate(BatchedTaxFormAssignment item)
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

    public void RequireValid(BatchedTaxFormAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedTaxFormAssignment> items)
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

/// <summary>Functions of batched tax form assignment rules.</summary>
public static class BatchedTaxFormAssignmentRules
{
    public static BatchedTaxFormAssignmentValidator DefaultBatchedTaxFormAssignmentValidator()
    {
        return new BatchedTaxFormAssignmentValidator(
            new List<IBatchedTaxFormAssignmentRule>
            {
                new BatchedTaxFormAssignmentCodeLengthRule(86400),
                new BatchedTaxFormAssignmentRequiredPhoneRule("batched tax form assignment needs an entry"),
                new BatchedTaxFormAssignmentRequiredCodeRule("batched tax form assignment needs a value"),
                new BatchedTaxFormAssignmentMaximumRangeRule(5.3, 3462.5),
            });
    }
}
