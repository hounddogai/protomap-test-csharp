namespace Acme.Workforce.Payrollops.Garnishment.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient garnishment budget must pass. Check returns its problems, or none.</summary>
public interface ILenientGarnishmentBudgetRule
{
    List<string> Check(LenientGarnishmentBudget item);
}

public sealed record LenientGarnishmentBudgetMaximumRangeRule(double Min, double Max) : ILenientGarnishmentBudgetRule
{
    public List<string> Check(LenientGarnishmentBudget item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientGarnishmentBudgetCodePrefixRule(string Prefix) : ILenientGarnishmentBudgetRule
{
    public List<string> Check(LenientGarnishmentBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentBudgetChecksumLengthRule(int MaxLength) : ILenientGarnishmentBudgetRule
{
    public List<string> Check(LenientGarnishmentBudget item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentBudgetRequiredChecksumRule(string Message) : ILenientGarnishmentBudgetRule
{
    public List<string> Check(LenientGarnishmentBudget item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentBudgetAllowedChecksumRule(List<string> Allowed) : ILenientGarnishmentBudgetRule
{
    public List<string> Check(LenientGarnishmentBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Checksum)))
        {
            return new List<string> { $"checksum {item.Checksum} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient garnishment budget.</summary>
public sealed record LenientGarnishmentBudgetValidator(List<ILenientGarnishmentBudgetRule> Rules)
{
    public List<string> Validate(LenientGarnishmentBudget item)
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

    public void RequireValid(LenientGarnishmentBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientGarnishmentBudget> items)
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

/// <summary>Functions of lenient garnishment budget rules.</summary>
public static class LenientGarnishmentBudgetRules
{
    public static LenientGarnishmentBudgetValidator DefaultLenientGarnishmentBudgetValidator()
    {
        return new LenientGarnishmentBudgetValidator(
            new List<ILenientGarnishmentBudgetRule>
            {
                new LenientGarnishmentBudgetMaximumRangeRule(6.5, 104.1),
                new LenientGarnishmentBudgetCodePrefixRule("REF"),
                new LenientGarnishmentBudgetChecksumLengthRule(120),
                new LenientGarnishmentBudgetRequiredChecksumRule("lenient garnishment budget needs an entry"),
                new LenientGarnishmentBudgetAllowedChecksumRule(new List<string> { "rolling", "manual", "batched" }),
            });
    }
}
