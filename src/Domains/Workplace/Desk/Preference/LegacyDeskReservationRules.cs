namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy desk reservation must pass. Check returns its problems, or none.</summary>
public interface ILegacyDeskReservationRule
{
    List<string> Check(LegacyDeskReservation item);
}

public sealed record LegacyDeskReservationLocaleLengthRule(int MaxLength) : ILegacyDeskReservationRule
{
    public List<string> Check(LegacyDeskReservation item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskReservationRequiredLocaleRule(string Message) : ILegacyDeskReservationRule
{
    public List<string> Check(LegacyDeskReservation item)
    {
        if (item.Locale.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskReservationRequiredReviewerRule(string Message) : ILegacyDeskReservationRule
{
    public List<string> Check(LegacyDeskReservation item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyDeskReservationAllowedReviewerRule(List<string> Allowed) : ILegacyDeskReservationRule
{
    public List<string> Check(LegacyDeskReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy desk reservation.</summary>
public sealed record LegacyDeskReservationValidator(List<ILegacyDeskReservationRule> Rules)
{
    public List<string> Validate(LegacyDeskReservation item)
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

    public void RequireValid(LegacyDeskReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyDeskReservation> items)
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

/// <summary>Functions of legacy desk reservation rules.</summary>
public static class LegacyDeskReservationRules
{
    public static LegacyDeskReservationValidator DefaultLegacyDeskReservationValidator()
    {
        return new LegacyDeskReservationValidator(
            new List<ILegacyDeskReservationRule>
            {
                new LegacyDeskReservationLocaleLengthRule(64),
                new LegacyDeskReservationRequiredLocaleRule("legacy desk reservation needs this field"),
                new LegacyDeskReservationRequiredReviewerRule("legacy desk reservation needs an entry"),
                new LegacyDeskReservationAllowedReviewerRule(new List<string>()),
            });
    }
}
