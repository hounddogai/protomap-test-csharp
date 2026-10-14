namespace Acme.Workforce.Absence.Holiday.Policy;

/// <summary>A rule a holiday variance must pass. Check returns its problems, or none.</summary>
public interface IHolidayVarianceRule
{
    List<string> Check(HolidayVariance item);
}

public sealed record HolidayVariancePhoneLengthRule(int MaxLength) : IHolidayVarianceRule
{
    public List<string> Check(HolidayVariance item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayVarianceRequiredNameRule(string Message) : IHolidayVarianceRule
{
    public List<string> Check(HolidayVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayVarianceCodeLengthRule(int MaxLength) : IHolidayVarianceRule
{
    public List<string> Check(HolidayVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayVarianceApprovedGuardRule(double Limit) : IHolidayVarianceRule
{
    public List<string> Check(HolidayVariance item)
    {
        if (item.Approved && item.Discount > Limit)
        {
            return new List<string> { $"discount {item.Discount} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record HolidayVarianceRequiredCostCenterRule(string Message) : IHolidayVarianceRule
{
    public List<string> Check(HolidayVariance item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday variance.</summary>
public sealed record HolidayVarianceValidator(List<IHolidayVarianceRule> Rules)
{
    public List<string> Validate(HolidayVariance item)
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

    public void RequireValid(HolidayVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayVariance> items)
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

/// <summary>Functions of holiday variance rules.</summary>
public static class HolidayVarianceRules
{
    public static HolidayVarianceValidator DefaultHolidayVarianceValidator()
    {
        return new HolidayVarianceValidator(
            new List<IHolidayVarianceRule>
            {
                new HolidayVariancePhoneLengthRule(3600),
                new HolidayVarianceRequiredNameRule("holiday variance needs an entry"),
                new HolidayVarianceCodeLengthRule(64),
                new HolidayVarianceApprovedGuardRule(4.4),
                new HolidayVarianceRequiredCostCenterRule("holiday variance needs this field"),
            });
    }
}
