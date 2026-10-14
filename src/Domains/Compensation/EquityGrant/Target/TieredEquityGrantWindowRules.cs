namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered equity grant window must pass. Check returns its problems, or none.</summary>
public interface ITieredEquityGrantWindowRule
{
    List<string> Check(TieredEquityGrantWindow item);
}

public sealed record TieredEquityGrantWindowRequiredCountryRule(string Message) : ITieredEquityGrantWindowRule
{
    public List<string> Check(TieredEquityGrantWindow item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantWindowCodeLengthRule(int MaxLength) : ITieredEquityGrantWindowRule
{
    public List<string> Check(TieredEquityGrantWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantWindowAllowedCodeRule(List<string> Allowed) : ITieredEquityGrantWindowRule
{
    public List<string> Check(TieredEquityGrantWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantWindowRequiredCodeRule(string Message) : ITieredEquityGrantWindowRule
{
    public List<string> Check(TieredEquityGrantWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantWindowTenureMonthsRangeRule(int Min, int Max) : ITieredEquityGrantWindowRule
{
    public List<string> Check(TieredEquityGrantWindow item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredEquityGrantWindowCountryLengthRule(int MaxLength) : ITieredEquityGrantWindowRule
{
    public List<string> Check(TieredEquityGrantWindow item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered equity grant window.</summary>
public sealed record TieredEquityGrantWindowValidator(List<ITieredEquityGrantWindowRule> Rules)
{
    public List<string> Validate(TieredEquityGrantWindow item)
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

    public void RequireValid(TieredEquityGrantWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredEquityGrantWindow> items)
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

/// <summary>Functions of tiered equity grant window rules.</summary>
public static class TieredEquityGrantWindowRules
{
    public static TieredEquityGrantWindowValidator DefaultTieredEquityGrantWindowValidator()
    {
        return new TieredEquityGrantWindowValidator(
            new List<ITieredEquityGrantWindowRule>
            {
                new TieredEquityGrantWindowRequiredCountryRule("tiered equity grant window needs this field"),
                new TieredEquityGrantWindowCodeLengthRule(3600),
                new TieredEquityGrantWindowAllowedCodeRule(new List<string>()),
                new TieredEquityGrantWindowRequiredCodeRule("tiered equity grant window needs a value"),
                new TieredEquityGrantWindowTenureMonthsRangeRule(1000, 3600),
                new TieredEquityGrantWindowCountryLengthRule(64),
            });
    }
}
