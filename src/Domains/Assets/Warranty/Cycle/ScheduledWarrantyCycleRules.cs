namespace Acme.Workforce.Assets.Warranty.Cycle;

/// <summary>A rule a scheduled warranty cycle must pass. Check returns its problems, or none.</summary>
public interface IScheduledWarrantyCycleRule
{
    List<string> Check(ScheduledWarrantyCycle item);
}

public sealed record ScheduledWarrantyCycleRequiredNameRule(string Message) : IScheduledWarrantyCycleRule
{
    public List<string> Check(ScheduledWarrantyCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyCycleCodePrefixRule(string Prefix) : IScheduledWarrantyCycleRule
{
    public List<string> Check(ScheduledWarrantyCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyCycleCodeLengthRule(int MaxLength) : IScheduledWarrantyCycleRule
{
    public List<string> Check(ScheduledWarrantyCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyCycleRequiredChecksumRule(string Message) : IScheduledWarrantyCycleRule
{
    public List<string> Check(ScheduledWarrantyCycle item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyCycleNameLengthRule(int MaxLength) : IScheduledWarrantyCycleRule
{
    public List<string> Check(ScheduledWarrantyCycle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScheduledWarrantyCycleChecksumLengthRule(int MaxLength) : IScheduledWarrantyCycleRule
{
    public List<string> Check(ScheduledWarrantyCycle item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scheduled warranty cycle.</summary>
public sealed record ScheduledWarrantyCycleValidator(List<IScheduledWarrantyCycleRule> Rules)
{
    public List<string> Validate(ScheduledWarrantyCycle item)
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

    public void RequireValid(ScheduledWarrantyCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScheduledWarrantyCycle> items)
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

/// <summary>Functions of scheduled warranty cycle rules.</summary>
public static class ScheduledWarrantyCycleRules
{
    public static ScheduledWarrantyCycleValidator DefaultScheduledWarrantyCycleValidator()
    {
        return new ScheduledWarrantyCycleValidator(
            new List<IScheduledWarrantyCycleRule>
            {
                new ScheduledWarrantyCycleRequiredNameRule("scheduled warranty cycle needs a value"),
                new ScheduledWarrantyCycleCodePrefixRule("ACM"),
                new ScheduledWarrantyCycleCodeLengthRule(255),
                new ScheduledWarrantyCycleRequiredChecksumRule("scheduled warranty cycle needs an entry"),
                new ScheduledWarrantyCycleNameLengthRule(1000),
                new ScheduledWarrantyCycleChecksumLengthRule(64),
            });
    }
}
