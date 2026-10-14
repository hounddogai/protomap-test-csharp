namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>A rule a license seat assignment must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatAssignmentRule
{
    List<string> Check(LicenseSeatAssignment item);
}

public sealed record LicenseSeatAssignmentRequiredGradeRule(string Message) : ILicenseSeatAssignmentRule
{
    public List<string> Check(LicenseSeatAssignment item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAssignmentUpdatedAtRangeRule(int Min, int Max) : ILicenseSeatAssignmentRule
{
    public List<string> Check(LicenseSeatAssignment item)
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

public sealed record LicenseSeatAssignmentCodePrefixRule(string Prefix) : ILicenseSeatAssignmentRule
{
    public List<string> Check(LicenseSeatAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAssignmentCodeLengthRule(int MaxLength) : ILicenseSeatAssignmentRule
{
    public List<string> Check(LicenseSeatAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAssignmentLocaleLengthRule(int MaxLength) : ILicenseSeatAssignmentRule
{
    public List<string> Check(LicenseSeatAssignment item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatAssignmentRequiredLabelRule(string Message) : ILicenseSeatAssignmentRule
{
    public List<string> Check(LicenseSeatAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat assignment.</summary>
public sealed record LicenseSeatAssignmentValidator(List<ILicenseSeatAssignmentRule> Rules)
{
    public List<string> Validate(LicenseSeatAssignment item)
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

    public void RequireValid(LicenseSeatAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatAssignment> items)
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

/// <summary>Functions of license seat assignment rules.</summary>
public static class LicenseSeatAssignmentRules
{
    public static LicenseSeatAssignmentValidator DefaultLicenseSeatAssignmentValidator()
    {
        return new LicenseSeatAssignmentValidator(
            new List<ILicenseSeatAssignmentRule>
            {
                new LicenseSeatAssignmentRequiredGradeRule("license seat assignment needs this field"),
                new LicenseSeatAssignmentUpdatedAtRangeRule(16, 80),
                new LicenseSeatAssignmentCodePrefixRule("REF"),
                new LicenseSeatAssignmentCodeLengthRule(255),
                new LicenseSeatAssignmentLocaleLengthRule(120),
                new LicenseSeatAssignmentRequiredLabelRule("license seat assignment needs this field"),
            });
    }
}
