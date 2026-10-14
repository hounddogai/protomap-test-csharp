namespace Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>A rule a accrual rollup must pass. Check returns its problems, or none.</summary>
public interface IAccrualRollupRule
{
    List<string> Check(AccrualRollup item);
}

public sealed record AccrualRollupCodeLengthRule(int MaxLength) : IAccrualRollupRule
{
    public List<string> Check(AccrualRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualRollupCodePrefixRule(string Prefix) : IAccrualRollupRule
{
    public List<string> Check(AccrualRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualRollupRequiredLabelRule(string Message) : IAccrualRollupRule
{
    public List<string> Check(AccrualRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual rollup.</summary>
public sealed record AccrualRollupValidator(List<IAccrualRollupRule> Rules)
{
    public List<string> Validate(AccrualRollup item)
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

    public void RequireValid(AccrualRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualRollup> items)
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

/// <summary>Functions of accrual rollup rules.</summary>
public static class AccrualRollupRules
{
    public static AccrualRollupValidator DefaultAccrualRollupValidator()
    {
        return new AccrualRollupValidator(
            new List<IAccrualRollupRule>
            {
                new AccrualRollupCodeLengthRule(32),
                new AccrualRollupCodePrefixRule("ACM"),
                new AccrualRollupRequiredLabelRule("accrual rollup needs an entry"),
            });
    }
}
