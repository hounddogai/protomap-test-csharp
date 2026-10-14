namespace Acme.Workforce.Workplace.Visitor.Score;

/// <summary>A rule a lenient visitor checkpoint must pass. Check returns its problems, or none.</summary>
public interface ILenientVisitorCheckpointRule
{
    List<string> Check(LenientVisitorCheckpoint item);
}

public sealed record LenientVisitorCheckpointRequiredNameRule(string Message) : ILenientVisitorCheckpointRule
{
    public List<string> Check(LenientVisitorCheckpoint item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorCheckpointCodeLengthRule(int MaxLength) : ILenientVisitorCheckpointRule
{
    public List<string> Check(LenientVisitorCheckpoint item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientVisitorCheckpointCodePrefixRule(string Prefix) : ILenientVisitorCheckpointRule
{
    public List<string> Check(LenientVisitorCheckpoint item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient visitor checkpoint.</summary>
public sealed record LenientVisitorCheckpointValidator(List<ILenientVisitorCheckpointRule> Rules)
{
    public List<string> Validate(LenientVisitorCheckpoint item)
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

    public void RequireValid(LenientVisitorCheckpoint item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientVisitorCheckpoint> items)
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

/// <summary>Functions of lenient visitor checkpoint rules.</summary>
public static class LenientVisitorCheckpointRules
{
    public static LenientVisitorCheckpointValidator DefaultLenientVisitorCheckpointValidator()
    {
        return new LenientVisitorCheckpointValidator(
            new List<ILenientVisitorCheckpointRule>
            {
                new LenientVisitorCheckpointRequiredNameRule("lenient visitor checkpoint needs this field"),
                new LenientVisitorCheckpointCodeLengthRule(86400),
                new LenientVisitorCheckpointCodePrefixRule("REF"),
            });
    }
}
