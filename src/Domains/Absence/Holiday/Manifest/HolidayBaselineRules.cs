namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday baseline must pass. Check returns its problems, or none.</summary>
public interface IHolidayBaselineRule
{
    List<string> Check(HolidayBaseline item);
}

public sealed record HolidayBaselineAllowedTeamRule(List<string> Allowed) : IHolidayBaselineRule
{
    public List<string> Check(HolidayBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record HolidayBaselineRequiredTeamRule(string Message) : IHolidayBaselineRule
{
    public List<string> Check(HolidayBaseline item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record HolidayBaselineCostCenterLengthRule(int MaxLength) : IHolidayBaselineRule
{
    public List<string> Check(HolidayBaseline item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayBaselineCodeLengthRule(int MaxLength) : IHolidayBaselineRule
{
    public List<string> Check(HolidayBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday baseline.</summary>
public sealed record HolidayBaselineValidator(List<IHolidayBaselineRule> Rules)
{
    public List<string> Validate(HolidayBaseline item)
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

    public void RequireValid(HolidayBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayBaseline> items)
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

/// <summary>Functions of holiday baseline rules.</summary>
public static class HolidayBaselineRules
{
    public static HolidayBaselineValidator DefaultHolidayBaselineValidator()
    {
        return new HolidayBaselineValidator(
            new List<IHolidayBaselineRule>
            {
                new HolidayBaselineAllowedTeamRule(new List<string> { "global" }),
                new HolidayBaselineRequiredTeamRule("holiday baseline needs a value"),
                new HolidayBaselineCostCenterLengthRule(16),
                new HolidayBaselineCodeLengthRule(32),
            });
    }
}
