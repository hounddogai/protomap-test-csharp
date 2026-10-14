namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>A rule a legacy exception profile must pass. Check returns its problems, or none.</summary>
public interface ILegacyExceptionProfileRule
{
    List<string> Check(LegacyExceptionProfile item);
}

public sealed record LegacyExceptionProfileLabelLengthRule(int MaxLength) : ILegacyExceptionProfileRule
{
    public List<string> Check(LegacyExceptionProfile item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyExceptionProfileArchivedGuardRule(int Limit) : ILegacyExceptionProfileRule
{
    public List<string> Check(LegacyExceptionProfile item)
    {
        if (item.Archived && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record LegacyExceptionProfileRequiredCodeRule(string Message) : ILegacyExceptionProfileRule
{
    public List<string> Check(LegacyExceptionProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy exception profile.</summary>
public sealed record LegacyExceptionProfileValidator(List<ILegacyExceptionProfileRule> Rules)
{
    public List<string> Validate(LegacyExceptionProfile item)
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

    public void RequireValid(LegacyExceptionProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyExceptionProfile> items)
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

/// <summary>Functions of legacy exception profile rules.</summary>
public static class LegacyExceptionProfileRules
{
    public static LegacyExceptionProfileValidator DefaultLegacyExceptionProfileValidator()
    {
        return new LegacyExceptionProfileValidator(
            new List<ILegacyExceptionProfileRule>
            {
                new LegacyExceptionProfileLabelLengthRule(120),
                new LegacyExceptionProfileArchivedGuardRule(120),
                new LegacyExceptionProfileRequiredCodeRule("legacy exception profile needs an entry"),
            });
    }
}
