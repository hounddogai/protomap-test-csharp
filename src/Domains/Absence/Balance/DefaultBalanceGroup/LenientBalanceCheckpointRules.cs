namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

/// <summary>A rule a lenient balance checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILenientBalanceCheckpointRule
{
    List<string> Check(LenientBalanceCheckpoint item);
}

public sealed record LenientBalanceCheckpointCodePrefixRule(string Prefix) : ILenientBalanceCheckpointRule
{
    public List<string> Check(LenientBalanceCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceCheckpointCodeLengthRule(int MaxLength) : ILenientBalanceCheckpointRule
{
    public List<string> Check(LenientBalanceCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceCheckpointRequiredEmployeeIdRule(string Message) : ILenientBalanceCheckpointRule
{
    public List<string> Check(LenientBalanceCheckpoint item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceCheckpointEmployeeIdLengthRule(int MaxLength) : ILenientBalanceCheckpointRule
{
    public List<string> Check(LenientBalanceCheckpoint item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientBalanceCheckpointRequiredCodeRule(string Message) : ILenientBalanceCheckpointRule
{
    public List<string> Check(LenientBalanceCheckpoint item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient balance checkpoint.</summary>
public sealed record LenientBalanceCheckpointValidator(List<ILenientBalanceCheckpointRule> Rules)
{
    public List<string> Validate(LenientBalanceCheckpoint item)
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

    public void RequireValid(LenientBalanceCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientBalanceCheckpoint> items)
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

/// <summary>Functions of lenient balance checkpoint rules.</summary>
public static class LenientBalanceCheckpointRules
{
    public static LenientBalanceCheckpointValidator DefaultLenientBalanceCheckpointValidator()
    {
        return new LenientBalanceCheckpointValidator(
            new List<ILenientBalanceCheckpointRule>
            {
                new LenientBalanceCheckpointCodePrefixRule("OPS"),
                new LenientBalanceCheckpointCodeLengthRule(255),
                new LenientBalanceCheckpointRequiredEmployeeIdRule("lenient balance checkpoint needs an entry"),
                new LenientBalanceCheckpointEmployeeIdLengthRule(255),
                new LenientBalanceCheckpointRequiredCodeRule("lenient balance checkpoint needs this field"),
            });
    }
}
