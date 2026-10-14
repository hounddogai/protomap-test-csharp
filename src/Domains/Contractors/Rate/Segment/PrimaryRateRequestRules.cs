namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary rate request must pass. Check returns its problems, or none.</summary>
public interface IPrimaryRateRequestRule
{
    List<string> Check(PrimaryRateRequest item);
}

public sealed record PrimaryRateRequestCostRangeRule(double Min, double Max) : IPrimaryRateRequestRule
{
    public List<string> Check(PrimaryRateRequest item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRateRequestRequiredLabelRule(string Message) : IPrimaryRateRequestRule
{
    public List<string> Check(PrimaryRateRequest item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRateRequestSegmentLengthRule(int MaxLength) : IPrimaryRateRequestRule
{
    public List<string> Check(PrimaryRateRequest item)
    {
        var size = item.Segment.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"segment is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRateRequestLeaveDaysRangeRule(int Min, int Max) : IPrimaryRateRequestRule
{
    public List<string> Check(PrimaryRateRequest item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRateRequestRequiredSegmentRule(string Message) : IPrimaryRateRequestRule
{
    public List<string> Check(PrimaryRateRequest item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryRateRequestAllowedCostCenterRule(List<string> Allowed) : IPrimaryRateRequestRule
{
    public List<string> Check(PrimaryRateRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary rate request.</summary>
public sealed record PrimaryRateRequestValidator(List<IPrimaryRateRequestRule> Rules)
{
    public List<string> Validate(PrimaryRateRequest item)
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

    public void RequireValid(PrimaryRateRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryRateRequest> items)
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

/// <summary>Functions of primary rate request rules.</summary>
public static class PrimaryRateRequestRules
{
    public static PrimaryRateRequestValidator DefaultPrimaryRateRequestValidator()
    {
        return new PrimaryRateRequestValidator(
            new List<IPrimaryRateRequestRule>
            {
                new PrimaryRateRequestCostRangeRule(4.1, 2122.9),
                new PrimaryRateRequestRequiredLabelRule("primary rate request needs an entry"),
                new PrimaryRateRequestSegmentLengthRule(1000),
                new PrimaryRateRequestLeaveDaysRangeRule(120, 3600),
                new PrimaryRateRequestRequiredSegmentRule("primary rate request needs this field"),
                new PrimaryRateRequestAllowedCostCenterRule(new List<string> { "global" }),
            });
    }
}
