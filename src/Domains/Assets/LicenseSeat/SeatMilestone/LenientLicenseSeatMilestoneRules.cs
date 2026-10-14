namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>A rule a lenient license seat milestone must pass. Check returns its problems, or none.</summary>
public interface ILenientLicenseSeatMilestoneRule
{
    List<string> Check(LenientLicenseSeatMilestone item);
}

public sealed record LenientLicenseSeatMilestoneCodePrefixRule(string Prefix) : ILenientLicenseSeatMilestoneRule
{
    public List<string> Check(LenientLicenseSeatMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatMilestoneNameLengthRule(int MaxLength) : ILenientLicenseSeatMilestoneRule
{
    public List<string> Check(LenientLicenseSeatMilestone item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLicenseSeatMilestoneApprovedGuardRule(int Limit) : ILenientLicenseSeatMilestoneRule
{
    public List<string> Check(LenientLicenseSeatMilestone item)
    {
        if (item.Approved && item.Attempts > Limit)
        {
            return new List<string> { $"attempts {item.Attempts} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient license seat milestone.</summary>
public sealed record LenientLicenseSeatMilestoneValidator(List<ILenientLicenseSeatMilestoneRule> Rules)
{
    public List<string> Validate(LenientLicenseSeatMilestone item)
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

    public void RequireValid(LenientLicenseSeatMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLicenseSeatMilestone> items)
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

/// <summary>Functions of lenient license seat milestone rules.</summary>
public static class LenientLicenseSeatMilestoneRules
{
    public static LenientLicenseSeatMilestoneValidator DefaultLenientLicenseSeatMilestoneValidator()
    {
        return new LenientLicenseSeatMilestoneValidator(
            new List<ILenientLicenseSeatMilestoneRule>
            {
                new LenientLicenseSeatMilestoneCodePrefixRule("REF"),
                new LenientLicenseSeatMilestoneNameLengthRule(120),
                new LenientLicenseSeatMilestoneApprovedGuardRule(16),
            });
    }
}
