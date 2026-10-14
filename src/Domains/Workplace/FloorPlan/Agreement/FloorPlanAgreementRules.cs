namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a floor plan agreement must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanAgreementRule
{
    List<string> Check(FloorPlanAgreement item);
}

public sealed record FloorPlanAgreementChannelLengthRule(int MaxLength) : IFloorPlanAgreementRule
{
    public List<string> Check(FloorPlanAgreement item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAgreementCodePrefixRule(string Prefix) : IFloorPlanAgreementRule
{
    public List<string> Check(FloorPlanAgreement item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAgreementAllowedCodeRule(List<string> Allowed) : IFloorPlanAgreementRule
{
    public List<string> Check(FloorPlanAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAgreementScoreRangeRule(double Min, double Max) : IFloorPlanAgreementRule
{
    public List<string> Check(FloorPlanAgreement item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record FloorPlanAgreementRequiredChannelRule(string Message) : IFloorPlanAgreementRule
{
    public List<string> Check(FloorPlanAgreement item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanAgreementNameLengthRule(int MaxLength) : IFloorPlanAgreementRule
{
    public List<string> Check(FloorPlanAgreement item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan agreement.</summary>
public sealed record FloorPlanAgreementValidator(List<IFloorPlanAgreementRule> Rules)
{
    public List<string> Validate(FloorPlanAgreement item)
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

    public void RequireValid(FloorPlanAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanAgreement> items)
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

/// <summary>Functions of floor plan agreement rules.</summary>
public static class FloorPlanAgreementRules
{
    public static FloorPlanAgreementValidator DefaultFloorPlanAgreementValidator()
    {
        return new FloorPlanAgreementValidator(
            new List<IFloorPlanAgreementRule>
            {
                new FloorPlanAgreementChannelLengthRule(32),
                new FloorPlanAgreementCodePrefixRule("OPS"),
                new FloorPlanAgreementAllowedCodeRule(new List<string> { "strict", "default", "rolling" }),
                new FloorPlanAgreementScoreRangeRule(2.0, 4452.5),
                new FloorPlanAgreementRequiredChannelRule("floor plan agreement needs an entry"),
                new FloorPlanAgreementNameLengthRule(32),
            });
    }
}
