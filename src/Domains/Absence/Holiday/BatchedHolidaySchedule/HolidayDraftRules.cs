namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a holiday draft must pass. Check returns its problems, or none.</summary>
public interface IHolidayDraftRule
{
    List<string> Check(HolidayDraft item);
}

public sealed record HolidayDraftAllowedTitleRule(List<string> Allowed) : IHolidayDraftRule
{
    public List<string> Check(HolidayDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record HolidayDraftCodePrefixRule(string Prefix) : IHolidayDraftRule
{
    public List<string> Check(HolidayDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record HolidayDraftRequiredContactEmailRule(string Message) : IHolidayDraftRule
{
    public List<string> Check(HolidayDraft item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a holiday draft.</summary>
public sealed record HolidayDraftValidator(List<IHolidayDraftRule> Rules)
{
    public List<string> Validate(HolidayDraft item)
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

    public void RequireValid(HolidayDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HolidayDraft> items)
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

/// <summary>Functions of holiday draft rules.</summary>
public static class HolidayDraftRules
{
    public static HolidayDraftValidator DefaultHolidayDraftValidator()
    {
        return new HolidayDraftValidator(
            new List<IHolidayDraftRule>
            {
                new HolidayDraftAllowedTitleRule(new List<string>()),
                new HolidayDraftCodePrefixRule("OPS"),
                new HolidayDraftRequiredContactEmailRule("holiday draft needs this field"),
            });
    }
}
