namespace Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>A rule a weighted visitor entry must pass. Check returns its problems, or none.</summary>
public interface IWeightedVisitorEntryRule
{
    List<string> Check(WeightedVisitorEntry item);
}

public sealed record WeightedVisitorEntryRequiredPhoneRule(string Message) : IWeightedVisitorEntryRule
{
    public List<string> Check(WeightedVisitorEntry item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedVisitorEntryCodeLengthRule(int MaxLength) : IWeightedVisitorEntryRule
{
    public List<string> Check(WeightedVisitorEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedVisitorEntryPhoneLengthRule(int MaxLength) : IWeightedVisitorEntryRule
{
    public List<string> Check(WeightedVisitorEntry item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted visitor entry.</summary>
public sealed record WeightedVisitorEntryValidator(List<IWeightedVisitorEntryRule> Rules)
{
    public List<string> Validate(WeightedVisitorEntry item)
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

    public void RequireValid(WeightedVisitorEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedVisitorEntry> items)
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

/// <summary>Functions of weighted visitor entry rules.</summary>
public static class WeightedVisitorEntryRules
{
    public static WeightedVisitorEntryValidator DefaultWeightedVisitorEntryValidator()
    {
        return new WeightedVisitorEntryValidator(
            new List<IWeightedVisitorEntryRule>
            {
                new WeightedVisitorEntryRequiredPhoneRule("weighted visitor entry needs this field"),
                new WeightedVisitorEntryCodeLengthRule(16),
                new WeightedVisitorEntryPhoneLengthRule(1000),
            });
    }
}
