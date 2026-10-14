namespace Acme.Workforce.Absence.Accrual.Group;

/// <summary>A rule a manual accrual entry must pass. Check returns its problems, or none.</summary>
public interface IManualAccrualEntryRule
{
    List<string> Check(ManualAccrualEntry item);
}

public sealed record ManualAccrualEntryRatingRangeRule(double Min, double Max) : IManualAccrualEntryRule
{
    public List<string> Check(ManualAccrualEntry item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAccrualEntryRequiredLabelRule(string Message) : IManualAccrualEntryRule
{
    public List<string> Check(ManualAccrualEntry item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualEntryLabelLengthRule(int MaxLength) : IManualAccrualEntryRule
{
    public List<string> Check(ManualAccrualEntry item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccrualEntryRequiredCodeRule(string Message) : IManualAccrualEntryRule
{
    public List<string> Check(ManualAccrualEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual accrual entry.</summary>
public sealed record ManualAccrualEntryValidator(List<IManualAccrualEntryRule> Rules)
{
    public List<string> Validate(ManualAccrualEntry item)
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

    public void RequireValid(ManualAccrualEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccrualEntry> items)
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

/// <summary>Functions of manual accrual entry rules.</summary>
public static class ManualAccrualEntryRules
{
    public static ManualAccrualEntryValidator DefaultManualAccrualEntryValidator()
    {
        return new ManualAccrualEntryValidator(
            new List<IManualAccrualEntryRule>
            {
                new ManualAccrualEntryRatingRangeRule(6.2, 1406.1),
                new ManualAccrualEntryRequiredLabelRule("manual accrual entry needs a value"),
                new ManualAccrualEntryLabelLengthRule(1000),
                new ManualAccrualEntryRequiredCodeRule("manual accrual entry needs a value"),
            });
    }
}
