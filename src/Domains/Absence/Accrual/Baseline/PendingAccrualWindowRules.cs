namespace Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>A rule a pending accrual window must pass. Check returns its problems, or none.</summary>
public interface IPendingAccrualWindowRule
{
    List<string> Check(PendingAccrualWindow item);
}

public sealed record PendingAccrualWindowRequiredCodeRule(string Message) : IPendingAccrualWindowRule
{
    public List<string> Check(PendingAccrualWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAccrualWindowStartsAtRangeRule(int Min, int Max) : IPendingAccrualWindowRule
{
    public List<string> Check(PendingAccrualWindow item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingAccrualWindowCodeLengthRule(int MaxLength) : IPendingAccrualWindowRule
{
    public List<string> Check(PendingAccrualWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAccrualWindowSalaryBandLengthRule(int MaxLength) : IPendingAccrualWindowRule
{
    public List<string> Check(PendingAccrualWindow item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending accrual window.</summary>
public sealed record PendingAccrualWindowValidator(List<IPendingAccrualWindowRule> Rules)
{
    public List<string> Validate(PendingAccrualWindow item)
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

    public void RequireValid(PendingAccrualWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAccrualWindow> items)
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

/// <summary>Functions of pending accrual window rules.</summary>
public static class PendingAccrualWindowRules
{
    public static PendingAccrualWindowValidator DefaultPendingAccrualWindowValidator()
    {
        return new PendingAccrualWindowValidator(
            new List<IPendingAccrualWindowRule>
            {
                new PendingAccrualWindowRequiredCodeRule("pending accrual window needs an entry"),
                new PendingAccrualWindowStartsAtRangeRule(32, 86400),
                new PendingAccrualWindowCodeLengthRule(16),
                new PendingAccrualWindowSalaryBandLengthRule(3600),
            });
    }
}
