namespace Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>A rule a exception snapshot must pass. Check returns its problems, or none.</summary>
public interface IExceptionSnapshotRule
{
    List<string> Check(ExceptionSnapshot item);
}

public sealed record ExceptionSnapshotCreatedAtRangeRule(int Min, int Max) : IExceptionSnapshotRule
{
    public List<string> Check(ExceptionSnapshot item)
    {
        var valueValue = item.CreatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"created at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"created at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionSnapshotRequiredEmployeeIdRule(string Message) : IExceptionSnapshotRule
{
    public List<string> Check(ExceptionSnapshot item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSnapshotLeaveDaysRangeRule(int Min, int Max) : IExceptionSnapshotRule
{
    public List<string> Check(ExceptionSnapshot item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ExceptionSnapshotRequiredPhoneRule(string Message) : IExceptionSnapshotRule
{
    public List<string> Check(ExceptionSnapshot item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSnapshotCodePrefixRule(string Prefix) : IExceptionSnapshotRule
{
    public List<string> Check(ExceptionSnapshot item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ExceptionSnapshotCodeLengthRule(int MaxLength) : IExceptionSnapshotRule
{
    public List<string> Check(ExceptionSnapshot item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a exception snapshot.</summary>
public sealed record ExceptionSnapshotValidator(List<IExceptionSnapshotRule> Rules)
{
    public List<string> Validate(ExceptionSnapshot item)
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

    public void RequireValid(ExceptionSnapshot item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ExceptionSnapshot> items)
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

/// <summary>Functions of exception snapshot rules.</summary>
public static class ExceptionSnapshotRules
{
    public static ExceptionSnapshotValidator DefaultExceptionSnapshotValidator()
    {
        return new ExceptionSnapshotValidator(
            new List<IExceptionSnapshotRule>
            {
                new ExceptionSnapshotCreatedAtRangeRule(64, 86400),
                new ExceptionSnapshotRequiredEmployeeIdRule("exception snapshot needs an entry"),
                new ExceptionSnapshotLeaveDaysRangeRule(80, 1000),
                new ExceptionSnapshotRequiredPhoneRule("exception snapshot needs this field"),
                new ExceptionSnapshotCodePrefixRule("ACM"),
                new ExceptionSnapshotCodeLengthRule(80),
            });
    }
}
