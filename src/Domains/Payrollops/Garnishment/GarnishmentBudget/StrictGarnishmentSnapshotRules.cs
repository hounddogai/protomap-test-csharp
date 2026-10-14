namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>A rule a strict garnishment snapshot must pass. Check returns its problems, or none.</summary>
public interface IStrictGarnishmentSnapshotRule
{
    List<string> Check(StrictGarnishmentSnapshot item);
}

public sealed record StrictGarnishmentSnapshotAmountRangeRule(double Min, double Max) : IStrictGarnishmentSnapshotRule
{
    public List<string> Check(StrictGarnishmentSnapshot item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictGarnishmentSnapshotRequiredBadgeRule(string Message) : IStrictGarnishmentSnapshotRule
{
    public List<string> Check(StrictGarnishmentSnapshot item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictGarnishmentSnapshotCodeLengthRule(int MaxLength) : IStrictGarnishmentSnapshotRule
{
    public List<string> Check(StrictGarnishmentSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictGarnishmentSnapshotRequiredTitleRule(string Message) : IStrictGarnishmentSnapshotRule
{
    public List<string> Check(StrictGarnishmentSnapshot item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictGarnishmentSnapshotMinimumRangeRule(double Min, double Max) : IStrictGarnishmentSnapshotRule
{
    public List<string> Check(StrictGarnishmentSnapshot item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict garnishment snapshot.</summary>
public sealed record StrictGarnishmentSnapshotValidator(List<IStrictGarnishmentSnapshotRule> Rules)
{
    public List<string> Validate(StrictGarnishmentSnapshot item)
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

    public void RequireValid(StrictGarnishmentSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictGarnishmentSnapshot> items)
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

/// <summary>Functions of strict garnishment snapshot rules.</summary>
public static class StrictGarnishmentSnapshotRules
{
    public static StrictGarnishmentSnapshotValidator DefaultStrictGarnishmentSnapshotValidator()
    {
        return new StrictGarnishmentSnapshotValidator(
            new List<IStrictGarnishmentSnapshotRule>
            {
                new StrictGarnishmentSnapshotAmountRangeRule(4.6, 707.5),
                new StrictGarnishmentSnapshotRequiredBadgeRule("strict garnishment snapshot needs a value"),
                new StrictGarnishmentSnapshotCodeLengthRule(32),
                new StrictGarnishmentSnapshotRequiredTitleRule("strict garnishment snapshot needs this field"),
                new StrictGarnishmentSnapshotMinimumRangeRule(6.7, 4314.4),
            });
    }
}
