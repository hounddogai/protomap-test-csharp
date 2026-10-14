namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual segment must pass. Check returns its problems, or none.</summary>
public interface IAccrualSegmentRule
{
    List<string> Check(AccrualSegment item);
}

public sealed record AccrualSegmentSourceLengthRule(int MaxLength) : IAccrualSegmentRule
{
    public List<string> Check(AccrualSegment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSegmentAllowedNotesRule(List<string> Allowed) : IAccrualSegmentRule
{
    public List<string> Check(AccrualSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record AccrualSegmentRequiredCodeRule(string Message) : IAccrualSegmentRule
{
    public List<string> Check(AccrualSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual segment.</summary>
public sealed record AccrualSegmentValidator(List<IAccrualSegmentRule> Rules)
{
    public List<string> Validate(AccrualSegment item)
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

    public void RequireValid(AccrualSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualSegment> items)
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

/// <summary>Functions of accrual segment rules.</summary>
public static class AccrualSegmentRules
{
    public static AccrualSegmentValidator DefaultAccrualSegmentValidator()
    {
        return new AccrualSegmentValidator(
            new List<IAccrualSegmentRule>
            {
                new AccrualSegmentSourceLengthRule(3600),
                new AccrualSegmentAllowedNotesRule(new List<string> { "capped", "adaptive", "adaptive" }),
                new AccrualSegmentRequiredCodeRule("accrual segment needs an entry"),
            });
    }
}
