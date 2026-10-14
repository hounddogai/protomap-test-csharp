namespace Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>A rule a global mobility record must pass. Check returns its problems, or none.</summary>
public interface IGlobalMobilityRecordRule
{
    List<string> Check(GlobalMobilityRecord item);
}

public sealed record GlobalMobilityRecordAmountRangeRule(double Min, double Max) : IGlobalMobilityRecordRule
{
    public List<string> Check(GlobalMobilityRecord item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMobilityRecordManagerIdLengthRule(int MaxLength) : IGlobalMobilityRecordRule
{
    public List<string> Check(GlobalMobilityRecord item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMobilityRecordReviewerLengthRule(int MaxLength) : IGlobalMobilityRecordRule
{
    public List<string> Check(GlobalMobilityRecord item)
    {
        var size = item.Reviewer.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"reviewer is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMobilityRecordCodePrefixRule(string Prefix) : IGlobalMobilityRecordRule
{
    public List<string> Check(GlobalMobilityRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalMobilityRecordTitleLengthRule(int MaxLength) : IGlobalMobilityRecordRule
{
    public List<string> Check(GlobalMobilityRecord item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global mobility record.</summary>
public sealed record GlobalMobilityRecordValidator(List<IGlobalMobilityRecordRule> Rules)
{
    public List<string> Validate(GlobalMobilityRecord item)
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

    public void RequireValid(GlobalMobilityRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalMobilityRecord> items)
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

/// <summary>Functions of global mobility record rules.</summary>
public static class GlobalMobilityRecordRules
{
    public static GlobalMobilityRecordValidator DefaultGlobalMobilityRecordValidator()
    {
        return new GlobalMobilityRecordValidator(
            new List<IGlobalMobilityRecordRule>
            {
                new GlobalMobilityRecordAmountRangeRule(6.3, 3311.6),
                new GlobalMobilityRecordManagerIdLengthRule(120),
                new GlobalMobilityRecordReviewerLengthRule(64),
                new GlobalMobilityRecordCodePrefixRule("Q"),
                new GlobalMobilityRecordTitleLengthRule(255),
            });
    }
}
