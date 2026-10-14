namespace Acme.Workforce.Absence.Accrual.Segment;

/// <summary>A rule a capped accrual score must pass. Check returns its problems, or none.</summary>
public interface ICappedAccrualScoreRule
{
    List<string> Check(CappedAccrualScore item);
}

public sealed record CappedAccrualScoreBatchSizeRangeRule(int Min, int Max) : ICappedAccrualScoreRule
{
    public List<string> Check(CappedAccrualScore item)
    {
        var valueValue = item.BatchSize;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"batch size {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"batch size {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAccrualScoreWindowRule(int MaxSpan) : ICappedAccrualScoreRule
{
    public List<string> Check(CappedAccrualScore item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccrualScoreFloorValueRangeRule(double Min, double Max) : ICappedAccrualScoreRule
{
    public List<string> Check(CappedAccrualScore item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CappedAccrualScoreLabelLengthRule(int MaxLength) : ICappedAccrualScoreRule
{
    public List<string> Check(CappedAccrualScore item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedAccrualScoreRequiredSegmentRule(string Message) : ICappedAccrualScoreRule
{
    public List<string> Check(CappedAccrualScore item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped accrual score.</summary>
public sealed record CappedAccrualScoreValidator(List<ICappedAccrualScoreRule> Rules)
{
    public List<string> Validate(CappedAccrualScore item)
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

    public void RequireValid(CappedAccrualScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedAccrualScore> items)
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

/// <summary>Functions of capped accrual score rules.</summary>
public static class CappedAccrualScoreRules
{
    public static CappedAccrualScoreValidator DefaultCappedAccrualScoreValidator()
    {
        return new CappedAccrualScoreValidator(
            new List<ICappedAccrualScoreRule>
            {
                new CappedAccrualScoreBatchSizeRangeRule(32, 86400),
                new CappedAccrualScoreWindowRule(16),
                new CappedAccrualScoreFloorValueRangeRule(0.6, 268.2),
                new CappedAccrualScoreLabelLengthRule(3600),
                new CappedAccrualScoreRequiredSegmentRule("capped accrual score needs an entry"),
            });
    }
}
