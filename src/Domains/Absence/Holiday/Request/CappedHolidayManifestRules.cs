namespace Acme.Workforce.Absence.Holiday.Request;

/// <summary>A rule a capped holiday manifest must pass. Check returns its problems, or none.</summary>
public interface ICappedHolidayManifestRule
{
    List<string> Check(CappedHolidayManifest item);
}

public sealed record CappedHolidayManifestSegmentLengthRule(int MaxLength) : ICappedHolidayManifestRule
{
    public List<string> Check(CappedHolidayManifest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedHolidayManifestRequiredLabelRule(string Message) : ICappedHolidayManifestRule
{
    public List<string> Check(CappedHolidayManifest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedHolidayManifestRateRangeRule(double Min, double Max) : ICappedHolidayManifestRule
{
    public List<string> Check(CappedHolidayManifest item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CappedHolidayManifestPinnedGuardRule(double Limit) : ICappedHolidayManifestRule
{
    public List<string> Check(CappedHolidayManifest item)
    {
        if (item.Pinned && item.Rate > Limit)
        {
            return new List<string> { $"rate {item.Rate} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record CappedHolidayManifestRequiredCodeRule(string Message) : ICappedHolidayManifestRule
{
    public List<string> Check(CappedHolidayManifest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped holiday manifest.</summary>
public sealed record CappedHolidayManifestValidator(List<ICappedHolidayManifestRule> Rules)
{
    public List<string> Validate(CappedHolidayManifest item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(CappedHolidayManifest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedHolidayManifest> items)
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

/// <summary>Functions of capped holiday manifest rules.</summary>
public static class CappedHolidayManifestRules
{
    public static CappedHolidayManifestValidator DefaultCappedHolidayManifestValidator()
    {
        return new CappedHolidayManifestValidator(
            new List<ICappedHolidayManifestRule>
            {
                new CappedHolidayManifestSegmentLengthRule(255),
                new CappedHolidayManifestRequiredLabelRule("capped holiday manifest needs a value"),
                new CappedHolidayManifestRateRangeRule(7.4, 4768.2),
                new CappedHolidayManifestPinnedGuardRule(5.2),
                new CappedHolidayManifestRequiredCodeRule("capped holiday manifest needs an entry"),
            });
    }
}
