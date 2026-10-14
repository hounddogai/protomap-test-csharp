namespace Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>A rule a seasonal payslip snapshot must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayslipSnapshotRule
{
    List<string> Check(SeasonalPayslipSnapshot item);
}

public sealed record SeasonalPayslipSnapshotRequiredLabelRule(string Message) : ISeasonalPayslipSnapshotRule
{
    public List<string> Check(SeasonalPayslipSnapshot item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSnapshotUpdatedAtRangeRule(int Min, int Max) : ISeasonalPayslipSnapshotRule
{
    public List<string> Check(SeasonalPayslipSnapshot item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPayslipSnapshotLimitRangeRule(int Min, int Max) : ISeasonalPayslipSnapshotRule
{
    public List<string> Check(SeasonalPayslipSnapshot item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSnapshotCodePrefixRule(string Prefix) : ISeasonalPayslipSnapshotRule
{
    public List<string> Check(SeasonalPayslipSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSnapshotCodeLengthRule(int MaxLength) : ISeasonalPayslipSnapshotRule
{
    public List<string> Check(SeasonalPayslipSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayslipSnapshotRequiredCodeRule(string Message) : ISeasonalPayslipSnapshotRule
{
    public List<string> Check(SeasonalPayslipSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal payslip snapshot.</summary>
public sealed record SeasonalPayslipSnapshotValidator(List<ISeasonalPayslipSnapshotRule> Rules)
{
    public List<string> Validate(SeasonalPayslipSnapshot item)
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

    public void RequireValid(SeasonalPayslipSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayslipSnapshot> items)
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

/// <summary>Functions of seasonal payslip snapshot rules.</summary>
public static class SeasonalPayslipSnapshotRules
{
    public static SeasonalPayslipSnapshotValidator DefaultSeasonalPayslipSnapshotValidator()
    {
        return new SeasonalPayslipSnapshotValidator(
            new List<ISeasonalPayslipSnapshotRule>
            {
                new SeasonalPayslipSnapshotRequiredLabelRule("seasonal payslip snapshot needs this field"),
                new SeasonalPayslipSnapshotUpdatedAtRangeRule(64, 80),
                new SeasonalPayslipSnapshotLimitRangeRule(16, 255),
                new SeasonalPayslipSnapshotCodePrefixRule("Q"),
                new SeasonalPayslipSnapshotCodeLengthRule(80),
                new SeasonalPayslipSnapshotRequiredCodeRule("seasonal payslip snapshot needs a value"),
            });
    }
}
