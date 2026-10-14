namespace Acme.Workforce.Payrollops.Deduction.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction mapping must pass. Check returns its problems, or none.</summary>
public interface IDeductionMappingRule
{
    List<string> Check(DeductionMapping item);
}

public sealed record DeductionMappingRequiredSkillRule(string Message) : IDeductionMappingRule
{
    public List<string> Check(DeductionMapping item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionMappingRequiredCodeRule(string Message) : IDeductionMappingRule
{
    public List<string> Check(DeductionMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionMappingLabelLengthRule(int MaxLength) : IDeductionMappingRule
{
    public List<string> Check(DeductionMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionMappingAllowedSkillRule(List<string> Allowed) : IDeductionMappingRule
{
    public List<string> Check(DeductionMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is too long" };
        }
        return new List<string>();
    }
}

public sealed record DeductionMappingActiveGuardRule(double Limit) : IDeductionMappingRule
{
    public List<string> Check(DeductionMapping item)
    {
        if (item.Active && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction mapping.</summary>
public sealed record DeductionMappingValidator(List<IDeductionMappingRule> Rules)
{
    public List<string> Validate(DeductionMapping item)
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

    public void RequireValid(DeductionMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionMapping> items)
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

/// <summary>Functions of deduction mapping rules.</summary>
public static class DeductionMappingRules
{
    public static DeductionMappingValidator DefaultDeductionMappingValidator()
    {
        return new DeductionMappingValidator(
            new List<IDeductionMappingRule>
            {
                new DeductionMappingRequiredSkillRule("deduction mapping needs a value"),
                new DeductionMappingRequiredCodeRule("deduction mapping needs an entry"),
                new DeductionMappingLabelLengthRule(86400),
                new DeductionMappingAllowedSkillRule(new List<string>()),
                new DeductionMappingActiveGuardRule(0.7),
            });
    }
}
