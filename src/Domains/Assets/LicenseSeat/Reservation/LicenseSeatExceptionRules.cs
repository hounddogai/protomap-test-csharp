namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>A rule a license seat exception must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatExceptionRule
{
    List<string> Check(LicenseSeatException item);
}

public sealed record LicenseSeatExceptionExpiresAtRangeRule(int Min, int Max) : ILicenseSeatExceptionRule
{
    public List<string> Check(LicenseSeatException item)
    {
        var valueValue = item.ExpiresAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"expires at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"expires at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LicenseSeatExceptionSegmentLengthRule(int MaxLength) : ILicenseSeatExceptionRule
{
    public List<string> Check(LicenseSeatException item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatExceptionRequiredNameRule(string Message) : ILicenseSeatExceptionRule
{
    public List<string> Check(LicenseSeatException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatExceptionScoreRangeRule(double Min, double Max) : ILicenseSeatExceptionRule
{
    public List<string> Check(LicenseSeatException item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat exception.</summary>
public sealed record LicenseSeatExceptionValidator(List<ILicenseSeatExceptionRule> Rules)
{
    public List<string> Validate(LicenseSeatException item)
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

    public void RequireValid(LicenseSeatException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatException> items)
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

/// <summary>Functions of license seat exception rules.</summary>
public static class LicenseSeatExceptionRules
{
    public static LicenseSeatExceptionValidator DefaultLicenseSeatExceptionValidator()
    {
        return new LicenseSeatExceptionValidator(
            new List<ILicenseSeatExceptionRule>
            {
                new LicenseSeatExceptionExpiresAtRangeRule(3600, 3600),
                new LicenseSeatExceptionSegmentLengthRule(16),
                new LicenseSeatExceptionRequiredNameRule("license seat exception needs a value"),
                new LicenseSeatExceptionScoreRangeRule(0.0, 3232.9),
            });
    }
}
