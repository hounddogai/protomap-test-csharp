namespace Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>A rule a deduction exception must pass. Check returns its problems, or none.</summary>
public interface IDeductionExceptionRule
{
    List<string> Check(DeductionException item);
}

public sealed record DeductionExceptionRequiredSegmentRule(string Message) : IDeductionExceptionRule
{
    public List<string> Check(DeductionException item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionExceptionCeilingRangeRule(double Min, double Max) : IDeductionExceptionRule
{
    public List<string> Check(DeductionException item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionExceptionNotesLengthRule(int MaxLength) : IDeductionExceptionRule
{
    public List<string> Check(DeductionException item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction exception.</summary>
public sealed record DeductionExceptionValidator(List<IDeductionExceptionRule> Rules)
{
    public List<string> Validate(DeductionException item)
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

    public void RequireValid(DeductionException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionException> items)
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

/// <summary>Functions of deduction exception rules.</summary>
public static class DeductionExceptionRules
{
    public static DeductionExceptionValidator DefaultDeductionExceptionValidator()
    {
        return new DeductionExceptionValidator(
            new List<IDeductionExceptionRule>
            {
                new DeductionExceptionRequiredSegmentRule("deduction exception needs an entry"),
                new DeductionExceptionCeilingRangeRule(7.2, 628.7),
                new DeductionExceptionNotesLengthRule(32),
            });
    }
}
