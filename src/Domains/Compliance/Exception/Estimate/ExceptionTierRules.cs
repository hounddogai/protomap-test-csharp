namespace Acme.Workforce.Compliance.Exception.Estimate;

/// <summary>A rule a exception tier must pass. Check returns its problems, or none.</summary>
public interface IExceptionTierRule
{
    List<string> Check(ExceptionTier item);
}

public sealed record ExceptionTierCodeLengthRule(int MaxLength) : IExceptionTierRule
{
    public List<string> Check(ExceptionTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTierRegionLengthRule(int MaxLength) : IExceptionTierRule
{
    public List<string> Check(ExceptionTier item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTierGraceDaysRangeRule(int Min, int Max) : IExceptionTierRule
{
    public List<string> Check(ExceptionTier item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTierArchivedGuardRule(double Limit) : IExceptionTierRule
{
    public List<string> Check(ExceptionTier item)
    {
        if (item.Archived && item.Maximum > Limit)
        {
            return new List<string> { $"maximum {item.Maximum} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionTierMaximumRangeRule(double Min, double Max) : IExceptionTierRule
{
    public List<string> Check(ExceptionTier item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a exception tier.</summary>
public sealed record ExceptionTierValidator(List<IExceptionTierRule> Rules)
{
    public List<string> Validate(ExceptionTier item)
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

    public void RequireValid(ExceptionTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionTier> items)
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

/// <summary>Functions of exception tier rules.</summary>
public static class ExceptionTierRules
{
    public static ExceptionTierValidator DefaultExceptionTierValidator()
    {
        return new ExceptionTierValidator(
            new List<IExceptionTierRule>
            {
                new ExceptionTierCodeLengthRule(255),
                new ExceptionTierRegionLengthRule(86400),
                new ExceptionTierGraceDaysRangeRule(32, 1000),
                new ExceptionTierArchivedGuardRule(4.7),
                new ExceptionTierMaximumRangeRule(6.7, 3232.9),
            });
    }
}
