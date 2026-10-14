namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a capped deduction assignment must pass. Check returns its problems, or none.</summary>
public interface ICappedDeductionAssignmentRule
{
    List<string> Check(CappedDeductionAssignment item);
}

public sealed record CappedDeductionAssignmentRequiredCodeRule(string Message) : ICappedDeductionAssignmentRule
{
    public List<string> Check(CappedDeductionAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionAssignmentCodeLengthRule(int MaxLength) : ICappedDeductionAssignmentRule
{
    public List<string> Check(CappedDeductionAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionAssignmentActiveGuardRule(double Limit) : ICappedDeductionAssignmentRule
{
    public List<string> Check(CappedDeductionAssignment item)
    {
        if (item.Active && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionAssignmentAllowedCodeRule(List<string> Allowed) : ICappedDeductionAssignmentRule
{
    public List<string> Check(CappedDeductionAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionAssignmentLabelLengthRule(int MaxLength) : ICappedDeductionAssignmentRule
{
    public List<string> Check(CappedDeductionAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedDeductionAssignmentAttemptsRangeRule(int Min, int Max) : ICappedDeductionAssignmentRule
{
    public List<string> Check(CappedDeductionAssignment item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped deduction assignment.</summary>
public sealed record CappedDeductionAssignmentValidator(List<ICappedDeductionAssignmentRule> Rules)
{
    public List<string> Validate(CappedDeductionAssignment item)
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

    public void RequireValid(CappedDeductionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedDeductionAssignment> items)
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

/// <summary>Functions of capped deduction assignment rules.</summary>
public static class CappedDeductionAssignmentRules
{
    public static CappedDeductionAssignmentValidator DefaultCappedDeductionAssignmentValidator()
    {
        return new CappedDeductionAssignmentValidator(
            new List<ICappedDeductionAssignmentRule>
            {
                new CappedDeductionAssignmentRequiredCodeRule("capped deduction assignment needs an entry"),
                new CappedDeductionAssignmentCodeLengthRule(16),
                new CappedDeductionAssignmentActiveGuardRule(7.9),
                new CappedDeductionAssignmentAllowedCodeRule(new List<string> { "batched" }),
                new CappedDeductionAssignmentLabelLengthRule(80),
                new CappedDeductionAssignmentAttemptsRangeRule(16, 64),
            });
    }
}
