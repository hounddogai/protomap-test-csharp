namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>A rule a occupancy exception must pass. Check returns its problems, or none.</summary>
public interface IOccupancyExceptionRule
{
    List<string> Check(OccupancyException item);
}

public sealed record OccupancyExceptionRequiredTimezoneRule(string Message) : IOccupancyExceptionRule
{
    public List<string> Check(OccupancyException item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyExceptionCodePrefixRule(string Prefix) : IOccupancyExceptionRule
{
    public List<string> Check(OccupancyException item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyExceptionRequiredSummaryRule(string Message) : IOccupancyExceptionRule
{
    public List<string> Check(OccupancyException item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyExceptionWindowDaysRangeRule(int Min, int Max) : IOccupancyExceptionRule
{
    public List<string> Check(OccupancyException item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyExceptionApprovedGuardRule(double Limit) : IOccupancyExceptionRule
{
    public List<string> Check(OccupancyException item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy exception.</summary>
public sealed record OccupancyExceptionValidator(List<IOccupancyExceptionRule> Rules)
{
    public List<string> Validate(OccupancyException item)
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

    public void RequireValid(OccupancyException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyException> items)
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

/// <summary>Functions of occupancy exception rules.</summary>
public static class OccupancyExceptionRules
{
    public static OccupancyExceptionValidator DefaultOccupancyExceptionValidator()
    {
        return new OccupancyExceptionValidator(
            new List<IOccupancyExceptionRule>
            {
                new OccupancyExceptionRequiredTimezoneRule("occupancy exception needs an entry"),
                new OccupancyExceptionCodePrefixRule("REF"),
                new OccupancyExceptionRequiredSummaryRule("occupancy exception needs an entry"),
                new OccupancyExceptionWindowDaysRangeRule(3600, 3600),
                new OccupancyExceptionApprovedGuardRule(4.8),
            });
    }
}
