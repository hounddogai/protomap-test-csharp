namespace Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>A rule a primary deduction budget must pass. Check returns its problems, or none.</summary>
public interface IPrimaryDeductionBudgetRule
{
    List<string> Check(PrimaryDeductionBudget item);
}

public sealed record PrimaryDeductionBudgetRequiredCountryRule(string Message) : IPrimaryDeductionBudgetRule
{
    public List<string> Check(PrimaryDeductionBudget item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeductionBudgetNameLengthRule(int MaxLength) : IPrimaryDeductionBudgetRule
{
    public List<string> Check(PrimaryDeductionBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeductionBudgetCodePrefixRule(string Prefix) : IPrimaryDeductionBudgetRule
{
    public List<string> Check(PrimaryDeductionBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeductionBudgetCreatedAtRangeRule(int Min, int Max) : IPrimaryDeductionBudgetRule
{
    public List<string> Check(PrimaryDeductionBudget item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryDeductionBudgetRequiredCodeRule(string Message) : IPrimaryDeductionBudgetRule
{
    public List<string> Check(PrimaryDeductionBudget item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary deduction budget.</summary>
public sealed record PrimaryDeductionBudgetValidator(List<IPrimaryDeductionBudgetRule> Rules)
{
    public List<string> Validate(PrimaryDeductionBudget item)
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

    public void RequireValid(PrimaryDeductionBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryDeductionBudget> items)
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

/// <summary>Functions of primary deduction budget rules.</summary>
public static class PrimaryDeductionBudgetRules
{
    public static PrimaryDeductionBudgetValidator DefaultPrimaryDeductionBudgetValidator()
    {
        return new PrimaryDeductionBudgetValidator(
            new List<IPrimaryDeductionBudgetRule>
            {
                new PrimaryDeductionBudgetRequiredCountryRule("primary deduction budget needs this field"),
                new PrimaryDeductionBudgetNameLengthRule(64),
                new PrimaryDeductionBudgetCodePrefixRule("REF"),
                new PrimaryDeductionBudgetCreatedAtRangeRule(32, 32),
                new PrimaryDeductionBudgetRequiredCodeRule("primary deduction budget needs an entry"),
            });
    }
}
