namespace Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>A rule a refresh segment must pass. Check returns its problems, or none.</summary>
public interface IRefreshSegmentRule
{
    List<string> Check(RefreshSegment item);
}

public sealed record RefreshSegmentTeamLengthRule(int MaxLength) : IRefreshSegmentRule
{
    public List<string> Check(RefreshSegment item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSegmentRequiredCodeRule(string Message) : IRefreshSegmentRule
{
    public List<string> Check(RefreshSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshSegmentSalaryBandLengthRule(int MaxLength) : IRefreshSegmentRule
{
    public List<string> Check(RefreshSegment item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSegmentTitleLengthRule(int MaxLength) : IRefreshSegmentRule
{
    public List<string> Check(RefreshSegment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshSegmentRequiredSalaryBandRule(string Message) : IRefreshSegmentRule
{
    public List<string> Check(RefreshSegment item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshSegmentDiscountRangeRule(double Min, double Max) : IRefreshSegmentRule
{
    public List<string> Check(RefreshSegment item)
    {
        var valueValue = item.Discount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"discount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a refresh segment.</summary>
public sealed record RefreshSegmentValidator(List<IRefreshSegmentRule> Rules)
{
    public List<string> Validate(RefreshSegment item)
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

    public void RequireValid(RefreshSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshSegment> items)
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

/// <summary>Functions of refresh segment rules.</summary>
public static class RefreshSegmentRules
{
    public static RefreshSegmentValidator DefaultRefreshSegmentValidator()
    {
        return new RefreshSegmentValidator(
            new List<IRefreshSegmentRule>
            {
                new RefreshSegmentTeamLengthRule(3600),
                new RefreshSegmentRequiredCodeRule("refresh segment needs an entry"),
                new RefreshSegmentSalaryBandLengthRule(255),
                new RefreshSegmentTitleLengthRule(255),
                new RefreshSegmentRequiredSalaryBandRule("refresh segment needs a value"),
                new RefreshSegmentDiscountRangeRule(7.4, 4100.9),
            });
    }
}
