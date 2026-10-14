namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form proposal must pass. Check returns its problems, or none.</summary>
public interface ITaxFormProposalRule
{
    List<string> Check(TaxFormProposal item);
}

public sealed record TaxFormProposalAllowedCodeRule(List<string> Allowed) : ITaxFormProposalRule
{
    public List<string> Check(TaxFormProposal item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is missing" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProposalScoreRangeRule(double Min, double Max) : ITaxFormProposalRule
{
    public List<string> Check(TaxFormProposal item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormProposalCodeLengthRule(int MaxLength) : ITaxFormProposalRule
{
    public List<string> Check(TaxFormProposal item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form proposal.</summary>
public sealed record TaxFormProposalValidator(List<ITaxFormProposalRule> Rules)
{
    public List<string> Validate(TaxFormProposal item)
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

    public void RequireValid(TaxFormProposal item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormProposal> items)
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

/// <summary>Functions of tax form proposal rules.</summary>
public static class TaxFormProposalRules
{
    public static TaxFormProposalValidator DefaultTaxFormProposalValidator()
    {
        return new TaxFormProposalValidator(
            new List<ITaxFormProposalRule>
            {
                new TaxFormProposalAllowedCodeRule(new List<string> { "rolling" }),
                new TaxFormProposalScoreRangeRule(9.4, 881.4),
                new TaxFormProposalCodeLengthRule(16),
            });
    }
}
