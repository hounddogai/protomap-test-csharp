namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a regional holiday window must pass. Check returns its problems, or none.</summary>
public interface IRegionalHolidayWindowRule
{
    List<string> Check(RegionalHolidayWindow item);
}

public sealed record RegionalHolidayWindowCostRangeRule(double Min, double Max) : IRegionalHolidayWindowRule
{
    public List<string> Check(RegionalHolidayWindow item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RegionalHolidayWindowRequiredCodeRule(string Message) : IRegionalHolidayWindowRule
{
    public List<string> Check(RegionalHolidayWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayWindowRequiredTeamRule(string Message) : IRegionalHolidayWindowRule
{
    public List<string> Check(RegionalHolidayWindow item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayWindowWeightRangeRule(double Min, double Max) : IRegionalHolidayWindowRule
{
    public List<string> Check(RegionalHolidayWindow item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayWindowRequiredLabelRule(string Message) : IRegionalHolidayWindowRule
{
    public List<string> Check(RegionalHolidayWindow item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalHolidayWindowAllowedLabelRule(List<string> Allowed) : IRegionalHolidayWindowRule
{
    public List<string> Check(RegionalHolidayWindow item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional holiday window.</summary>
public sealed record RegionalHolidayWindowValidator(List<IRegionalHolidayWindowRule> Rules)
{
    public List<string> Validate(RegionalHolidayWindow item)
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

    public void RequireValid(RegionalHolidayWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalHolidayWindow> items)
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

/// <summary>Functions of regional holiday window rules.</summary>
public static class RegionalHolidayWindowRules
{
    public static RegionalHolidayWindowValidator DefaultRegionalHolidayWindowValidator()
    {
        return new RegionalHolidayWindowValidator(
            new List<IRegionalHolidayWindowRule>
            {
                new RegionalHolidayWindowCostRangeRule(5.7, 1759.7),
                new RegionalHolidayWindowRequiredCodeRule("regional holiday window needs an entry"),
                new RegionalHolidayWindowRequiredTeamRule("regional holiday window needs an entry"),
                new RegionalHolidayWindowWeightRangeRule(8.5, 3266.9),
                new RegionalHolidayWindowRequiredLabelRule("regional holiday window needs an entry"),
                new RegionalHolidayWindowAllowedLabelRule(new List<string> { "tiered", "seasonal", "manual" }),
            });
    }
}
