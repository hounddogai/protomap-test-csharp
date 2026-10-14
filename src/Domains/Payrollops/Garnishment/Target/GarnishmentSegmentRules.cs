namespace Acme.Workforce.Payrollops.Garnishment.Target;

/// <summary>A rule a garnishment segment must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentSegmentRule
{
    List<string> Check(GarnishmentSegment item);
}

public sealed record GarnishmentSegmentMinimumRangeRule(double Min, double Max) : IGarnishmentSegmentRule
{
    public List<string> Check(GarnishmentSegment item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentSegmentRequiredCodeRule(string Message) : IGarnishmentSegmentRule
{
    public List<string> Check(GarnishmentSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentSegmentCodeLengthRule(int MaxLength) : IGarnishmentSegmentRule
{
    public List<string> Check(GarnishmentSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment segment.</summary>
public sealed record GarnishmentSegmentValidator(List<IGarnishmentSegmentRule> Rules)
{
    public List<string> Validate(GarnishmentSegment item)
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

    public void RequireValid(GarnishmentSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentSegment> items)
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

/// <summary>Functions of garnishment segment rules.</summary>
public static class GarnishmentSegmentRules
{
    public static GarnishmentSegmentValidator DefaultGarnishmentSegmentValidator()
    {
        return new GarnishmentSegmentValidator(
            new List<IGarnishmentSegmentRule>
            {
                new GarnishmentSegmentMinimumRangeRule(10.0, 2084.4),
                new GarnishmentSegmentRequiredCodeRule("garnishment segment needs this field"),
                new GarnishmentSegmentCodeLengthRule(255),
            });
    }
}
