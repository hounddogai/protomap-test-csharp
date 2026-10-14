namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched nomination reservation must pass. Check returns its problems, or none.</summary>
public interface IBatchedNominationReservationRule
{
    List<string> Check(BatchedNominationReservation item);
}

public sealed record BatchedNominationReservationApprovedGuardRule(int Limit) : IBatchedNominationReservationRule
{
    public List<string> Check(BatchedNominationReservation item)
    {
        if (item.Approved && item.UpdatedAt > Limit)
        {
            return new List<string> { $"updated at {item.UpdatedAt} exceeds {Limit} while approved" };
        }
        return new List<string>();
    }
}

public sealed record BatchedNominationReservationRequiredLabelRule(string Message) : IBatchedNominationReservationRule
{
    public List<string> Check(BatchedNominationReservation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedNominationReservationAllowedLabelRule(List<string> Allowed) : IBatchedNominationReservationRule
{
    public List<string> Check(BatchedNominationReservation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record BatchedNominationReservationTitleLengthRule(int MaxLength) : IBatchedNominationReservationRule
{
    public List<string> Check(BatchedNominationReservation item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedNominationReservationCodePrefixRule(string Prefix) : IBatchedNominationReservationRule
{
    public List<string> Check(BatchedNominationReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched nomination reservation.</summary>
public sealed record BatchedNominationReservationValidator(List<IBatchedNominationReservationRule> Rules)
{
    public List<string> Validate(BatchedNominationReservation item)
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

    public void RequireValid(BatchedNominationReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedNominationReservation> items)
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

/// <summary>Functions of batched nomination reservation rules.</summary>
public static class BatchedNominationReservationRules
{
    public static BatchedNominationReservationValidator DefaultBatchedNominationReservationValidator()
    {
        return new BatchedNominationReservationValidator(
            new List<IBatchedNominationReservationRule>
            {
                new BatchedNominationReservationApprovedGuardRule(16),
                new BatchedNominationReservationRequiredLabelRule("batched nomination reservation needs this field"),
                new BatchedNominationReservationAllowedLabelRule(new List<string> { "default" }),
                new BatchedNominationReservationTitleLengthRule(255),
                new BatchedNominationReservationCodePrefixRule("Q"),
            });
    }
}
