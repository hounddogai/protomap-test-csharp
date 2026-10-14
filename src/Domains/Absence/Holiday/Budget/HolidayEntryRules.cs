namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday entry must pass. Check returns its problems, or none.</summary>
public interface IHolidayEntryRule
{
    List<string> Check(HolidayEntry item);
}

public sealed record HolidayEntryAllowedCodeRule(List<string> Allowed) : IHolidayEntryRule
{
    public List<string> Check(HolidayEntry item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record HolidayEntryCodePrefixRule(string Prefix) : IHolidayEntryRule
{
    public List<string> Check(HolidayEntry item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayEntryCodeLengthRule(int MaxLength) : IHolidayEntryRule
{
    public List<string> Check(HolidayEntry item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayEntryTitleLengthRule(int MaxLength) : IHolidayEntryRule
{
    public List<string> Check(HolidayEntry item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday entry.</summary>
public sealed record HolidayEntryValidator(List<IHolidayEntryRule> Rules)
{
    public List<string> Validate(HolidayEntry item)
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

    public void RequireValid(HolidayEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayEntry> items)
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

/// <summary>Functions of holiday entry rules.</summary>
public static class HolidayEntryRules
{
    public static HolidayEntryValidator DefaultHolidayEntryValidator()
    {
        return new HolidayEntryValidator(
            new List<IHolidayEntryRule>
            {
                new HolidayEntryAllowedCodeRule(new List<string> { "rolling" }),
                new HolidayEntryCodePrefixRule("X"),
                new HolidayEntryCodeLengthRule(86400),
                new HolidayEntryTitleLengthRule(86400),
            });
    }
}
