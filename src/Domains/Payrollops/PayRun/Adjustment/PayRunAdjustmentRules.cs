namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>A rule a pay run adjustment must pass. Check returns its problems, or none.</summary>
public interface IPayRunAdjustmentRule
{
    List<string> Check(PayRunAdjustment item);
}

public sealed record PayRunAdjustmentScoreRangeRule(double Min, double Max) : IPayRunAdjustmentRule
{
    public List<string> Check(PayRunAdjustment item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAdjustmentRequiredCodeRule(string Message) : IPayRunAdjustmentRule
{
    public List<string> Check(PayRunAdjustment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunAdjustmentRequiredEmployeeIdRule(string Message) : IPayRunAdjustmentRule
{
    public List<string> Check(PayRunAdjustment item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PayRunAdjustmentCodePrefixRule(string Prefix) : IPayRunAdjustmentRule
{
    public List<string> Check(PayRunAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunAdjustmentLocationLengthRule(int MaxLength) : IPayRunAdjustmentRule
{
    public List<string> Check(PayRunAdjustment item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run adjustment.</summary>
public sealed record PayRunAdjustmentValidator(List<IPayRunAdjustmentRule> Rules)
{
    public List<string> Validate(PayRunAdjustment item)
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

    public void RequireValid(PayRunAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunAdjustment> items)
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

/// <summary>Functions of pay run adjustment rules.</summary>
public static class PayRunAdjustmentRules
{
    public static PayRunAdjustmentValidator DefaultPayRunAdjustmentValidator()
    {
        return new PayRunAdjustmentValidator(
            new List<IPayRunAdjustmentRule>
            {
                new PayRunAdjustmentScoreRangeRule(3.2, 618.5),
                new PayRunAdjustmentRequiredCodeRule("pay run adjustment needs a value"),
                new PayRunAdjustmentRequiredEmployeeIdRule("pay run adjustment needs this field"),
                new PayRunAdjustmentCodePrefixRule("REF"),
                new PayRunAdjustmentLocationLengthRule(3600),
            });
    }
}
