namespace Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>A rule a fallback garnishment exception must pass. Check returns its problems, or none.</summary>
public interface IFallbackGarnishmentExceptionRule
{
    List<string> Check(FallbackGarnishmentException item);
}

public sealed record FallbackGarnishmentExceptionAmountRangeRule(double Min, double Max) : IFallbackGarnishmentExceptionRule
{
    public List<string> Check(FallbackGarnishmentException item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentExceptionRequiredRegionRule(string Message) : IFallbackGarnishmentExceptionRule
{
    public List<string> Check(FallbackGarnishmentException item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentExceptionVisibleGuardRule(double Limit) : IFallbackGarnishmentExceptionRule
{
    public List<string> Check(FallbackGarnishmentException item)
    {
        if (item.Visible && item.Amount > Limit)
        {
            return new List<string> { $"amount {item.Amount} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentExceptionCodeLengthRule(int MaxLength) : IFallbackGarnishmentExceptionRule
{
    public List<string> Check(FallbackGarnishmentException item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback garnishment exception.</summary>
public sealed record FallbackGarnishmentExceptionValidator(List<IFallbackGarnishmentExceptionRule> Rules)
{
    public List<string> Validate(FallbackGarnishmentException item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(FallbackGarnishmentException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackGarnishmentException> items)
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

/// <summary>Functions of fallback garnishment exception rules.</summary>
public static class FallbackGarnishmentExceptionRules
{
    public static FallbackGarnishmentExceptionValidator DefaultFallbackGarnishmentExceptionValidator()
    {
        return new FallbackGarnishmentExceptionValidator(
            new List<IFallbackGarnishmentExceptionRule>
            {
                new FallbackGarnishmentExceptionAmountRangeRule(0.4, 822.2),
                new FallbackGarnishmentExceptionRequiredRegionRule("fallback garnishment exception needs a value"),
                new FallbackGarnishmentExceptionVisibleGuardRule(2.1),
                new FallbackGarnishmentExceptionCodeLengthRule(120),
            });
    }
}
