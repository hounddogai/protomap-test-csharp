namespace Acme.Workforce.Assets.LicenseSeat.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback license seat record must pass. Check returns its problems, or none.</summary>
public interface IFallbackLicenseSeatRecordRule
{
    List<string> Check(FallbackLicenseSeatRecord item);
}

public sealed record FallbackLicenseSeatRecordCodePrefixRule(string Prefix) : IFallbackLicenseSeatRecordRule
{
    public List<string> Check(FallbackLicenseSeatRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatRecordNameLengthRule(int MaxLength) : IFallbackLicenseSeatRecordRule
{
    public List<string> Check(FallbackLicenseSeatRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatRecordCreatedAtRangeRule(int Min, int Max) : IFallbackLicenseSeatRecordRule
{
    public List<string> Check(FallbackLicenseSeatRecord item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatRecordAllowedCodeRule(List<string> Allowed) : IFallbackLicenseSeatRecordRule
{
    public List<string> Check(FallbackLicenseSeatRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLicenseSeatRecordSourceLengthRule(int MaxLength) : IFallbackLicenseSeatRecordRule
{
    public List<string> Check(FallbackLicenseSeatRecord item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback license seat record.</summary>
public sealed record FallbackLicenseSeatRecordValidator(List<IFallbackLicenseSeatRecordRule> Rules)
{
    public List<string> Validate(FallbackLicenseSeatRecord item)
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

    public void RequireValid(FallbackLicenseSeatRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLicenseSeatRecord> items)
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

/// <summary>Functions of fallback license seat record rules.</summary>
public static class FallbackLicenseSeatRecordRules
{
    public static FallbackLicenseSeatRecordValidator DefaultFallbackLicenseSeatRecordValidator()
    {
        return new FallbackLicenseSeatRecordValidator(
            new List<IFallbackLicenseSeatRecordRule>
            {
                new FallbackLicenseSeatRecordCodePrefixRule("ACM"),
                new FallbackLicenseSeatRecordNameLengthRule(16),
                new FallbackLicenseSeatRecordCreatedAtRangeRule(64, 80),
                new FallbackLicenseSeatRecordAllowedCodeRule(new List<string> { "strict", "regional", "pending" }),
                new FallbackLicenseSeatRecordSourceLengthRule(16),
            });
    }
}
