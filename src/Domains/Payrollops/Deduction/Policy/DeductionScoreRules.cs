namespace Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>A rule a deduction score must pass. Check returns its problems, or none.</summary>
public interface IDeductionScoreRule
{
    List<string> Check(DeductionScore item);
}

public sealed record DeductionScoreTitleLengthRule(int MaxLength) : IDeductionScoreRule
{
    public List<string> Check(DeductionScore item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionScoreCodePrefixRule(string Prefix) : IDeductionScoreRule
{
    public List<string> Check(DeductionScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionScoreUnitLengthRule(int MaxLength) : IDeductionScoreRule
{
    public List<string> Check(DeductionScore item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction score.</summary>
public sealed record DeductionScoreValidator(List<IDeductionScoreRule> Rules)
{
    public List<string> Validate(DeductionScore item)
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

    public void RequireValid(DeductionScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionScore> items)
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

/// <summary>Functions of deduction score rules.</summary>
public static class DeductionScoreRules
{
    public static DeductionScoreValidator DefaultDeductionScoreValidator()
    {
        return new DeductionScoreValidator(
            new List<IDeductionScoreRule>
            {
                new DeductionScoreTitleLengthRule(255),
                new DeductionScoreCodePrefixRule("OPS"),
                new DeductionScoreUnitLengthRule(86400),
            });
    }
}
