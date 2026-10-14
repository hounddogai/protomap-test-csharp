namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy holiday group must pass. Check returns its problems, or none.</summary>
public interface ILegacyHolidayGroupRule
{
    List<string> Check(LegacyHolidayGroup item);
}

public sealed record LegacyHolidayGroupRequiredLocationRule(string Message) : ILegacyHolidayGroupRule
{
    public List<string> Check(LegacyHolidayGroup item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayGroupRequiredCodeRule(string Message) : ILegacyHolidayGroupRule
{
    public List<string> Check(LegacyHolidayGroup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayGroupRequiredChannelRule(string Message) : ILegacyHolidayGroupRule
{
    public List<string> Check(LegacyHolidayGroup item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayGroupLocationLengthRule(int MaxLength) : ILegacyHolidayGroupRule
{
    public List<string> Check(LegacyHolidayGroup item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyHolidayGroupAllowedCodeRule(List<string> Allowed) : ILegacyHolidayGroupRule
{
    public List<string> Check(LegacyHolidayGroup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy holiday group.</summary>
public sealed record LegacyHolidayGroupValidator(List<ILegacyHolidayGroupRule> Rules)
{
    public List<string> Validate(LegacyHolidayGroup item)
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

    public void RequireValid(LegacyHolidayGroup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyHolidayGroup> items)
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

/// <summary>Functions of legacy holiday group rules.</summary>
public static class LegacyHolidayGroupRules
{
    public static LegacyHolidayGroupValidator DefaultLegacyHolidayGroupValidator()
    {
        return new LegacyHolidayGroupValidator(
            new List<ILegacyHolidayGroupRule>
            {
                new LegacyHolidayGroupRequiredLocationRule("legacy holiday group needs this field"),
                new LegacyHolidayGroupRequiredCodeRule("legacy holiday group needs an entry"),
                new LegacyHolidayGroupRequiredChannelRule("legacy holiday group needs an entry"),
                new LegacyHolidayGroupLocationLengthRule(32),
                new LegacyHolidayGroupAllowedCodeRule(new List<string> { "pending", "pending" }),
            });
    }
}
