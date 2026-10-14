namespace Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>A rule a occupancy rollup must pass. Check returns its problems, or none.</summary>
public interface IOccupancyRollupRule
{
    List<string> Check(OccupancyRollup item);
}

public sealed record OccupancyRollupRequiredContactEmailRule(string Message) : IOccupancyRollupRule
{
    public List<string> Check(OccupancyRollup item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRollupScoreRangeRule(double Min, double Max) : IOccupancyRollupRule
{
    public List<string> Check(OccupancyRollup item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRollupCodePrefixRule(string Prefix) : IOccupancyRollupRule
{
    public List<string> Check(OccupancyRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRollupCodeLengthRule(int MaxLength) : IOccupancyRollupRule
{
    public List<string> Check(OccupancyRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRollupContactEmailLengthRule(int MaxLength) : IOccupancyRollupRule
{
    public List<string> Check(OccupancyRollup item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OccupancyRollupRequiredChecksumRule(string Message) : IOccupancyRollupRule
{
    public List<string> Check(OccupancyRollup item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a occupancy rollup.</summary>
public sealed record OccupancyRollupValidator(List<IOccupancyRollupRule> Rules)
{
    public List<string> Validate(OccupancyRollup item)
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

    public void RequireValid(OccupancyRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OccupancyRollup> items)
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

/// <summary>Functions of occupancy rollup rules.</summary>
public static class OccupancyRollupRules
{
    public static OccupancyRollupValidator DefaultOccupancyRollupValidator()
    {
        return new OccupancyRollupValidator(
            new List<IOccupancyRollupRule>
            {
                new OccupancyRollupRequiredContactEmailRule("occupancy rollup needs an entry"),
                new OccupancyRollupScoreRangeRule(3.4, 2510.9),
                new OccupancyRollupCodePrefixRule("OPS"),
                new OccupancyRollupCodeLengthRule(86400),
                new OccupancyRollupContactEmailLengthRule(64),
                new OccupancyRollupRequiredChecksumRule("occupancy rollup needs this field"),
            });
    }
}
