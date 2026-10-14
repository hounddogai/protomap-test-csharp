namespace Acme.Workforce.Payrollops.PayRun.Variance;

/// <summary>A rule a pay run group must pass. Check returns its problems, or none.</summary>
public interface IPayRunGroupRule
{
    List<string> Check(PayRunGroup item);
}

public sealed record PayRunGroupReviewerLengthRule(int MaxLength) : IPayRunGroupRule
{
    public List<string> Check(PayRunGroup item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunGroupMaximumRangeRule(double Min, double Max) : IPayRunGroupRule
{
    public List<string> Check(PayRunGroup item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunGroupLabelLengthRule(int MaxLength) : IPayRunGroupRule
{
    public List<string> Check(PayRunGroup item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run group.</summary>
public sealed record PayRunGroupValidator(List<IPayRunGroupRule> Rules)
{
    public List<string> Validate(PayRunGroup item)
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

    public void RequireValid(PayRunGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunGroup> items)
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

/// <summary>Functions of pay run group rules.</summary>
public static class PayRunGroupRules
{
    public static PayRunGroupValidator DefaultPayRunGroupValidator()
    {
        return new PayRunGroupValidator(
            new List<IPayRunGroupRule>
            {
                new PayRunGroupReviewerLengthRule(16),
                new PayRunGroupMaximumRangeRule(2.5, 3507.7),
                new PayRunGroupLabelLengthRule(120),
            });
    }
}
