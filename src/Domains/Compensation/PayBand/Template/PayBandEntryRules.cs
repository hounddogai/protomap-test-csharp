namespace Acme.Workforce.Compensation.PayBand.Template;

/// <summary>A rule a pay band entry must pass. Check returns its problems, or none.</summary>
public interface IPayBandEntryRule
{
    List<string> Check(PayBandEntry item);
}

public sealed record PayBandEntryCodeLengthRule(int MaxLength) : IPayBandEntryRule
{
    public List<string> Check(PayBandEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandEntryRetriesRangeRule(int Min, int Max) : IPayBandEntryRule
{
    public List<string> Check(PayBandEntry item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PayBandEntryChecksumLengthRule(int MaxLength) : IPayBandEntryRule
{
    public List<string> Check(PayBandEntry item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pay band entry.</summary>
public sealed record PayBandEntryValidator(List<IPayBandEntryRule> Rules)
{
    public List<string> Validate(PayBandEntry item)
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

    public void RequireValid(PayBandEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PayBandEntry> items)
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

/// <summary>Functions of pay band entry rules.</summary>
public static class PayBandEntryRules
{
    public static PayBandEntryValidator DefaultPayBandEntryValidator()
    {
        return new PayBandEntryValidator(
            new List<IPayBandEntryRule>
            {
                new PayBandEntryCodeLengthRule(120),
                new PayBandEntryRetriesRangeRule(1000, 3600),
                new PayBandEntryChecksumLengthRule(3600),
            });
    }
}
