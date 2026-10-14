namespace Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>A rule a fallback adjustment proposal must pass. Check returns its problems, or none.</summary>
public interface IFallbackAdjustmentProposalRule
{
    List<string> Check(FallbackAdjustmentProposal item);
}

public sealed record FallbackAdjustmentProposalWindowDaysRangeRule(int Min, int Max) : IFallbackAdjustmentProposalRule
{
    public List<string> Check(FallbackAdjustmentProposal item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentProposalCodeLengthRule(int MaxLength) : IFallbackAdjustmentProposalRule
{
    public List<string> Check(FallbackAdjustmentProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentProposalRequiredExternalRefRule(string Message) : IFallbackAdjustmentProposalRule
{
    public List<string> Check(FallbackAdjustmentProposal item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackAdjustmentProposalPinnedGuardRule(int Limit) : IFallbackAdjustmentProposalRule
{
    public List<string> Check(FallbackAdjustmentProposal item)
    {
        if (item.Pinned && item.Sequence > Limit)
        {
            return new List<string> { $"sequence {item.Sequence} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback adjustment proposal.</summary>
public sealed record FallbackAdjustmentProposalValidator(List<IFallbackAdjustmentProposalRule> Rules)
{
    public List<string> Validate(FallbackAdjustmentProposal item)
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

    public void RequireValid(FallbackAdjustmentProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackAdjustmentProposal> items)
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

/// <summary>Functions of fallback adjustment proposal rules.</summary>
public static class FallbackAdjustmentProposalRules
{
    public static FallbackAdjustmentProposalValidator DefaultFallbackAdjustmentProposalValidator()
    {
        return new FallbackAdjustmentProposalValidator(
            new List<IFallbackAdjustmentProposalRule>
            {
                new FallbackAdjustmentProposalWindowDaysRangeRule(32, 1000),
                new FallbackAdjustmentProposalCodeLengthRule(255),
                new FallbackAdjustmentProposalRequiredExternalRefRule("fallback adjustment proposal needs a value"),
                new FallbackAdjustmentProposalPinnedGuardRule(64),
            });
    }
}
