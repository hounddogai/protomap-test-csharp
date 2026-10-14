namespace Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>A rule a deduction request must pass. Check returns its problems, or none.</summary>
public interface IDeductionRequestRule
{
    List<string> Check(DeductionRequest item);
}

public sealed record DeductionRequestRequiredCountryRule(string Message) : IDeductionRequestRule
{
    public List<string> Check(DeductionRequest item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionRequestCountryLengthRule(int MaxLength) : IDeductionRequestRule
{
    public List<string> Check(DeductionRequest item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRequestMinimumRangeRule(double Min, double Max) : IDeductionRequestRule
{
    public List<string> Check(DeductionRequest item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction request.</summary>
public sealed record DeductionRequestValidator(List<IDeductionRequestRule> Rules)
{
    public List<string> Validate(DeductionRequest item)
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

    public void RequireValid(DeductionRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionRequest> items)
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

/// <summary>Functions of deduction request rules.</summary>
public static class DeductionRequestRules
{
    public static DeductionRequestValidator DefaultDeductionRequestValidator()
    {
        return new DeductionRequestValidator(
            new List<IDeductionRequestRule>
            {
                new DeductionRequestRequiredCountryRule("deduction request needs this field"),
                new DeductionRequestCountryLengthRule(16),
                new DeductionRequestMinimumRangeRule(2.3, 3187.8),
            });
    }
}
