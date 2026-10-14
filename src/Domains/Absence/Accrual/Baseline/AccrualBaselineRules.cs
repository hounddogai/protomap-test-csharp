namespace Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>A rule a accrual baseline must pass. Check returns its problems, or none.</summary>
public interface IAccrualBaselineRule
{
    List<string> Check(AccrualBaseline item);
}

public sealed record AccrualBaselineCodeLengthRule(int MaxLength) : IAccrualBaselineRule
{
    public List<string> Check(AccrualBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBaselineBatchSizeRangeRule(int Min, int Max) : IAccrualBaselineRule
{
    public List<string> Check(AccrualBaseline item)
    {
        var valueValue = item.BatchSize;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"batch size {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBaselineRequiredOwnerIdRule(string Message) : IAccrualBaselineRule
{
    public List<string> Check(AccrualBaseline item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AccrualBaselineCodePrefixRule(string Prefix) : IAccrualBaselineRule
{
    public List<string> Check(AccrualBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualBaselineRequiredCodeRule(string Message) : IAccrualBaselineRule
{
    public List<string> Check(AccrualBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual baseline.</summary>
public sealed record AccrualBaselineValidator(List<IAccrualBaselineRule> Rules)
{
    public List<string> Validate(AccrualBaseline item)
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

    public void RequireValid(AccrualBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualBaseline> items)
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

/// <summary>Functions of accrual baseline rules.</summary>
public static class AccrualBaselineRules
{
    public static AccrualBaselineValidator DefaultAccrualBaselineValidator()
    {
        return new AccrualBaselineValidator(
            new List<IAccrualBaselineRule>
            {
                new AccrualBaselineCodeLengthRule(80),
                new AccrualBaselineBatchSizeRangeRule(255, 1000),
                new AccrualBaselineRequiredOwnerIdRule("accrual baseline needs a value"),
                new AccrualBaselineCodePrefixRule("OPS"),
                new AccrualBaselineRequiredCodeRule("accrual baseline needs a value"),
            });
    }
}
