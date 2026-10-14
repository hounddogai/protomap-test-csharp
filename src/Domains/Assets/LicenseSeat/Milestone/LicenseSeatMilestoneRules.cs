namespace Acme.Workforce.Assets.LicenseSeat.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a license seat milestone must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatMilestoneRule
{
    List<string> Check(LicenseSeatMilestone item);
}

public sealed record LicenseSeatMilestoneContactEmailLengthRule(int MaxLength) : ILicenseSeatMilestoneRule
{
    public List<string> Check(LicenseSeatMilestone item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMilestoneAllowedSegmentRule(List<string> Allowed) : ILicenseSeatMilestoneRule
{
    public List<string> Check(LicenseSeatMilestone item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Segment)))
        {
            return new List<string> { $"segment {item.Segment} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMilestoneSegmentLengthRule(int MaxLength) : ILicenseSeatMilestoneRule
{
    public List<string> Check(LicenseSeatMilestone item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMilestoneArchivedGuardRule(int Limit) : ILicenseSeatMilestoneRule
{
    public List<string> Check(LicenseSeatMilestone item)
    {
        if (item.Archived && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMilestoneRequiredSegmentRule(string Message) : ILicenseSeatMilestoneRule
{
    public List<string> Check(LicenseSeatMilestone item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatMilestoneRequiredCodeRule(string Message) : ILicenseSeatMilestoneRule
{
    public List<string> Check(LicenseSeatMilestone item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat milestone.</summary>
public sealed record LicenseSeatMilestoneValidator(List<ILicenseSeatMilestoneRule> Rules)
{
    public List<string> Validate(LicenseSeatMilestone item)
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

    public void RequireValid(LicenseSeatMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatMilestone> items)
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

/// <summary>Functions of license seat milestone rules.</summary>
public static class LicenseSeatMilestoneRules
{
    public static LicenseSeatMilestoneValidator DefaultLicenseSeatMilestoneValidator()
    {
        return new LicenseSeatMilestoneValidator(
            new List<ILicenseSeatMilestoneRule>
            {
                new LicenseSeatMilestoneContactEmailLengthRule(255),
                new LicenseSeatMilestoneAllowedSegmentRule(new List<string> { "seasonal", "fallback", "seasonal" }),
                new LicenseSeatMilestoneSegmentLengthRule(3600),
                new LicenseSeatMilestoneArchivedGuardRule(86400),
                new LicenseSeatMilestoneRequiredSegmentRule("license seat milestone needs an entry"),
                new LicenseSeatMilestoneRequiredCodeRule("license seat milestone needs a value"),
            });
    }
}
