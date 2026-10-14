namespace Acme.Workforce.Assets.Refresh.Review;

/// <summary>A rule a refresh record must pass. Check returns its problems, or none.</summary>
public interface IRefreshRecordRule
{
    List<string> Check(RefreshRecord item);
}

public sealed record RefreshRecordSegmentLengthRule(int MaxLength) : IRefreshRecordRule
{
    public List<string> Check(RefreshRecord item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRecordRateRangeRule(double Min, double Max) : IRefreshRecordRule
{
    public List<string> Check(RefreshRecord item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RefreshRecordCodeLengthRule(int MaxLength) : IRefreshRecordRule
{
    public List<string> Check(RefreshRecord item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshRecordRequiredCodeRule(string Message) : IRefreshRecordRule
{
    public List<string> Check(RefreshRecord item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshRecordExpiresAtRangeRule(int Min, int Max) : IRefreshRecordRule
{
    public List<string> Check(RefreshRecord item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh record.</summary>
public sealed record RefreshRecordValidator(List<IRefreshRecordRule> Rules)
{
    public List<string> Validate(RefreshRecord item)
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

    public void RequireValid(RefreshRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshRecord> items)
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

/// <summary>Functions of refresh record rules.</summary>
public static class RefreshRecordRules
{
    public static RefreshRecordValidator DefaultRefreshRecordValidator()
    {
        return new RefreshRecordValidator(
            new List<IRefreshRecordRule>
            {
                new RefreshRecordSegmentLengthRule(1000),
                new RefreshRecordRateRangeRule(0.1, 4020.9),
                new RefreshRecordCodeLengthRule(255),
                new RefreshRecordRequiredCodeRule("refresh record needs a value"),
                new RefreshRecordExpiresAtRangeRule(64, 255),
            });
    }
}
