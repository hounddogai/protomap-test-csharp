namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a retro pay proposal must pass. Check returns its problems, or none.</summary>
public interface IRetroPayProposalRule
{
    List<string> Check(RetroPayProposal item);
}

public sealed record RetroPayProposalCodeLengthRule(int MaxLength) : IRetroPayProposalRule
{
    public List<string> Check(RetroPayProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayProposalGradeLengthRule(int MaxLength) : IRetroPayProposalRule
{
    public List<string> Check(RetroPayProposal item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayProposalUpdatedAtRangeRule(int Min, int Max) : IRetroPayProposalRule
{
    public List<string> Check(RetroPayProposal item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayProposalAllowedCodeRule(List<string> Allowed) : IRetroPayProposalRule
{
    public List<string> Check(RetroPayProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay proposal.</summary>
public sealed record RetroPayProposalValidator(List<IRetroPayProposalRule> Rules)
{
    public List<string> Validate(RetroPayProposal item)
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

    public void RequireValid(RetroPayProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayProposal> items)
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

/// <summary>Functions of retro pay proposal rules.</summary>
public static class RetroPayProposalRules
{
    public static RetroPayProposalValidator DefaultRetroPayProposalValidator()
    {
        return new RetroPayProposalValidator(
            new List<IRetroPayProposalRule>
            {
                new RetroPayProposalCodeLengthRule(255),
                new RetroPayProposalGradeLengthRule(1000),
                new RetroPayProposalUpdatedAtRangeRule(120, 120),
                new RetroPayProposalAllowedCodeRule(new List<string> { "regional" }),
            });
    }
}
