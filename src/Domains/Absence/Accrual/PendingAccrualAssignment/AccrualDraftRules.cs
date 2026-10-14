namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a accrual draft must pass. Check returns its problems, or none.</summary>
public interface IAccrualDraftRule
{
    List<string> Check(AccrualDraft item);
}

public sealed record AccrualDraftArchivedGuardRule(int Limit) : IAccrualDraftRule
{
    public List<string> Check(AccrualDraft item)
    {
        if (item.Archived && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDraftCountryLengthRule(int MaxLength) : IAccrualDraftRule
{
    public List<string> Check(AccrualDraft item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDraftCodePrefixRule(string Prefix) : IAccrualDraftRule
{
    public List<string> Check(AccrualDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDraftCodeLengthRule(int MaxLength) : IAccrualDraftRule
{
    public List<string> Check(AccrualDraft item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDraftAllowedCountryRule(List<string> Allowed) : IAccrualDraftRule
{
    public List<string> Check(AccrualDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record AccrualDraftRequiredLabelRule(string Message) : IAccrualDraftRule
{
    public List<string> Check(AccrualDraft item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a accrual draft.</summary>
public sealed record AccrualDraftValidator(List<IAccrualDraftRule> Rules)
{
    public List<string> Validate(AccrualDraft item)
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

    public void RequireValid(AccrualDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccrualDraft> items)
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

/// <summary>Functions of accrual draft rules.</summary>
public static class AccrualDraftRules
{
    public static AccrualDraftValidator DefaultAccrualDraftValidator()
    {
        return new AccrualDraftValidator(
            new List<IAccrualDraftRule>
            {
                new AccrualDraftArchivedGuardRule(80),
                new AccrualDraftCountryLengthRule(86400),
                new AccrualDraftCodePrefixRule("REF"),
                new AccrualDraftCodeLengthRule(32),
                new AccrualDraftAllowedCountryRule(new List<string> { "manual" }),
                new AccrualDraftRequiredLabelRule("accrual draft needs an entry"),
            });
    }
}
