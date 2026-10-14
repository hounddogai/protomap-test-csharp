namespace Acme.Workforce.Absence.Holiday.Quota;

/// <summary>A rule a holiday quota must pass. Check returns its problems, or none.</summary>
public interface IHolidayQuotaRule
{
    List<string> Check(HolidayQuota item);
}

public sealed record HolidayQuotaQuantityRangeRule(int Min, int Max) : IHolidayQuotaRule
{
    public List<string> Check(HolidayQuota item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayQuotaCodePrefixRule(string Prefix) : IHolidayQuotaRule
{
    public List<string> Check(HolidayQuota item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayQuotaRequiredContactEmailRule(string Message) : IHolidayQuotaRule
{
    public List<string> Check(HolidayQuota item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayQuotaFloorValueRangeRule(double Min, double Max) : IHolidayQuotaRule
{
    public List<string> Check(HolidayQuota item)
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

/// <summary>Runs every rule of a holiday quota.</summary>
public sealed record HolidayQuotaValidator(List<IHolidayQuotaRule> Rules)
{
    public List<string> Validate(HolidayQuota item)
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

    public void RequireValid(HolidayQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayQuota> items)
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

/// <summary>Functions of holiday quota rules.</summary>
public static class HolidayQuotaRules
{
    public static HolidayQuotaValidator DefaultHolidayQuotaValidator()
    {
        return new HolidayQuotaValidator(
            new List<IHolidayQuotaRule>
            {
                new HolidayQuotaQuantityRangeRule(16, 1000),
                new HolidayQuotaCodePrefixRule("OPS"),
                new HolidayQuotaRequiredContactEmailRule("holiday quota needs a value"),
                new HolidayQuotaFloorValueRangeRule(3.9, 403.5),
            });
    }
}
