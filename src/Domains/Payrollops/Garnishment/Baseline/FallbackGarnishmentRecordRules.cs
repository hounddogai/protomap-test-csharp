namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>A rule a fallback garnishment record must pass. Check returns its problems, or none.</summary>
public interface IFallbackGarnishmentRecordRule
{
    List<string> Check(FallbackGarnishmentRecord item);
}

public sealed record FallbackGarnishmentRecordOwnerIdLengthRule(int MaxLength) : IFallbackGarnishmentRecordRule
{
    public List<string> Check(FallbackGarnishmentRecord item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentRecordFteRangeRule(double Min, double Max) : IFallbackGarnishmentRecordRule
{
    public List<string> Check(FallbackGarnishmentRecord item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentRecordRequiredOwnerIdRule(string Message) : IFallbackGarnishmentRecordRule
{
    public List<string> Check(FallbackGarnishmentRecord item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentRecordMarginRangeRule(double Min, double Max) : IFallbackGarnishmentRecordRule
{
    public List<string> Check(FallbackGarnishmentRecord item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentRecordRequiredCodeRule(string Message) : IFallbackGarnishmentRecordRule
{
    public List<string> Check(FallbackGarnishmentRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackGarnishmentRecordRequiredCountryRule(string Message) : IFallbackGarnishmentRecordRule
{
    public List<string> Check(FallbackGarnishmentRecord item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback garnishment record.</summary>
public sealed record FallbackGarnishmentRecordValidator(List<IFallbackGarnishmentRecordRule> Rules)
{
    public List<string> Validate(FallbackGarnishmentRecord item)
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

    public void RequireValid(FallbackGarnishmentRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackGarnishmentRecord> items)
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

/// <summary>Functions of fallback garnishment record rules.</summary>
public static class FallbackGarnishmentRecordRules
{
    public static FallbackGarnishmentRecordValidator DefaultFallbackGarnishmentRecordValidator()
    {
        return new FallbackGarnishmentRecordValidator(
            new List<IFallbackGarnishmentRecordRule>
            {
                new FallbackGarnishmentRecordOwnerIdLengthRule(255),
                new FallbackGarnishmentRecordFteRangeRule(2.4, 4926.9),
                new FallbackGarnishmentRecordRequiredOwnerIdRule("fallback garnishment record needs an entry"),
                new FallbackGarnishmentRecordMarginRangeRule(1.1, 2784.5),
                new FallbackGarnishmentRecordRequiredCodeRule("fallback garnishment record needs a value"),
                new FallbackGarnishmentRecordRequiredCountryRule("fallback garnishment record needs an entry"),
            });
    }
}
