namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>A rule a strict floor plan bundle must pass. Check returns its problems, or none.</summary>
public interface IStrictFloorPlanBundleRule
{
    List<string> Check(StrictFloorPlanBundle item);
}

public sealed record StrictFloorPlanBundleCodeLengthRule(int MaxLength) : IStrictFloorPlanBundleRule
{
    public List<string> Check(StrictFloorPlanBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictFloorPlanBundleStartsAtRangeRule(int Min, int Max) : IStrictFloorPlanBundleRule
{
    public List<string> Check(StrictFloorPlanBundle item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictFloorPlanBundleRequiredSalaryBandRule(string Message) : IStrictFloorPlanBundleRule
{
    public List<string> Check(StrictFloorPlanBundle item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictFloorPlanBundleUnitLengthRule(int MaxLength) : IStrictFloorPlanBundleRule
{
    public List<string> Check(StrictFloorPlanBundle item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictFloorPlanBundleApprovedGuardRule(int Limit) : IStrictFloorPlanBundleRule
{
    public List<string> Check(StrictFloorPlanBundle item)
    {
        if (item.Approved && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict floor plan bundle.</summary>
public sealed record StrictFloorPlanBundleValidator(List<IStrictFloorPlanBundleRule> Rules)
{
    public List<string> Validate(StrictFloorPlanBundle item)
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

    public void RequireValid(StrictFloorPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictFloorPlanBundle> items)
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

/// <summary>Functions of strict floor plan bundle rules.</summary>
public static class StrictFloorPlanBundleRules
{
    public static StrictFloorPlanBundleValidator DefaultStrictFloorPlanBundleValidator()
    {
        return new StrictFloorPlanBundleValidator(
            new List<IStrictFloorPlanBundleRule>
            {
                new StrictFloorPlanBundleCodeLengthRule(120),
                new StrictFloorPlanBundleStartsAtRangeRule(16, 255),
                new StrictFloorPlanBundleRequiredSalaryBandRule("strict floor plan bundle needs a value"),
                new StrictFloorPlanBundleUnitLengthRule(120),
                new StrictFloorPlanBundleApprovedGuardRule(64),
            });
    }
}
