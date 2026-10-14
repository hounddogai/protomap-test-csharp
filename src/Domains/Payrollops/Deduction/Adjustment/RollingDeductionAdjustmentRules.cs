namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling deduction adjustment must pass. Check returns its problems, or none.</summary>
public interface IRollingDeductionAdjustmentRule
{
    List<string> Check(RollingDeductionAdjustment item);
}

public sealed record RollingDeductionAdjustmentRequiredNameRule(string Message) : IRollingDeductionAdjustmentRule
{
    public List<string> Check(RollingDeductionAdjustment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionAdjustmentUnitLengthRule(int MaxLength) : IRollingDeductionAdjustmentRule
{
    public List<string> Check(RollingDeductionAdjustment item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionAdjustmentRequiredGradeRule(string Message) : IRollingDeductionAdjustmentRule
{
    public List<string> Check(RollingDeductionAdjustment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionAdjustmentRevisionRangeRule(int Min, int Max) : IRollingDeductionAdjustmentRule
{
    public List<string> Check(RollingDeductionAdjustment item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionAdjustmentCodeLengthRule(int MaxLength) : IRollingDeductionAdjustmentRule
{
    public List<string> Check(RollingDeductionAdjustment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingDeductionAdjustmentAllowedGradeRule(List<string> Allowed) : IRollingDeductionAdjustmentRule
{
    public List<string> Check(RollingDeductionAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is missing" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling deduction adjustment.</summary>
public sealed record RollingDeductionAdjustmentValidator(List<IRollingDeductionAdjustmentRule> Rules)
{
    public List<string> Validate(RollingDeductionAdjustment item)
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

    public void RequireValid(RollingDeductionAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingDeductionAdjustment> items)
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

/// <summary>Functions of rolling deduction adjustment rules.</summary>
public static class RollingDeductionAdjustmentRules
{
    public static RollingDeductionAdjustmentValidator DefaultRollingDeductionAdjustmentValidator()
    {
        return new RollingDeductionAdjustmentValidator(
            new List<IRollingDeductionAdjustmentRule>
            {
                new RollingDeductionAdjustmentRequiredNameRule("rolling deduction adjustment needs an entry"),
                new RollingDeductionAdjustmentUnitLengthRule(1000),
                new RollingDeductionAdjustmentRequiredGradeRule("rolling deduction adjustment needs an entry"),
                new RollingDeductionAdjustmentRevisionRangeRule(64, 1000),
                new RollingDeductionAdjustmentCodeLengthRule(16),
                new RollingDeductionAdjustmentAllowedGradeRule(new List<string> { "lenient", "pending" }),
            });
    }
}
