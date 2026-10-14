namespace Acme.Workforce.Payrollops.Payslip.Group;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a payslip template must pass. Check returns its problems, or none.</summary>
public interface IPayslipTemplateRule
{
    List<string> Check(PayslipTemplate item);
}

public sealed record PayslipTemplateExpiresAtRangeRule(int Min, int Max) : IPayslipTemplateRule
{
    public List<string> Check(PayslipTemplate item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipTemplateAllowedGradeRule(List<string> Allowed) : IPayslipTemplateRule
{
    public List<string> Check(PayslipTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PayslipTemplateCodePrefixRule(string Prefix) : IPayslipTemplateRule
{
    public List<string> Check(PayslipTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PayslipTemplateTitleLengthRule(int MaxLength) : IPayslipTemplateRule
{
    public List<string> Check(PayslipTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a payslip template.</summary>
public sealed record PayslipTemplateValidator(List<IPayslipTemplateRule> Rules)
{
    public List<string> Validate(PayslipTemplate item)
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

    public void RequireValid(PayslipTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayslipTemplate> items)
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

/// <summary>Functions of payslip template rules.</summary>
public static class PayslipTemplateRules
{
    public static PayslipTemplateValidator DefaultPayslipTemplateValidator()
    {
        return new PayslipTemplateValidator(
            new List<IPayslipTemplateRule>
            {
                new PayslipTemplateExpiresAtRangeRule(32, 1000),
                new PayslipTemplateAllowedGradeRule(new List<string> { "rolling", "lenient" }),
                new PayslipTemplateCodePrefixRule("Q"),
                new PayslipTemplateTitleLengthRule(64),
            });
    }
}
