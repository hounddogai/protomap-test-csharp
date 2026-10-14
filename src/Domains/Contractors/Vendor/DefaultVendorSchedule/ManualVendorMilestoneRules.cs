namespace Acme.Workforce.Contractors.Vendor.DefaultVendorSchedule;

/// <summary>A rule a manual vendor milestone must pass. Check returns its problems, or none.</summary>
public interface IManualVendorMilestoneRule
{
    List<string> Check(ManualVendorMilestone item);
}

public sealed record ManualVendorMilestoneNameLengthRule(int MaxLength) : IManualVendorMilestoneRule
{
    public List<string> Check(ManualVendorMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVendorMilestoneDescriptionLengthRule(int MaxLength) : IManualVendorMilestoneRule
{
    public List<string> Check(ManualVendorMilestone item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualVendorMilestoneRequiredDescriptionRule(string Message) : IManualVendorMilestoneRule
{
    public List<string> Check(ManualVendorMilestone item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualVendorMilestoneSequenceRangeRule(int Min, int Max) : IManualVendorMilestoneRule
{
    public List<string> Check(ManualVendorMilestone item)
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

public sealed record ManualVendorMilestoneVersionRangeRule(int Min, int Max) : IManualVendorMilestoneRule
{
    public List<string> Check(ManualVendorMilestone item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual vendor milestone.</summary>
public sealed record ManualVendorMilestoneValidator(List<IManualVendorMilestoneRule> Rules)
{
    public List<string> Validate(ManualVendorMilestone item)
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

    public void RequireValid(ManualVendorMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualVendorMilestone> items)
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

/// <summary>Functions of manual vendor milestone rules.</summary>
public static class ManualVendorMilestoneRules
{
    public static ManualVendorMilestoneValidator DefaultManualVendorMilestoneValidator()
    {
        return new ManualVendorMilestoneValidator(
            new List<IManualVendorMilestoneRule>
            {
                new ManualVendorMilestoneNameLengthRule(255),
                new ManualVendorMilestoneDescriptionLengthRule(16),
                new ManualVendorMilestoneRequiredDescriptionRule("manual vendor milestone needs this field"),
                new ManualVendorMilestoneSequenceRangeRule(32, 86400),
                new ManualVendorMilestoneVersionRangeRule(80, 3600),
            });
    }
}
