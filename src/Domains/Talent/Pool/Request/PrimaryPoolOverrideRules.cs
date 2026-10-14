namespace Acme.Workforce.Talent.Pool.Request;

/// <summary>A rule a primary pool override must pass. Check returns its problems, or none.</summary>
public interface IPrimaryPoolOverrideRule
{
    List<string> Check(PrimaryPoolOverride item);
}

public sealed record PrimaryPoolOverrideChecksumLengthRule(int MaxLength) : IPrimaryPoolOverrideRule
{
    public List<string> Check(PrimaryPoolOverride item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPoolOverrideExpiresAtRangeRule(int Min, int Max) : IPrimaryPoolOverrideRule
{
    public List<string> Check(PrimaryPoolOverride item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPoolOverrideRequiredNameRule(string Message) : IPrimaryPoolOverrideRule
{
    public List<string> Check(PrimaryPoolOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPoolOverrideSalaryBandLengthRule(int MaxLength) : IPrimaryPoolOverrideRule
{
    public List<string> Check(PrimaryPoolOverride item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryPoolOverrideDurationMinutesRangeRule(int Min, int Max) : IPrimaryPoolOverrideRule
{
    public List<string> Check(PrimaryPoolOverride item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary pool override.</summary>
public sealed record PrimaryPoolOverrideValidator(List<IPrimaryPoolOverrideRule> Rules)
{
    public List<string> Validate(PrimaryPoolOverride item)
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

    public void RequireValid(PrimaryPoolOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryPoolOverride> items)
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

/// <summary>Functions of primary pool override rules.</summary>
public static class PrimaryPoolOverrideRules
{
    public static PrimaryPoolOverrideValidator DefaultPrimaryPoolOverrideValidator()
    {
        return new PrimaryPoolOverrideValidator(
            new List<IPrimaryPoolOverrideRule>
            {
                new PrimaryPoolOverrideChecksumLengthRule(64),
                new PrimaryPoolOverrideExpiresAtRangeRule(64, 80),
                new PrimaryPoolOverrideRequiredNameRule("primary pool override needs this field"),
                new PrimaryPoolOverrideSalaryBandLengthRule(255),
                new PrimaryPoolOverrideDurationMinutesRangeRule(32, 80),
            });
    }
}
