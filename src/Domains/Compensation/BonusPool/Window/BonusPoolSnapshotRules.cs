namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>A rule a bonus pool snapshot must pass. Check returns its problems, or none.</summary>
public interface IBonusPoolSnapshotRule
{
    List<string> Check(BonusPoolSnapshot item);
}

public sealed record BonusPoolSnapshotLeaveDaysRangeRule(int Min, int Max) : IBonusPoolSnapshotRule
{
    public List<string> Check(BonusPoolSnapshot item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolSnapshotRequiredNameRule(string Message) : IBonusPoolSnapshotRule
{
    public List<string> Check(BonusPoolSnapshot item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolSnapshotVisibleGuardRule(double Limit) : IBonusPoolSnapshotRule
{
    public List<string> Check(BonusPoolSnapshot item)
    {
        if (item.Visible && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record BonusPoolSnapshotQuantityRangeRule(int Min, int Max) : IBonusPoolSnapshotRule
{
    public List<string> Check(BonusPoolSnapshot item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BonusPoolSnapshotRateRangeRule(double Min, double Max) : IBonusPoolSnapshotRule
{
    public List<string> Check(BonusPoolSnapshot item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a bonus pool snapshot.</summary>
public sealed record BonusPoolSnapshotValidator(List<IBonusPoolSnapshotRule> Rules)
{
    public List<string> Validate(BonusPoolSnapshot item)
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

    public void RequireValid(BonusPoolSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BonusPoolSnapshot> items)
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

/// <summary>Functions of bonus pool snapshot rules.</summary>
public static class BonusPoolSnapshotRules
{
    public static BonusPoolSnapshotValidator DefaultBonusPoolSnapshotValidator()
    {
        return new BonusPoolSnapshotValidator(
            new List<IBonusPoolSnapshotRule>
            {
                new BonusPoolSnapshotLeaveDaysRangeRule(32, 3600),
                new BonusPoolSnapshotRequiredNameRule("bonus pool snapshot needs an entry"),
                new BonusPoolSnapshotVisibleGuardRule(2.7),
                new BonusPoolSnapshotQuantityRangeRule(32, 255),
                new BonusPoolSnapshotRateRangeRule(9.7, 3527.6),
            });
    }
}
