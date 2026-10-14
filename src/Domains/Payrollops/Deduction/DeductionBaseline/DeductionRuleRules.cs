namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a deduction rule must pass. Check returns its problems, or none.</summary>
public interface IDeductionRuleRule
{
    List<string> Check(DeductionRule item);
}

public sealed record DeductionRuleCodeLengthRule(int MaxLength) : IDeductionRuleRule
{
    public List<string> Check(DeductionRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRuleCodePrefixRule(string Prefix) : IDeductionRuleRule
{
    public List<string> Check(DeductionRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRuleRequiredNameRule(string Message) : IDeductionRuleRule
{
    public List<string> Check(DeductionRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionRuleAllowedCodeRule(List<string> Allowed) : IDeductionRuleRule
{
    public List<string> Check(DeductionRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRuleNameLengthRule(int MaxLength) : IDeductionRuleRule
{
    public List<string> Check(DeductionRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction rule.</summary>
public sealed record DeductionRuleValidator(List<IDeductionRuleRule> Rules)
{
    public List<string> Validate(DeductionRule item)
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

    public void RequireValid(DeductionRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionRule> items)
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

/// <summary>Functions of deduction rule rules.</summary>
public static class DeductionRuleRules
{
    public static DeductionRuleValidator DefaultDeductionRuleValidator()
    {
        return new DeductionRuleValidator(
            new List<IDeductionRuleRule>
            {
                new DeductionRuleCodeLengthRule(32),
                new DeductionRuleCodePrefixRule("REF"),
                new DeductionRuleRequiredNameRule("deduction rule needs this field"),
                new DeductionRuleAllowedCodeRule(new List<string> { "primary", "rolling" }),
                new DeductionRuleNameLengthRule(64),
            });
    }
}
