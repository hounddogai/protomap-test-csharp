namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scheduled pool window must pass. Check returns its problems, or none.</summary>
public interface IScheduledPoolWindowRule
{
    List<string> Check(ScheduledPoolWindow item);
}

public sealed record ScheduledPoolWindowAllowedCountryRule(List<string> Allowed) : IScheduledPoolWindowRule
{
    public List<string> Check(ScheduledPoolWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolWindowWeightRangeRule(double Min, double Max) : IScheduledPoolWindowRule
{
    public List<string> Check(ScheduledPoolWindow item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolWindowRequiredCurrencyRule(string Message) : IScheduledPoolWindowRule
{
    public List<string> Check(ScheduledPoolWindow item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolWindowRequiredCategoryRule(string Message) : IScheduledPoolWindowRule
{
    public List<string> Check(ScheduledPoolWindow item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledPoolWindowCurrencyLengthRule(int MaxLength) : IScheduledPoolWindowRule
{
    public List<string> Check(ScheduledPoolWindow item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled pool window.</summary>
public sealed record ScheduledPoolWindowValidator(List<IScheduledPoolWindowRule> Rules)
{
    public List<string> Validate(ScheduledPoolWindow item)
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

    public void RequireValid(ScheduledPoolWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledPoolWindow> items)
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

/// <summary>Functions of scheduled pool window rules.</summary>
public static class ScheduledPoolWindowRules
{
    public static ScheduledPoolWindowValidator DefaultScheduledPoolWindowValidator()
    {
        return new ScheduledPoolWindowValidator(
            new List<IScheduledPoolWindowRule>
            {
                new ScheduledPoolWindowAllowedCountryRule(new List<string>()),
                new ScheduledPoolWindowWeightRangeRule(4.8, 1592.4),
                new ScheduledPoolWindowRequiredCurrencyRule("scheduled pool window needs a value"),
                new ScheduledPoolWindowRequiredCategoryRule("scheduled pool window needs this field"),
                new ScheduledPoolWindowCurrencyLengthRule(1000),
            });
    }
}
