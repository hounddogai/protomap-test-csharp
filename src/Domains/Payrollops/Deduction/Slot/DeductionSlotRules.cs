namespace Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>A rule a deduction slot must pass. Check returns its problems, or none.</summary>
public interface IDeductionSlotRule
{
    List<string> Check(DeductionSlot item);
}

public sealed record DeductionSlotChecksumLengthRule(int MaxLength) : IDeductionSlotRule
{
    public List<string> Check(DeductionSlot item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionSlotLabelLengthRule(int MaxLength) : IDeductionSlotRule
{
    public List<string> Check(DeductionSlot item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionSlotPriorityRangeRule(int Min, int Max) : IDeductionSlotRule
{
    public List<string> Check(DeductionSlot item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a deduction slot.</summary>
public sealed record DeductionSlotValidator(List<IDeductionSlotRule> Rules)
{
    public List<string> Validate(DeductionSlot item)
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

    public void RequireValid(DeductionSlot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionSlot> items)
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

/// <summary>Functions of deduction slot rules.</summary>
public static class DeductionSlotRules
{
    public static DeductionSlotValidator DefaultDeductionSlotValidator()
    {
        return new DeductionSlotValidator(
            new List<IDeductionSlotRule>
            {
                new DeductionSlotChecksumLengthRule(86400),
                new DeductionSlotLabelLengthRule(86400),
                new DeductionSlotPriorityRangeRule(32, 1000),
            });
    }
}
