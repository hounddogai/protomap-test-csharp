namespace Acme.Workforce.Workplace.Desk.Digest;

/// <summary>A rule a desk digest must pass. Check returns its problems, or none.</summary>
public interface IDeskDigestRule
{
    List<string> Check(DeskDigest item);
}

public sealed record DeskDigestRegionLengthRule(int MaxLength) : IDeskDigestRule
{
    public List<string> Check(DeskDigest item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskDigestRequiredCodeRule(string Message) : IDeskDigestRule
{
    public List<string> Check(DeskDigest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskDigestCeilingRangeRule(double Min, double Max) : IDeskDigestRule
{
    public List<string> Check(DeskDigest item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk digest.</summary>
public sealed record DeskDigestValidator(List<IDeskDigestRule> Rules)
{
    public List<string> Validate(DeskDigest item)
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

    public void RequireValid(DeskDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskDigest> items)
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

/// <summary>Functions of desk digest rules.</summary>
public static class DeskDigestRules
{
    public static DeskDigestValidator DefaultDeskDigestValidator()
    {
        return new DeskDigestValidator(
            new List<IDeskDigestRule>
            {
                new DeskDigestRegionLengthRule(1000),
                new DeskDigestRequiredCodeRule("desk digest needs a value"),
                new DeskDigestCeilingRangeRule(3.1, 3599.5),
            });
    }
}
