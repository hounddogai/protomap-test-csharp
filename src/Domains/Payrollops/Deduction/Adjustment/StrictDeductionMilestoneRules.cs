namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict deduction milestone must pass. Check returns its problems, or none.</summary>
public interface IStrictDeductionMilestoneRule
{
    List<string> Check(StrictDeductionMilestone item);
}

public sealed record StrictDeductionMilestoneCodeLengthRule(int MaxLength) : IStrictDeductionMilestoneRule
{
    public List<string> Check(StrictDeductionMilestone item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionMilestoneAllowedNameRule(List<string> Allowed) : IStrictDeductionMilestoneRule
{
    public List<string> Check(StrictDeductionMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionMilestoneCodePrefixRule(string Prefix) : IStrictDeductionMilestoneRule
{
    public List<string> Check(StrictDeductionMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictDeductionMilestoneManagerIdLengthRule(int MaxLength) : IStrictDeductionMilestoneRule
{
    public List<string> Check(StrictDeductionMilestone item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict deduction milestone.</summary>
public sealed record StrictDeductionMilestoneValidator(List<IStrictDeductionMilestoneRule> Rules)
{
    public List<string> Validate(StrictDeductionMilestone item)
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

    public void RequireValid(StrictDeductionMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictDeductionMilestone> items)
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

/// <summary>Functions of strict deduction milestone rules.</summary>
public static class StrictDeductionMilestoneRules
{
    public static StrictDeductionMilestoneValidator DefaultStrictDeductionMilestoneValidator()
    {
        return new StrictDeductionMilestoneValidator(
            new List<IStrictDeductionMilestoneRule>
            {
                new StrictDeductionMilestoneCodeLengthRule(120),
                new StrictDeductionMilestoneAllowedNameRule(new List<string> { "weighted", "regional", "regional" }),
                new StrictDeductionMilestoneCodePrefixRule("X"),
                new StrictDeductionMilestoneManagerIdLengthRule(16),
            });
    }
}
