namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

/// <summary>A rule a floor plan draft must pass. Check returns its problems, or none.</summary>
public interface IFloorPlanDraftRule
{
    List<string> Check(FloorPlanDraft item);
}

public sealed record FloorPlanDraftRequiredSummaryRule(string Message) : IFloorPlanDraftRule
{
    public List<string> Check(FloorPlanDraft item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDraftWindowDaysRangeRule(int Min, int Max) : IFloorPlanDraftRule
{
    public List<string> Check(FloorPlanDraft item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDraftNameLengthRule(int MaxLength) : IFloorPlanDraftRule
{
    public List<string> Check(FloorPlanDraft item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDraftCodePrefixRule(string Prefix) : IFloorPlanDraftRule
{
    public List<string> Check(FloorPlanDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FloorPlanDraftChecksumLengthRule(int MaxLength) : IFloorPlanDraftRule
{
    public List<string> Check(FloorPlanDraft item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a floor plan draft.</summary>
public sealed record FloorPlanDraftValidator(List<IFloorPlanDraftRule> Rules)
{
    public List<string> Validate(FloorPlanDraft item)
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

    public void RequireValid(FloorPlanDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FloorPlanDraft> items)
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

/// <summary>Functions of floor plan draft rules.</summary>
public static class FloorPlanDraftRules
{
    public static FloorPlanDraftValidator DefaultFloorPlanDraftValidator()
    {
        return new FloorPlanDraftValidator(
            new List<IFloorPlanDraftRule>
            {
                new FloorPlanDraftRequiredSummaryRule("floor plan draft needs an entry"),
                new FloorPlanDraftWindowDaysRangeRule(255, 1000),
                new FloorPlanDraftNameLengthRule(80),
                new FloorPlanDraftCodePrefixRule("ACM"),
                new FloorPlanDraftChecksumLengthRule(80),
            });
    }
}
