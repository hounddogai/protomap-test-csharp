namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary deduction template must pass. Check returns its problems, or none.</summary>
public interface IPrimaryDeductionTemplateRule
{
    List<string> Check(PrimaryDeductionTemplate item);
}

public sealed record PrimaryDeductionTemplateCurrencyLengthRule(int MaxLength) : IPrimaryDeductionTemplateRule
{
    public List<string> Check(PrimaryDeductionTemplate item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeductionTemplateScoreRangeRule(double Min, double Max) : IPrimaryDeductionTemplateRule
{
    public List<string> Check(PrimaryDeductionTemplate item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryDeductionTemplateAllowedPhoneRule(List<string> Allowed) : IPrimaryDeductionTemplateRule
{
    public List<string> Check(PrimaryDeductionTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is above maximum" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary deduction template.</summary>
public sealed record PrimaryDeductionTemplateValidator(List<IPrimaryDeductionTemplateRule> Rules)
{
    public List<string> Validate(PrimaryDeductionTemplate item)
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

    public void RequireValid(PrimaryDeductionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryDeductionTemplate> items)
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

/// <summary>Functions of primary deduction template rules.</summary>
public static class PrimaryDeductionTemplateRules
{
    public static PrimaryDeductionTemplateValidator DefaultPrimaryDeductionTemplateValidator()
    {
        return new PrimaryDeductionTemplateValidator(
            new List<IPrimaryDeductionTemplateRule>
            {
                new PrimaryDeductionTemplateCurrencyLengthRule(86400),
                new PrimaryDeductionTemplateScoreRangeRule(2.3, 4271.2),
                new PrimaryDeductionTemplateAllowedPhoneRule(new List<string> { "fallback" }),
            });
    }
}
