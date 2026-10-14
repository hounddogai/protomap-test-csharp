namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation summary must pass. Check returns its problems, or none.</summary>
public interface IAttestationSummaryRule
{
    List<string> Check(AttestationSummary item);
}

public sealed record AttestationSummaryRequiredCodeRule(string Message) : IAttestationSummaryRule
{
    public List<string> Check(AttestationSummary item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationSummaryBudgetRangeRule(double Min, double Max) : IAttestationSummaryRule
{
    public List<string> Check(AttestationSummary item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AttestationSummaryDescriptionLengthRule(int MaxLength) : IAttestationSummaryRule
{
    public List<string> Check(AttestationSummary item)
    {
        var size = item.Description.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"description is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationSummaryRateRangeRule(double Min, double Max) : IAttestationSummaryRule
{
    public List<string> Check(AttestationSummary item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AttestationSummaryArchivedGuardRule(double Limit) : IAttestationSummaryRule
{
    public List<string> Check(AttestationSummary item)
    {
        if (item.Archived && item.Budget > Limit)
        {
            return new List<string> { $"budget {item.Budget} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record AttestationSummaryAllowedBadgeRule(List<string> Allowed) : IAttestationSummaryRule
{
    public List<string> Check(AttestationSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Badge)))
        {
            return new List<string> { $"badge {item.Badge} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation summary.</summary>
public sealed record AttestationSummaryValidator(List<IAttestationSummaryRule> Rules)
{
    public List<string> Validate(AttestationSummary item)
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

    public void RequireValid(AttestationSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationSummary> items)
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

/// <summary>Functions of attestation summary rules.</summary>
public static class AttestationSummaryRules
{
    public static AttestationSummaryValidator DefaultAttestationSummaryValidator()
    {
        return new AttestationSummaryValidator(
            new List<IAttestationSummaryRule>
            {
                new AttestationSummaryRequiredCodeRule("attestation summary needs a value"),
                new AttestationSummaryBudgetRangeRule(5.1, 2620.2),
                new AttestationSummaryDescriptionLengthRule(80),
                new AttestationSummaryRateRangeRule(8.0, 2927.3),
                new AttestationSummaryArchivedGuardRule(8.2),
                new AttestationSummaryAllowedBadgeRule(new List<string> { "fallback", "pending", "tiered" }),
            });
    }
}
