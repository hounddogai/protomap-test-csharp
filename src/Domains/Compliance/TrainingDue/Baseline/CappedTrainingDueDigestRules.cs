namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>A rule a capped training due digest must pass. Check returns its problems, or none.</summary>
public interface ICappedTrainingDueDigestRule
{
    List<string> Check(CappedTrainingDueDigest item);
}

public sealed record CappedTrainingDueDigestRequiredNameRule(string Message) : ICappedTrainingDueDigestRule
{
    public List<string> Check(CappedTrainingDueDigest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedTrainingDueDigestTeamLengthRule(int MaxLength) : ICappedTrainingDueDigestRule
{
    public List<string> Check(CappedTrainingDueDigest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedTrainingDueDigestApprovedGuardRule(int Limit) : ICappedTrainingDueDigestRule
{
    public List<string> Check(CappedTrainingDueDigest item)
    {
        if (item.Approved && item.StartsAt > Limit)
        {
            return new List<string> { $"starts at {item.StartsAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped training due digest.</summary>
public sealed record CappedTrainingDueDigestValidator(List<ICappedTrainingDueDigestRule> Rules)
{
    public List<string> Validate(CappedTrainingDueDigest item)
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

    public void RequireValid(CappedTrainingDueDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedTrainingDueDigest> items)
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

/// <summary>Functions of capped training due digest rules.</summary>
public static class CappedTrainingDueDigestRules
{
    public static CappedTrainingDueDigestValidator DefaultCappedTrainingDueDigestValidator()
    {
        return new CappedTrainingDueDigestValidator(
            new List<ICappedTrainingDueDigestRule>
            {
                new CappedTrainingDueDigestRequiredNameRule("capped training due digest needs a value"),
                new CappedTrainingDueDigestTeamLengthRule(86400),
                new CappedTrainingDueDigestApprovedGuardRule(16),
            });
    }
}
