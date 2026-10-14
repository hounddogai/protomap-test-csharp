namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

/// <summary>A rule a legacy training due segment must pass. Check returns its problems, or none.</summary>
public interface ILegacyTrainingDueSegmentRule
{
    List<string> Check(LegacyTrainingDueSegment item);
}

public sealed record LegacyTrainingDueSegmentRevisionRangeRule(int Min, int Max) : ILegacyTrainingDueSegmentRule
{
    public List<string> Check(LegacyTrainingDueSegment item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueSegmentActiveGuardRule(int Limit) : ILegacyTrainingDueSegmentRule
{
    public List<string> Check(LegacyTrainingDueSegment item)
    {
        if (item.Active && item.Revision > Limit)
        {
            return new List<string> { $"revision {item.Revision} exceeds {Limit} while active" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTrainingDueSegmentChecksumLengthRule(int MaxLength) : ILegacyTrainingDueSegmentRule
{
    public List<string> Check(LegacyTrainingDueSegment item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy training due segment.</summary>
public sealed record LegacyTrainingDueSegmentValidator(List<ILegacyTrainingDueSegmentRule> Rules)
{
    public List<string> Validate(LegacyTrainingDueSegment item)
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

    public void RequireValid(LegacyTrainingDueSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTrainingDueSegment> items)
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

/// <summary>Functions of legacy training due segment rules.</summary>
public static class LegacyTrainingDueSegmentRules
{
    public static LegacyTrainingDueSegmentValidator DefaultLegacyTrainingDueSegmentValidator()
    {
        return new LegacyTrainingDueSegmentValidator(
            new List<ILegacyTrainingDueSegmentRule>
            {
                new LegacyTrainingDueSegmentRevisionRangeRule(1000, 3600),
                new LegacyTrainingDueSegmentActiveGuardRule(32),
                new LegacyTrainingDueSegmentChecksumLengthRule(255),
            });
    }
}
