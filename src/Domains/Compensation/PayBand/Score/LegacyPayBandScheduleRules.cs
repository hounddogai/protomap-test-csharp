namespace Acme.Workforce.Compensation.PayBand.Score;

/// <summary>A rule a legacy pay band schedule must pass. Check returns its problems, or none.</summary>
public interface ILegacyPayBandScheduleRule
{
    List<string> Check(LegacyPayBandSchedule item);
}

public sealed record LegacyPayBandScheduleCurrencyLengthRule(int MaxLength) : ILegacyPayBandScheduleRule
{
    public List<string> Check(LegacyPayBandSchedule item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandScheduleCodeLengthRule(int MaxLength) : ILegacyPayBandScheduleRule
{
    public List<string> Check(LegacyPayBandSchedule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandScheduleRequiredCurrencyRule(string Message) : ILegacyPayBandScheduleRule
{
    public List<string> Check(LegacyPayBandSchedule item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPayBandScheduleCodePrefixRule(string Prefix) : ILegacyPayBandScheduleRule
{
    public List<string> Check(LegacyPayBandSchedule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy pay band schedule.</summary>
public sealed record LegacyPayBandScheduleValidator(List<ILegacyPayBandScheduleRule> Rules)
{
    public List<string> Validate(LegacyPayBandSchedule item)
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

    public void RequireValid(LegacyPayBandSchedule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPayBandSchedule> items)
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

/// <summary>Functions of legacy pay band schedule rules.</summary>
public static class LegacyPayBandScheduleRules
{
    public static LegacyPayBandScheduleValidator DefaultLegacyPayBandScheduleValidator()
    {
        return new LegacyPayBandScheduleValidator(
            new List<ILegacyPayBandScheduleRule>
            {
                new LegacyPayBandScheduleCurrencyLengthRule(3600),
                new LegacyPayBandScheduleCodeLengthRule(80),
                new LegacyPayBandScheduleRequiredCurrencyRule("legacy pay band schedule needs a value"),
                new LegacyPayBandScheduleCodePrefixRule("X"),
            });
    }
}
