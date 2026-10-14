namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate cycle must pass. Check returns its problems, or none.</summary>
public interface IRateCycleRule
{
    List<string> Check(RateCycle item);
}

public sealed record RateCycleAllowedTitleRule(List<string> Allowed) : IRateCycleRule
{
    public List<string> Check(RateCycle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RateCycleRequiredCodeRule(string Message) : IRateCycleRule
{
    public List<string> Check(RateCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateCycleSalaryBandLengthRule(int MaxLength) : IRateCycleRule
{
    public List<string> Check(RateCycle item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateCycleRequiredTitleRule(string Message) : IRateCycleRule
{
    public List<string> Check(RateCycle item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateCycleRequiredEmployeeIdRule(string Message) : IRateCycleRule
{
    public List<string> Check(RateCycle item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RateCycleStartsAtRangeRule(int Min, int Max) : IRateCycleRule
{
    public List<string> Check(RateCycle item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate cycle.</summary>
public sealed record RateCycleValidator(List<IRateCycleRule> Rules)
{
    public List<string> Validate(RateCycle item)
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

    public void RequireValid(RateCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateCycle> items)
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

/// <summary>Functions of rate cycle rules.</summary>
public static class RateCycleRules
{
    public static RateCycleValidator DefaultRateCycleValidator()
    {
        return new RateCycleValidator(
            new List<IRateCycleRule>
            {
                new RateCycleAllowedTitleRule(new List<string>()),
                new RateCycleRequiredCodeRule("rate cycle needs an entry"),
                new RateCycleSalaryBandLengthRule(32),
                new RateCycleRequiredTitleRule("rate cycle needs a value"),
                new RateCycleRequiredEmployeeIdRule("rate cycle needs this field"),
                new RateCycleStartsAtRangeRule(64, 255),
            });
    }
}
