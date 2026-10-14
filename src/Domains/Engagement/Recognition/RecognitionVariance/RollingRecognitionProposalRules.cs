namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling recognition proposal must pass. Check returns its problems, or none.</summary>
public interface IRollingRecognitionProposalRule
{
    List<string> Check(RollingRecognitionProposal item);
}

public sealed record RollingRecognitionProposalAllowedCodeRule(List<string> Allowed) : IRollingRecognitionProposalRule
{
    public List<string> Check(RollingRecognitionProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionProposalRequiredCountryRule(string Message) : IRollingRecognitionProposalRule
{
    public List<string> Check(RollingRecognitionProposal item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionProposalCodePrefixRule(string Prefix) : IRollingRecognitionProposalRule
{
    public List<string> Check(RollingRecognitionProposal item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionProposalRequiredLabelRule(string Message) : IRollingRecognitionProposalRule
{
    public List<string> Check(RollingRecognitionProposal item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingRecognitionProposalLabelLengthRule(int MaxLength) : IRollingRecognitionProposalRule
{
    public List<string> Check(RollingRecognitionProposal item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling recognition proposal.</summary>
public sealed record RollingRecognitionProposalValidator(List<IRollingRecognitionProposalRule> Rules)
{
    public List<string> Validate(RollingRecognitionProposal item)
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

    public void RequireValid(RollingRecognitionProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingRecognitionProposal> items)
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

/// <summary>Functions of rolling recognition proposal rules.</summary>
public static class RollingRecognitionProposalRules
{
    public static RollingRecognitionProposalValidator DefaultRollingRecognitionProposalValidator()
    {
        return new RollingRecognitionProposalValidator(
            new List<IRollingRecognitionProposalRule>
            {
                new RollingRecognitionProposalAllowedCodeRule(new List<string> { "capped", "scheduled" }),
                new RollingRecognitionProposalRequiredCountryRule("rolling recognition proposal needs an entry"),
                new RollingRecognitionProposalCodePrefixRule("X"),
                new RollingRecognitionProposalRequiredLabelRule("rolling recognition proposal needs an entry"),
                new RollingRecognitionProposalLabelLengthRule(86400),
            });
    }
}
