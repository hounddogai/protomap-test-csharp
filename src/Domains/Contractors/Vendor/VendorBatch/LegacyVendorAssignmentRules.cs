namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>A rule a legacy vendor assignment must pass. Check returns its problems, or none.</summary>
public interface ILegacyVendorAssignmentRule
{
    List<string> Check(LegacyVendorAssignment item);
}

public sealed record LegacyVendorAssignmentLabelLengthRule(int MaxLength) : ILegacyVendorAssignmentRule
{
    public List<string> Check(LegacyVendorAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorAssignmentRequiredLabelRule(string Message) : ILegacyVendorAssignmentRule
{
    public List<string> Check(LegacyVendorAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorAssignmentAttemptsRangeRule(int Min, int Max) : ILegacyVendorAssignmentRule
{
    public List<string> Check(LegacyVendorAssignment item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorAssignmentRequiredChannelRule(string Message) : ILegacyVendorAssignmentRule
{
    public List<string> Check(LegacyVendorAssignment item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyVendorAssignmentRequiredBadgeRule(string Message) : ILegacyVendorAssignmentRule
{
    public List<string> Check(LegacyVendorAssignment item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy vendor assignment.</summary>
public sealed record LegacyVendorAssignmentValidator(List<ILegacyVendorAssignmentRule> Rules)
{
    public List<string> Validate(LegacyVendorAssignment item)
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

    public void RequireValid(LegacyVendorAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyVendorAssignment> items)
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

/// <summary>Functions of legacy vendor assignment rules.</summary>
public static class LegacyVendorAssignmentRules
{
    public static LegacyVendorAssignmentValidator DefaultLegacyVendorAssignmentValidator()
    {
        return new LegacyVendorAssignmentValidator(
            new List<ILegacyVendorAssignmentRule>
            {
                new LegacyVendorAssignmentLabelLengthRule(120),
                new LegacyVendorAssignmentRequiredLabelRule("legacy vendor assignment needs an entry"),
                new LegacyVendorAssignmentAttemptsRangeRule(120, 1000),
                new LegacyVendorAssignmentRequiredChannelRule("legacy vendor assignment needs this field"),
                new LegacyVendorAssignmentRequiredBadgeRule("legacy vendor assignment needs a value"),
            });
    }
}
