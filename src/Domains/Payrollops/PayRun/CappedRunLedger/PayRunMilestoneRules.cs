namespace Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>A rule a pay run milestone must pass. Check returns its problems, or none.</summary>
public interface IPayRunMilestoneRule
{
    List<string> Check(PayRunMilestone item);
}

public sealed record PayRunMilestoneLabelLengthRule(int MaxLength) : IPayRunMilestoneRule
{
    public List<string> Check(PayRunMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMilestoneCodeLengthRule(int MaxLength) : IPayRunMilestoneRule
{
    public List<string> Check(PayRunMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMilestoneNameLengthRule(int MaxLength) : IPayRunMilestoneRule
{
    public List<string> Check(PayRunMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMilestoneDiscountRangeRule(double Min, double Max) : IPayRunMilestoneRule
{
    public List<string> Check(PayRunMilestone item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunMilestoneCodePrefixRule(string Prefix) : IPayRunMilestoneRule
{
    public List<string> Check(PayRunMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run milestone.</summary>
public sealed record PayRunMilestoneValidator(List<IPayRunMilestoneRule> Rules)
{
    public List<string> Validate(PayRunMilestone item)
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

    public void RequireValid(PayRunMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunMilestone> items)
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

/// <summary>Functions of pay run milestone rules.</summary>
public static class PayRunMilestoneRules
{
    public static PayRunMilestoneValidator DefaultPayRunMilestoneValidator()
    {
        return new PayRunMilestoneValidator(
            new List<IPayRunMilestoneRule>
            {
                new PayRunMilestoneLabelLengthRule(80),
                new PayRunMilestoneCodeLengthRule(1000),
                new PayRunMilestoneNameLengthRule(16),
                new PayRunMilestoneDiscountRangeRule(8.4, 4168.6),
                new PayRunMilestoneCodePrefixRule("ACM"),
            });
    }
}
