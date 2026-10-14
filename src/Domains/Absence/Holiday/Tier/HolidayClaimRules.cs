namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday claim must pass. Check returns its problems, or none.</summary>
public interface IHolidayClaimRule
{
    List<string> Check(HolidayClaim item);
}

public sealed record HolidayClaimRequiredPhoneRule(string Message) : IHolidayClaimRule
{
    public List<string> Check(HolidayClaim item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayClaimAllowedPhoneRule(List<string> Allowed) : IHolidayClaimRule
{
    public List<string> Check(HolidayClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record HolidayClaimRequiredCurrencyRule(string Message) : IHolidayClaimRule
{
    public List<string> Check(HolidayClaim item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayClaimFloorValueRangeRule(double Min, double Max) : IHolidayClaimRule
{
    public List<string> Check(HolidayClaim item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayClaimCodePrefixRule(string Prefix) : IHolidayClaimRule
{
    public List<string> Check(HolidayClaim item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayClaimCodeLengthRule(int MaxLength) : IHolidayClaimRule
{
    public List<string> Check(HolidayClaim item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday claim.</summary>
public sealed record HolidayClaimValidator(List<IHolidayClaimRule> Rules)
{
    public List<string> Validate(HolidayClaim item)
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

    public void RequireValid(HolidayClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayClaim> items)
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

/// <summary>Functions of holiday claim rules.</summary>
public static class HolidayClaimRules
{
    public static HolidayClaimValidator DefaultHolidayClaimValidator()
    {
        return new HolidayClaimValidator(
            new List<IHolidayClaimRule>
            {
                new HolidayClaimRequiredPhoneRule("holiday claim needs a value"),
                new HolidayClaimAllowedPhoneRule(new List<string> { "batched", "weighted" }),
                new HolidayClaimRequiredCurrencyRule("holiday claim needs this field"),
                new HolidayClaimFloorValueRangeRule(7.1, 4940.6),
                new HolidayClaimCodePrefixRule("ACM"),
                new HolidayClaimCodeLengthRule(86400),
            });
    }
}
