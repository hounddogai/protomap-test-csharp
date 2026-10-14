namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal holiday assignment must pass. Check returns its problems, or none.</summary>
public interface ISeasonalHolidayAssignmentRule
{
    List<string> Check(SeasonalHolidayAssignment item);
}

public sealed record SeasonalHolidayAssignmentAllowedCodeRule(List<string> Allowed) : ISeasonalHolidayAssignmentRule
{
    public List<string> Check(SeasonalHolidayAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAssignmentFloorValueRangeRule(double Min, double Max) : ISeasonalHolidayAssignmentRule
{
    public List<string> Check(SeasonalHolidayAssignment item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAssignmentRequiredLabelRule(string Message) : ISeasonalHolidayAssignmentRule
{
    public List<string> Check(SeasonalHolidayAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAssignmentLabelLengthRule(int MaxLength) : ISeasonalHolidayAssignmentRule
{
    public List<string> Check(SeasonalHolidayAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalHolidayAssignmentCodePrefixRule(string Prefix) : ISeasonalHolidayAssignmentRule
{
    public List<string> Check(SeasonalHolidayAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal holiday assignment.</summary>
public sealed record SeasonalHolidayAssignmentValidator(List<ISeasonalHolidayAssignmentRule> Rules)
{
    public List<string> Validate(SeasonalHolidayAssignment item)
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

    public void RequireValid(SeasonalHolidayAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalHolidayAssignment> items)
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

/// <summary>Functions of seasonal holiday assignment rules.</summary>
public static class SeasonalHolidayAssignmentRules
{
    public static SeasonalHolidayAssignmentValidator DefaultSeasonalHolidayAssignmentValidator()
    {
        return new SeasonalHolidayAssignmentValidator(
            new List<ISeasonalHolidayAssignmentRule>
            {
                new SeasonalHolidayAssignmentAllowedCodeRule(new List<string> { "capped" }),
                new SeasonalHolidayAssignmentFloorValueRangeRule(2.1, 3689.8),
                new SeasonalHolidayAssignmentRequiredLabelRule("seasonal holiday assignment needs this field"),
                new SeasonalHolidayAssignmentLabelLengthRule(80),
                new SeasonalHolidayAssignmentCodePrefixRule("REF"),
            });
    }
}
