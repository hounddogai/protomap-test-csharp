namespace Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>A rule a lenient position segment must pass. Check returns its problems, or none.</summary>
public interface ILenientPositionSegmentRule
{
    List<string> Check(LenientPositionSegment item);
}

public sealed record LenientPositionSegmentRequiredDepartmentRule(string Message) : ILenientPositionSegmentRule
{
    public List<string> Check(LenientPositionSegment item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionSegmentRequiredCodeRule(string Message) : ILenientPositionSegmentRule
{
    public List<string> Check(LenientPositionSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionSegmentRequiredNameRule(string Message) : ILenientPositionSegmentRule
{
    public List<string> Check(LenientPositionSegment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientPositionSegmentNameLengthRule(int MaxLength) : ILenientPositionSegmentRule
{
    public List<string> Check(LenientPositionSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient position segment.</summary>
public sealed record LenientPositionSegmentValidator(List<ILenientPositionSegmentRule> Rules)
{
    public List<string> Validate(LenientPositionSegment item)
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

    public void RequireValid(LenientPositionSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientPositionSegment> items)
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

/// <summary>Functions of lenient position segment rules.</summary>
public static class LenientPositionSegmentRules
{
    public static LenientPositionSegmentValidator DefaultLenientPositionSegmentValidator()
    {
        return new LenientPositionSegmentValidator(
            new List<ILenientPositionSegmentRule>
            {
                new LenientPositionSegmentRequiredDepartmentRule("lenient position segment needs an entry"),
                new LenientPositionSegmentRequiredCodeRule("lenient position segment needs an entry"),
                new LenientPositionSegmentRequiredNameRule("lenient position segment needs an entry"),
                new LenientPositionSegmentNameLengthRule(16),
            });
    }
}
