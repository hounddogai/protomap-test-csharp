namespace Acme.Workforce.Compliance.TrainingDue.Tier;

/// <summary>A rule a lenient training due tier must pass. Check returns its problems, or none.</summary>
public interface ILenientTrainingDueTierRule
{
    List<string> Check(LenientTrainingDueTier item);
}

public sealed record LenientTrainingDueTierTitleLengthRule(int MaxLength) : ILenientTrainingDueTierRule
{
    public List<string> Check(LenientTrainingDueTier item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTrainingDueTierLocationLengthRule(int MaxLength) : ILenientTrainingDueTierRule
{
    public List<string> Check(LenientTrainingDueTier item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientTrainingDueTierCodePrefixRule(string Prefix) : ILenientTrainingDueTierRule
{
    public List<string> Check(LenientTrainingDueTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient training due tier.</summary>
public sealed record LenientTrainingDueTierValidator(List<ILenientTrainingDueTierRule> Rules)
{
    public List<string> Validate(LenientTrainingDueTier item)
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

    public void RequireValid(LenientTrainingDueTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientTrainingDueTier> items)
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

/// <summary>Functions of lenient training due tier rules.</summary>
public static class LenientTrainingDueTierRules
{
    public static LenientTrainingDueTierValidator DefaultLenientTrainingDueTierValidator()
    {
        return new LenientTrainingDueTierValidator(
            new List<ILenientTrainingDueTierRule>
            {
                new LenientTrainingDueTierTitleLengthRule(64),
                new LenientTrainingDueTierLocationLengthRule(120),
                new LenientTrainingDueTierCodePrefixRule("REF"),
            });
    }
}
