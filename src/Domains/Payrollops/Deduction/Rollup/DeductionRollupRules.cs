namespace Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>A rule a deduction rollup must pass. Check returns its problems, or none.</summary>
public interface IDeductionRollupRule
{
    List<string> Check(DeductionRollup item);
}

public sealed record DeductionRollupCodePrefixRule(string Prefix) : IDeductionRollupRule
{
    public List<string> Check(DeductionRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionRollupRequiredCodeRule(string Message) : IDeductionRollupRule
{
    public List<string> Check(DeductionRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionRollupSequenceRangeRule(int Min, int Max) : IDeductionRollupRule
{
    public List<string> Check(DeductionRollup item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a deduction rollup.</summary>
public sealed record DeductionRollupValidator(List<IDeductionRollupRule> Rules)
{
    public List<string> Validate(DeductionRollup item)
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

    public void RequireValid(DeductionRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionRollup> items)
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

/// <summary>Functions of deduction rollup rules.</summary>
public static class DeductionRollupRules
{
    public static DeductionRollupValidator DefaultDeductionRollupValidator()
    {
        return new DeductionRollupValidator(
            new List<IDeductionRollupRule>
            {
                new DeductionRollupCodePrefixRule("OPS"),
                new DeductionRollupRequiredCodeRule("deduction rollup needs an entry"),
                new DeductionRollupSequenceRangeRule(120, 255),
            });
    }
}
