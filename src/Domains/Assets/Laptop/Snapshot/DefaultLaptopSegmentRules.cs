namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a default laptop segment must pass. Check returns its problems, or none.</summary>
public interface IDefaultLaptopSegmentRule
{
    List<string> Check(DefaultLaptopSegment item);
}

public sealed record DefaultLaptopSegmentRequiredSalaryBandRule(string Message) : IDefaultLaptopSegmentRule
{
    public List<string> Check(DefaultLaptopSegment item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopSegmentSummaryLengthRule(int MaxLength) : IDefaultLaptopSegmentRule
{
    public List<string> Check(DefaultLaptopSegment item)
    {
        var size = item.Summary.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"summary is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultLaptopSegmentAllowedSalaryBandRule(List<string> Allowed) : IDefaultLaptopSegmentRule
{
    public List<string> Check(DefaultLaptopSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.SalaryBand)))
        {
            return new List<string> { $"salary band {item.SalaryBand} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default laptop segment.</summary>
public sealed record DefaultLaptopSegmentValidator(List<IDefaultLaptopSegmentRule> Rules)
{
    public List<string> Validate(DefaultLaptopSegment item)
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

    public void RequireValid(DefaultLaptopSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultLaptopSegment> items)
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

/// <summary>Functions of default laptop segment rules.</summary>
public static class DefaultLaptopSegmentRules
{
    public static DefaultLaptopSegmentValidator DefaultDefaultLaptopSegmentValidator()
    {
        return new DefaultLaptopSegmentValidator(
            new List<IDefaultLaptopSegmentRule>
            {
                new DefaultLaptopSegmentRequiredSalaryBandRule("default laptop segment needs a value"),
                new DefaultLaptopSegmentSummaryLengthRule(255),
                new DefaultLaptopSegmentAllowedSalaryBandRule(new List<string> { "global", "global" }),
            });
    }
}
