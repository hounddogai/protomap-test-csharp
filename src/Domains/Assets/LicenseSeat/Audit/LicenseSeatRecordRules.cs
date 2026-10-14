namespace Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>A rule a license seat record must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatRecordRule
{
    List<string> Check(LicenseSeatRecord item);
}

public sealed record LicenseSeatRecordPhoneLengthRule(int MaxLength) : ILicenseSeatRecordRule
{
    public List<string> Check(LicenseSeatRecord item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRecordNameLengthRule(int MaxLength) : ILicenseSeatRecordRule
{
    public List<string> Check(LicenseSeatRecord item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRecordCodePrefixRule(string Prefix) : ILicenseSeatRecordRule
{
    public List<string> Check(LicenseSeatRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRecordWindowDaysRangeRule(int Min, int Max) : ILicenseSeatRecordRule
{
    public List<string> Check(LicenseSeatRecord item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatRecordCodeLengthRule(int MaxLength) : ILicenseSeatRecordRule
{
    public List<string> Check(LicenseSeatRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatRecordRequiredCodeRule(string Message) : ILicenseSeatRecordRule
{
    public List<string> Check(LicenseSeatRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat record.</summary>
public sealed record LicenseSeatRecordValidator(List<ILicenseSeatRecordRule> Rules)
{
    public List<string> Validate(LicenseSeatRecord item)
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

    public void RequireValid(LicenseSeatRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatRecord> items)
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

/// <summary>Functions of license seat record rules.</summary>
public static class LicenseSeatRecordRules
{
    public static LicenseSeatRecordValidator DefaultLicenseSeatRecordValidator()
    {
        return new LicenseSeatRecordValidator(
            new List<ILicenseSeatRecordRule>
            {
                new LicenseSeatRecordPhoneLengthRule(80),
                new LicenseSeatRecordNameLengthRule(255),
                new LicenseSeatRecordCodePrefixRule("REF"),
                new LicenseSeatRecordWindowDaysRangeRule(16, 120),
                new LicenseSeatRecordCodeLengthRule(1000),
                new LicenseSeatRecordRequiredCodeRule("license seat record needs this field"),
            });
    }
}
