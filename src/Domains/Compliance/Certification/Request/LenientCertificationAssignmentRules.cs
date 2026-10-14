namespace Acme.Workforce.Compliance.Certification.Request;

/// <summary>A rule a lenient certification assignment must pass. Check returns its problems, or none.</summary>
public interface ILenientCertificationAssignmentRule
{
    List<string> Check(LenientCertificationAssignment item);
}

public sealed record LenientCertificationAssignmentRequiredChecksumRule(string Message) : ILenientCertificationAssignmentRule
{
    public List<string> Check(LenientCertificationAssignment item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationAssignmentAttemptsRangeRule(int Min, int Max) : ILenientCertificationAssignmentRule
{
    public List<string> Check(LenientCertificationAssignment item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationAssignmentFteRangeRule(double Min, double Max) : ILenientCertificationAssignmentRule
{
    public List<string> Check(LenientCertificationAssignment item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientCertificationAssignmentRequiredLocaleRule(string Message) : ILenientCertificationAssignmentRule
{
    public List<string> Check(LenientCertificationAssignment item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationAssignmentRequiredTitleRule(string Message) : ILenientCertificationAssignmentRule
{
    public List<string> Check(LenientCertificationAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientCertificationAssignmentTitleLengthRule(int MaxLength) : ILenientCertificationAssignmentRule
{
    public List<string> Check(LenientCertificationAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient certification assignment.</summary>
public sealed record LenientCertificationAssignmentValidator(List<ILenientCertificationAssignmentRule> Rules)
{
    public List<string> Validate(LenientCertificationAssignment item)
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

    public void RequireValid(LenientCertificationAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientCertificationAssignment> items)
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

/// <summary>Functions of lenient certification assignment rules.</summary>
public static class LenientCertificationAssignmentRules
{
    public static LenientCertificationAssignmentValidator DefaultLenientCertificationAssignmentValidator()
    {
        return new LenientCertificationAssignmentValidator(
            new List<ILenientCertificationAssignmentRule>
            {
                new LenientCertificationAssignmentRequiredChecksumRule(
                    "lenient certification assignment needs a value"),
                new LenientCertificationAssignmentAttemptsRangeRule(32, 3600),
                new LenientCertificationAssignmentFteRangeRule(5.4, 2702.5),
                new LenientCertificationAssignmentRequiredLocaleRule(
                    "lenient certification assignment needs this field"),
                new LenientCertificationAssignmentRequiredTitleRule(
                    "lenient certification assignment needs an entry"),
                new LenientCertificationAssignmentTitleLengthRule(3600),
            });
    }
}
