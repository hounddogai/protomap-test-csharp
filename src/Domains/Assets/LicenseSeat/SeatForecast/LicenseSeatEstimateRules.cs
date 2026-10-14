namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>A rule a license seat estimate must pass. Check returns its problems, or none.</summary>
public interface ILicenseSeatEstimateRule
{
    List<string> Check(LicenseSeatEstimate item);
}

public sealed record LicenseSeatEstimateChecksumLengthRule(int MaxLength) : ILicenseSeatEstimateRule
{
    public List<string> Check(LicenseSeatEstimate item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatEstimateCodeLengthRule(int MaxLength) : ILicenseSeatEstimateRule
{
    public List<string> Check(LicenseSeatEstimate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LicenseSeatEstimateDurationMinutesRangeRule(int Min, int Max) : ILicenseSeatEstimateRule
{
    public List<string> Check(LicenseSeatEstimate item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a license seat estimate.</summary>
public sealed record LicenseSeatEstimateValidator(List<ILicenseSeatEstimateRule> Rules)
{
    public List<string> Validate(LicenseSeatEstimate item)
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

    public void RequireValid(LicenseSeatEstimate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LicenseSeatEstimate> items)
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

/// <summary>Functions of license seat estimate rules.</summary>
public static class LicenseSeatEstimateRules
{
    public static LicenseSeatEstimateValidator DefaultLicenseSeatEstimateValidator()
    {
        return new LicenseSeatEstimateValidator(
            new List<ILicenseSeatEstimateRule>
            {
                new LicenseSeatEstimateChecksumLengthRule(16),
                new LicenseSeatEstimateCodeLengthRule(64),
                new LicenseSeatEstimateDurationMinutesRangeRule(64, 3600),
            });
    }
}
