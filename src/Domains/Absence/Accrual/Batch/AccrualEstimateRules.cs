namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual estimate must pass. Check returns its problems, or none.</summary>
public interface IAccrualEstimateRule
{
    List<string> Check(AccrualEstimate item);
}

public sealed record AccrualEstimateAllowedCodeRule(List<string> Allowed) : IAccrualEstimateRule
{
    public List<string> Check(AccrualEstimate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record AccrualEstimateCodeLengthRule(int MaxLength) : IAccrualEstimateRule
{
    public List<string> Check(AccrualEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualEstimateRequiredNameRule(string Message) : IAccrualEstimateRule
{
    public List<string> Check(AccrualEstimate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualEstimateCodePrefixRule(string Prefix) : IAccrualEstimateRule
{
    public List<string> Check(AccrualEstimate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual estimate.</summary>
public sealed record AccrualEstimateValidator(List<IAccrualEstimateRule> Rules)
{
    public List<string> Validate(AccrualEstimate item)
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

    public void RequireValid(AccrualEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualEstimate> items)
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

/// <summary>Functions of accrual estimate rules.</summary>
public static class AccrualEstimateRules
{
    public static AccrualEstimateValidator DefaultAccrualEstimateValidator()
    {
        return new AccrualEstimateValidator(
            new List<IAccrualEstimateRule>
            {
                new AccrualEstimateAllowedCodeRule(new List<string>()),
                new AccrualEstimateCodeLengthRule(1000),
                new AccrualEstimateRequiredNameRule("accrual estimate needs an entry"),
                new AccrualEstimateCodePrefixRule("REF"),
            });
    }
}
