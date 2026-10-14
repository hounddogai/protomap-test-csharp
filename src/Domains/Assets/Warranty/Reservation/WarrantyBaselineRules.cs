namespace Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>A rule a warranty baseline must pass. Check returns its problems, or none.</summary>
public interface IWarrantyBaselineRule
{
    List<string> Check(WarrantyBaseline item);
}

public sealed record WarrantyBaselinePhoneLengthRule(int MaxLength) : IWarrantyBaselineRule
{
    public List<string> Check(WarrantyBaseline item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBaselineCodePrefixRule(string Prefix) : IWarrantyBaselineRule
{
    public List<string> Check(WarrantyBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBaselineSegmentLengthRule(int MaxLength) : IWarrantyBaselineRule
{
    public List<string> Check(WarrantyBaseline item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBaselineLabelLengthRule(int MaxLength) : IWarrantyBaselineRule
{
    public List<string> Check(WarrantyBaseline item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBaselineCodeLengthRule(int MaxLength) : IWarrantyBaselineRule
{
    public List<string> Check(WarrantyBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantyBaselineScoreRangeRule(double Min, double Max) : IWarrantyBaselineRule
{
    public List<string> Check(WarrantyBaseline item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty baseline.</summary>
public sealed record WarrantyBaselineValidator(List<IWarrantyBaselineRule> Rules)
{
    public List<string> Validate(WarrantyBaseline item)
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

    public void RequireValid(WarrantyBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantyBaseline> items)
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

/// <summary>Functions of warranty baseline rules.</summary>
public static class WarrantyBaselineRules
{
    public static WarrantyBaselineValidator DefaultWarrantyBaselineValidator()
    {
        return new WarrantyBaselineValidator(
            new List<IWarrantyBaselineRule>
            {
                new WarrantyBaselinePhoneLengthRule(80),
                new WarrantyBaselineCodePrefixRule("ACM"),
                new WarrantyBaselineSegmentLengthRule(120),
                new WarrantyBaselineLabelLengthRule(255),
                new WarrantyBaselineCodeLengthRule(255),
                new WarrantyBaselineScoreRangeRule(8.7, 3854.2),
            });
    }
}
