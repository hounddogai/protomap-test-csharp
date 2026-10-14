namespace Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>A rule a primary adjustment assignment must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAdjustmentAssignmentRule
{
    List<string> Check(PrimaryAdjustmentAssignment item);
}

public sealed record PrimaryAdjustmentAssignmentAmountRangeRule(double Min, double Max) : IPrimaryAdjustmentAssignmentRule
{
    public List<string> Check(PrimaryAdjustmentAssignment item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAdjustmentAssignmentRequiredTitleRule(string Message) : IPrimaryAdjustmentAssignmentRule
{
    public List<string> Check(PrimaryAdjustmentAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAdjustmentAssignmentTitleLengthRule(int MaxLength) : IPrimaryAdjustmentAssignmentRule
{
    public List<string> Check(PrimaryAdjustmentAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAdjustmentAssignmentCodePrefixRule(string Prefix) : IPrimaryAdjustmentAssignmentRule
{
    public List<string> Check(PrimaryAdjustmentAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAdjustmentAssignmentRequiredChecksumRule(string Message) : IPrimaryAdjustmentAssignmentRule
{
    public List<string> Check(PrimaryAdjustmentAssignment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAdjustmentAssignmentRateRangeRule(double Min, double Max) : IPrimaryAdjustmentAssignmentRule
{
    public List<string> Check(PrimaryAdjustmentAssignment item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary adjustment assignment.</summary>
public sealed record PrimaryAdjustmentAssignmentValidator(List<IPrimaryAdjustmentAssignmentRule> Rules)
{
    public List<string> Validate(PrimaryAdjustmentAssignment item)
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

    public void RequireValid(PrimaryAdjustmentAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAdjustmentAssignment> items)
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

/// <summary>Functions of primary adjustment assignment rules.</summary>
public static class PrimaryAdjustmentAssignmentRules
{
    public static PrimaryAdjustmentAssignmentValidator DefaultPrimaryAdjustmentAssignmentValidator()
    {
        return new PrimaryAdjustmentAssignmentValidator(
            new List<IPrimaryAdjustmentAssignmentRule>
            {
                new PrimaryAdjustmentAssignmentAmountRangeRule(2.3, 4150.7),
                new PrimaryAdjustmentAssignmentRequiredTitleRule("primary adjustment assignment needs this field"),
                new PrimaryAdjustmentAssignmentTitleLengthRule(3600),
                new PrimaryAdjustmentAssignmentCodePrefixRule("OPS"),
                new PrimaryAdjustmentAssignmentRequiredChecksumRule("primary adjustment assignment needs an entry"),
                new PrimaryAdjustmentAssignmentRateRangeRule(3.5, 589.7),
            });
    }
}
