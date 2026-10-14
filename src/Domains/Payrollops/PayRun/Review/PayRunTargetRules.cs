namespace Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>A rule a pay run target must pass. Check returns its problems, or none.</summary>
public interface IPayRunTargetRule
{
    List<string> Check(PayRunTarget item);
}

public sealed record PayRunTargetPhoneLengthRule(int MaxLength) : IPayRunTargetRule
{
    public List<string> Check(PayRunTarget item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunTargetCodeLengthRule(int MaxLength) : IPayRunTargetRule
{
    public List<string> Check(PayRunTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunTargetSequenceRangeRule(int Min, int Max) : IPayRunTargetRule
{
    public List<string> Check(PayRunTarget item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PayRunTargetDepartmentLengthRule(int MaxLength) : IPayRunTargetRule
{
    public List<string> Check(PayRunTarget item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayRunTargetRequiredLabelRule(string Message) : IPayRunTargetRule
{
    public List<string> Check(PayRunTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay run target.</summary>
public sealed record PayRunTargetValidator(List<IPayRunTargetRule> Rules)
{
    public List<string> Validate(PayRunTarget item)
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

    public void RequireValid(PayRunTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayRunTarget> items)
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

/// <summary>Functions of pay run target rules.</summary>
public static class PayRunTargetRules
{
    public static PayRunTargetValidator DefaultPayRunTargetValidator()
    {
        return new PayRunTargetValidator(
            new List<IPayRunTargetRule>
            {
                new PayRunTargetPhoneLengthRule(3600),
                new PayRunTargetCodeLengthRule(255),
                new PayRunTargetSequenceRangeRule(16, 120),
                new PayRunTargetDepartmentLengthRule(64),
                new PayRunTargetRequiredLabelRule("pay run target needs a value"),
            });
    }
}
