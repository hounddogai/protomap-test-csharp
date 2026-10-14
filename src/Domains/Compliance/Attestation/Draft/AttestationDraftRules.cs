namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a attestation draft must pass. Check returns its problems, or none.</summary>
public interface IAttestationDraftRule
{
    List<string> Check(AttestationDraft item);
}

public sealed record AttestationDraftAllowedReviewerRule(List<string> Allowed) : IAttestationDraftRule
{
    public List<string> Check(AttestationDraft item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Reviewer)))
        {
            return new List<string> { $"reviewer {item.Reviewer} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record AttestationDraftRequiredChannelRule(string Message) : IAttestationDraftRule
{
    public List<string> Check(AttestationDraft item)
    {
        if (item.Channel.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AttestationDraftRatingRangeRule(double Min, double Max) : IAttestationDraftRule
{
    public List<string> Check(AttestationDraft item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a attestation draft.</summary>
public sealed record AttestationDraftValidator(List<IAttestationDraftRule> Rules)
{
    public List<string> Validate(AttestationDraft item)
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

    public void RequireValid(AttestationDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AttestationDraft> items)
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

/// <summary>Functions of attestation draft rules.</summary>
public static class AttestationDraftRules
{
    public static AttestationDraftValidator DefaultAttestationDraftValidator()
    {
        return new AttestationDraftValidator(
            new List<IAttestationDraftRule>
            {
                new AttestationDraftAllowedReviewerRule(new List<string> { "weighted", "scheduled" }),
                new AttestationDraftRequiredChannelRule("attestation draft needs an entry"),
                new AttestationDraftRatingRangeRule(0.3, 3854.2),
            });
    }
}
