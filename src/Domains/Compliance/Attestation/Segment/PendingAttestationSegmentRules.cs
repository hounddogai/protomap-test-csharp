namespace Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>A rule a pending attestation segment must pass. Check returns its problems, or none.</summary>
public interface IPendingAttestationSegmentRule
{
    List<string> Check(PendingAttestationSegment item);
}

public sealed record PendingAttestationSegmentRequiredTitleRule(string Message) : IPendingAttestationSegmentRule
{
    public List<string> Check(PendingAttestationSegment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationSegmentCodeLengthRule(int MaxLength) : IPendingAttestationSegmentRule
{
    public List<string> Check(PendingAttestationSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationSegmentPinnedGuardRule(int Limit) : IPendingAttestationSegmentRule
{
    public List<string> Check(PendingAttestationSegment item)
    {
        if (item.Pinned && item.WindowDays > Limit)
        {
            return new List<string> { $"window days {item.WindowDays} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record PendingAttestationSegmentTitleLengthRule(int MaxLength) : IPendingAttestationSegmentRule
{
    public List<string> Check(PendingAttestationSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending attestation segment.</summary>
public sealed record PendingAttestationSegmentValidator(List<IPendingAttestationSegmentRule> Rules)
{
    public List<string> Validate(PendingAttestationSegment item)
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

    public void RequireValid(PendingAttestationSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAttestationSegment> items)
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

/// <summary>Functions of pending attestation segment rules.</summary>
public static class PendingAttestationSegmentRules
{
    public static PendingAttestationSegmentValidator DefaultPendingAttestationSegmentValidator()
    {
        return new PendingAttestationSegmentValidator(
            new List<IPendingAttestationSegmentRule>
            {
                new PendingAttestationSegmentRequiredTitleRule("pending attestation segment needs an entry"),
                new PendingAttestationSegmentCodeLengthRule(3600),
                new PendingAttestationSegmentPinnedGuardRule(80),
                new PendingAttestationSegmentTitleLengthRule(3600),
            });
    }
}
