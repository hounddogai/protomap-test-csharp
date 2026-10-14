namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a desk snapshot must pass. Check returns its problems, or none.</summary>
public interface IDeskSnapshotRule
{
    List<string> Check(DeskSnapshot item);
}

public sealed record DeskSnapshotCostRangeRule(double Min, double Max) : IDeskSnapshotRule
{
    public List<string> Check(DeskSnapshot item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskSnapshotRequiredCodeRule(string Message) : IDeskSnapshotRule
{
    public List<string> Check(DeskSnapshot item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskSnapshotAllowedLabelRule(List<string> Allowed) : IDeskSnapshotRule
{
    public List<string> Check(DeskSnapshot item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record DeskSnapshotRequiredCurrencyRule(string Message) : IDeskSnapshotRule
{
    public List<string> Check(DeskSnapshot item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskSnapshotLimitRangeRule(int Min, int Max) : IDeskSnapshotRule
{
    public List<string> Check(DeskSnapshot item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeskSnapshotCurrencyLengthRule(int MaxLength) : IDeskSnapshotRule
{
    public List<string> Check(DeskSnapshot item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk snapshot.</summary>
public sealed record DeskSnapshotValidator(List<IDeskSnapshotRule> Rules)
{
    public List<string> Validate(DeskSnapshot item)
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

    public void RequireValid(DeskSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskSnapshot> items)
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

/// <summary>Functions of desk snapshot rules.</summary>
public static class DeskSnapshotRules
{
    public static DeskSnapshotValidator DefaultDeskSnapshotValidator()
    {
        return new DeskSnapshotValidator(
            new List<IDeskSnapshotRule>
            {
                new DeskSnapshotCostRangeRule(3.2, 3458.3),
                new DeskSnapshotRequiredCodeRule("desk snapshot needs a value"),
                new DeskSnapshotAllowedLabelRule(new List<string>()),
                new DeskSnapshotRequiredCurrencyRule("desk snapshot needs this field"),
                new DeskSnapshotLimitRangeRule(32, 1000),
                new DeskSnapshotCurrencyLengthRule(32),
            });
    }
}
