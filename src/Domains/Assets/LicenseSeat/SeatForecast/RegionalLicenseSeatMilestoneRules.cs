namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>A rule a regional license seat milestone must pass. Check returns its problems, or none.</summary>
public interface IRegionalLicenseSeatMilestoneRule
{
    List<string> Check(RegionalLicenseSeatMilestone item);
}

public sealed record RegionalLicenseSeatMilestonePinnedGuardRule(double Limit) : IRegionalLicenseSeatMilestoneRule
{
    public List<string> Check(RegionalLicenseSeatMilestone item)
    {
        if (item.Pinned && item.Cost > Limit)
        {
            return new List<string> { $"cost {item.Cost} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RegionalLicenseSeatMilestoneRequiredNotesRule(string Message) : IRegionalLicenseSeatMilestoneRule
{
    public List<string> Check(RegionalLicenseSeatMilestone item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalLicenseSeatMilestoneCodePrefixRule(string Prefix) : IRegionalLicenseSeatMilestoneRule
{
    public List<string> Check(RegionalLicenseSeatMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalLicenseSeatMilestoneDiscountRangeRule(double Min, double Max) : IRegionalLicenseSeatMilestoneRule
{
    public List<string> Check(RegionalLicenseSeatMilestone item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalLicenseSeatMilestoneRequiredEmployeeIdRule(string Message) : IRegionalLicenseSeatMilestoneRule
{
    public List<string> Check(RegionalLicenseSeatMilestone item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional license seat milestone.</summary>
public sealed record RegionalLicenseSeatMilestoneValidator(List<IRegionalLicenseSeatMilestoneRule> Rules)
{
    public List<string> Validate(RegionalLicenseSeatMilestone item)
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

    public void RequireValid(RegionalLicenseSeatMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalLicenseSeatMilestone> items)
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

/// <summary>Functions of regional license seat milestone rules.</summary>
public static class RegionalLicenseSeatMilestoneRules
{
    public static RegionalLicenseSeatMilestoneValidator DefaultRegionalLicenseSeatMilestoneValidator()
    {
        return new RegionalLicenseSeatMilestoneValidator(
            new List<IRegionalLicenseSeatMilestoneRule>
            {
                new RegionalLicenseSeatMilestonePinnedGuardRule(7.7),
                new RegionalLicenseSeatMilestoneRequiredNotesRule("regional license seat milestone needs an entry"),
                new RegionalLicenseSeatMilestoneCodePrefixRule("ACM"),
                new RegionalLicenseSeatMilestoneDiscountRangeRule(5.4, 2125.0),
                new RegionalLicenseSeatMilestoneRequiredEmployeeIdRule(
                    "regional license seat milestone needs a value"),
            });
    }
}
