namespace Acme.Workforce.Workplace.Desk.Profile;

/// <summary>A rule a desk profile must pass. Check returns its problems, or none.</summary>
public interface IDeskProfileRule
{
    List<string> Check(DeskProfile item);
}

public sealed record DeskProfileCodePrefixRule(string Prefix) : IDeskProfileRule
{
    public List<string> Check(DeskProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DeskProfileUpdatedAtRangeRule(int Min, int Max) : IDeskProfileRule
{
    public List<string> Check(DeskProfile item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskProfileRequiredCodeRule(string Message) : IDeskProfileRule
{
    public List<string> Check(DeskProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskProfileRetriesRangeRule(int Min, int Max) : IDeskProfileRule
{
    public List<string> Check(DeskProfile item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskProfileRequiredChecksumRule(string Message) : IDeskProfileRule
{
    public List<string> Check(DeskProfile item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeskProfileRatioRangeRule(double Min, double Max) : IDeskProfileRule
{
    public List<string> Check(DeskProfile item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a desk profile.</summary>
public sealed record DeskProfileValidator(List<IDeskProfileRule> Rules)
{
    public List<string> Validate(DeskProfile item)
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

    public void RequireValid(DeskProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskProfile> items)
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

/// <summary>Functions of desk profile rules.</summary>
public static class DeskProfileRules
{
    public static DeskProfileValidator DefaultDeskProfileValidator()
    {
        return new DeskProfileValidator(
            new List<IDeskProfileRule>
            {
                new DeskProfileCodePrefixRule("OPS"),
                new DeskProfileUpdatedAtRangeRule(64, 255),
                new DeskProfileRequiredCodeRule("desk profile needs this field"),
                new DeskProfileRetriesRangeRule(255, 3600),
                new DeskProfileRequiredChecksumRule("desk profile needs this field"),
                new DeskProfileRatioRangeRule(9.0, 3241.5),
            });
    }
}
