namespace Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>A rule a merit cycle quota must pass. Check returns its problems, or none.</summary>
public interface IMeritCycleQuotaRule
{
    List<string> Check(MeritCycleQuota item);
}

public sealed record MeritCycleQuotaCategoryLengthRule(int MaxLength) : IMeritCycleQuotaRule
{
    public List<string> Check(MeritCycleQuota item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleQuotaApprovedGuardRule(double Limit) : IMeritCycleQuotaRule
{
    public List<string> Check(MeritCycleQuota item)
    {
        if (item.Approved && item.Score > Limit)
        {
            return new List<string> { $"score {item.Score} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleQuotaRequiredCodeRule(string Message) : IMeritCycleQuotaRule
{
    public List<string> Check(MeritCycleQuota item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleQuotaFloorValueRangeRule(double Min, double Max) : IMeritCycleQuotaRule
{
    public List<string> Check(MeritCycleQuota item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MeritCycleQuotaRequiredCurrencyRule(string Message) : IMeritCycleQuotaRule
{
    public List<string> Check(MeritCycleQuota item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MeritCycleQuotaCurrencyLengthRule(int MaxLength) : IMeritCycleQuotaRule
{
    public List<string> Check(MeritCycleQuota item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a merit cycle quota.</summary>
public sealed record MeritCycleQuotaValidator(List<IMeritCycleQuotaRule> Rules)
{
    public List<string> Validate(MeritCycleQuota item)
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

    public void RequireValid(MeritCycleQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MeritCycleQuota> items)
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

/// <summary>Functions of merit cycle quota rules.</summary>
public static class MeritCycleQuotaRules
{
    public static MeritCycleQuotaValidator DefaultMeritCycleQuotaValidator()
    {
        return new MeritCycleQuotaValidator(
            new List<IMeritCycleQuotaRule>
            {
                new MeritCycleQuotaCategoryLengthRule(1000),
                new MeritCycleQuotaApprovedGuardRule(9.8),
                new MeritCycleQuotaRequiredCodeRule("merit cycle quota needs an entry"),
                new MeritCycleQuotaFloorValueRangeRule(0.1, 245.1),
                new MeritCycleQuotaRequiredCurrencyRule("merit cycle quota needs a value"),
                new MeritCycleQuotaCurrencyLengthRule(86400),
            });
    }
}
