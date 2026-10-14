namespace Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>A rule a deduction bundle must pass. Check returns its problems, or none.</summary>
public interface IDeductionBundleRule
{
    List<string> Check(DeductionBundle item);
}

public sealed record DeductionBundleRequiredPhoneRule(string Message) : IDeductionBundleRule
{
    public List<string> Check(DeductionBundle item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionBundleRequiredNameRule(string Message) : IDeductionBundleRule
{
    public List<string> Check(DeductionBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionBundleNameLengthRule(int MaxLength) : IDeductionBundleRule
{
    public List<string> Check(DeductionBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction bundle.</summary>
public sealed record DeductionBundleValidator(List<IDeductionBundleRule> Rules)
{
    public List<string> Validate(DeductionBundle item)
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

    public void RequireValid(DeductionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionBundle> items)
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

/// <summary>Functions of deduction bundle rules.</summary>
public static class DeductionBundleRules
{
    public static DeductionBundleValidator DefaultDeductionBundleValidator()
    {
        return new DeductionBundleValidator(
            new List<IDeductionBundleRule>
            {
                new DeductionBundleRequiredPhoneRule("deduction bundle needs a value"),
                new DeductionBundleRequiredNameRule("deduction bundle needs this field"),
                new DeductionBundleNameLengthRule(32),
            });
    }
}
