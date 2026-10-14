namespace Acme.Workforce.Talent.Mobility.Entry;

/// <summary>A rule a mobility entry must pass. Check returns its problems, or none.</summary>
public interface IMobilityEntryRule
{
    List<string> Check(MobilityEntry item);
}

public sealed record MobilityEntryBudgetRangeRule(double Min, double Max) : IMobilityEntryRule
{
    public List<string> Check(MobilityEntry item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityEntryTitleLengthRule(int MaxLength) : IMobilityEntryRule
{
    public List<string> Check(MobilityEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record MobilityEntryCodeLengthRule(int MaxLength) : IMobilityEntryRule
{
    public List<string> Check(MobilityEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a mobility entry.</summary>
public sealed record MobilityEntryValidator(List<IMobilityEntryRule> Rules)
{
    public List<string> Validate(MobilityEntry item)
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

    public void RequireValid(MobilityEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityEntry> items)
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

/// <summary>Functions of mobility entry rules.</summary>
public static class MobilityEntryRules
{
    public static MobilityEntryValidator DefaultMobilityEntryValidator()
    {
        return new MobilityEntryValidator(
            new List<IMobilityEntryRule>
            {
                new MobilityEntryBudgetRangeRule(3.7, 4386.9),
                new MobilityEntryTitleLengthRule(16),
                new MobilityEntryCodeLengthRule(1000),
            });
    }
}
