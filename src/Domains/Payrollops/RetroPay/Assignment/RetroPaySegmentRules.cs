namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay segment must pass. Check returns its problems, or none.</summary>
public interface IRetroPaySegmentRule
{
    List<string> Check(RetroPaySegment item);
}

public sealed record RetroPaySegmentRequiredCodeRule(string Message) : IRetroPaySegmentRule
{
    public List<string> Check(RetroPaySegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySegmentRegionLengthRule(int MaxLength) : IRetroPaySegmentRule
{
    public List<string> Check(RetroPaySegment item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySegmentCodeLengthRule(int MaxLength) : IRetroPaySegmentRule
{
    public List<string> Check(RetroPaySegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySegmentAllowedRegionRule(List<string> Allowed) : IRetroPaySegmentRule
{
    public List<string> Check(RetroPaySegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Region)))
        {
            return new List<string> { $"region {item.Region} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySegmentRequiredLocationRule(string Message) : IRetroPaySegmentRule
{
    public List<string> Check(RetroPaySegment item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySegmentCeilingRangeRule(double Min, double Max) : IRetroPaySegmentRule
{
    public List<string> Check(RetroPaySegment item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a retro pay segment.</summary>
public sealed record RetroPaySegmentValidator(List<IRetroPaySegmentRule> Rules)
{
    public List<string> Validate(RetroPaySegment item)
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

    public void RequireValid(RetroPaySegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPaySegment> items)
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

/// <summary>Functions of retro pay segment rules.</summary>
public static class RetroPaySegmentRules
{
    public static RetroPaySegmentValidator DefaultRetroPaySegmentValidator()
    {
        return new RetroPaySegmentValidator(
            new List<IRetroPaySegmentRule>
            {
                new RetroPaySegmentRequiredCodeRule("retro pay segment needs this field"),
                new RetroPaySegmentRegionLengthRule(32),
                new RetroPaySegmentCodeLengthRule(64),
                new RetroPaySegmentAllowedRegionRule(new List<string> { "pending" }),
                new RetroPaySegmentRequiredLocationRule("retro pay segment needs an entry"),
                new RetroPaySegmentCeilingRangeRule(6.8, 1605.2),
            });
    }
}
