namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a warranty segment must pass. Check returns its problems, or none.</summary>
public interface IWarrantySegmentRule
{
    List<string> Check(WarrantySegment item);
}

public sealed record WarrantySegmentStartsAtRangeRule(int Min, int Max) : IWarrantySegmentRule
{
    public List<string> Check(WarrantySegment item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantySegmentAllowedLabelRule(List<string> Allowed) : IWarrantySegmentRule
{
    public List<string> Check(WarrantySegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record WarrantySegmentChannelLengthRule(int MaxLength) : IWarrantySegmentRule
{
    public List<string> Check(WarrantySegment item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WarrantySegmentLabelLengthRule(int MaxLength) : IWarrantySegmentRule
{
    public List<string> Check(WarrantySegment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a warranty segment.</summary>
public sealed record WarrantySegmentValidator(List<IWarrantySegmentRule> Rules)
{
    public List<string> Validate(WarrantySegment item)
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

    public void RequireValid(WarrantySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WarrantySegment> items)
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

/// <summary>Functions of warranty segment rules.</summary>
public static class WarrantySegmentRules
{
    public static WarrantySegmentValidator DefaultWarrantySegmentValidator()
    {
        return new WarrantySegmentValidator(
            new List<IWarrantySegmentRule>
            {
                new WarrantySegmentStartsAtRangeRule(64, 64),
                new WarrantySegmentAllowedLabelRule(new List<string> { "pending", "seasonal", "weighted" }),
                new WarrantySegmentChannelLengthRule(3600),
                new WarrantySegmentLabelLengthRule(80),
            });
    }
}
