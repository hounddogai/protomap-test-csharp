namespace Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>A rule a deduction policy must pass. Check returns its problems, or none.</summary>
public interface IDeductionPolicyRule
{
    List<string> Check(DeductionPolicy item);
}

public sealed record DeductionPolicyRequiredUnitRule(string Message) : IDeductionPolicyRule
{
    public List<string> Check(DeductionPolicy item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionPolicyUnitLengthRule(int MaxLength) : IDeductionPolicyRule
{
    public List<string> Check(DeductionPolicy item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionPolicyRequiredLocationRule(string Message) : IDeductionPolicyRule
{
    public List<string> Check(DeductionPolicy item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionPolicyRatioRangeRule(double Min, double Max) : IDeductionPolicyRule
{
    public List<string> Check(DeductionPolicy item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction policy.</summary>
public sealed record DeductionPolicyValidator(List<IDeductionPolicyRule> Rules)
{
    public List<string> Validate(DeductionPolicy item)
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

    public void RequireValid(DeductionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionPolicy> items)
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

/// <summary>Functions of deduction policy rules.</summary>
public static class DeductionPolicyRules
{
    public static DeductionPolicyValidator DefaultDeductionPolicyValidator()
    {
        return new DeductionPolicyValidator(
            new List<IDeductionPolicyRule>
            {
                new DeductionPolicyRequiredUnitRule("deduction policy needs a value"),
                new DeductionPolicyUnitLengthRule(86400),
                new DeductionPolicyRequiredLocationRule("deduction policy needs an entry"),
                new DeductionPolicyRatioRangeRule(2.9, 2803.7),
            });
    }
}
