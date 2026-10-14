namespace Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>A rule a capped license seat cycle must pass. Check returns its problems, or none.</summary>
public interface ICappedLicenseSeatCycleRule
{
    List<string> Check(CappedLicenseSeatCycle item);
}

public sealed record CappedLicenseSeatCycleCodePrefixRule(string Prefix) : ICappedLicenseSeatCycleRule
{
    public List<string> Check(CappedLicenseSeatCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCycleRequiredCodeRule(string Message) : ICappedLicenseSeatCycleRule
{
    public List<string> Check(CappedLicenseSeatCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCycleRequiredNameRule(string Message) : ICappedLicenseSeatCycleRule
{
    public List<string> Check(CappedLicenseSeatCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCycleCategoryLengthRule(int MaxLength) : ICappedLicenseSeatCycleRule
{
    public List<string> Check(CappedLicenseSeatCycle item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedLicenseSeatCycleEndsAtRangeRule(int Min, int Max) : ICappedLicenseSeatCycleRule
{
    public List<string> Check(CappedLicenseSeatCycle item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a capped license seat cycle.</summary>
public sealed record CappedLicenseSeatCycleValidator(List<ICappedLicenseSeatCycleRule> Rules)
{
    public List<string> Validate(CappedLicenseSeatCycle item)
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

    public void RequireValid(CappedLicenseSeatCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedLicenseSeatCycle> items)
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

/// <summary>Functions of capped license seat cycle rules.</summary>
public static class CappedLicenseSeatCycleRules
{
    public static CappedLicenseSeatCycleValidator DefaultCappedLicenseSeatCycleValidator()
    {
        return new CappedLicenseSeatCycleValidator(
            new List<ICappedLicenseSeatCycleRule>
            {
                new CappedLicenseSeatCycleCodePrefixRule("ACM"),
                new CappedLicenseSeatCycleRequiredCodeRule("capped license seat cycle needs a value"),
                new CappedLicenseSeatCycleRequiredNameRule("capped license seat cycle needs this field"),
                new CappedLicenseSeatCycleCategoryLengthRule(120),
                new CappedLicenseSeatCycleEndsAtRangeRule(32, 120),
            });
    }
}
