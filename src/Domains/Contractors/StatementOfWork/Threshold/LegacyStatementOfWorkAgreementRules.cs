namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy statement of work agreement must pass. Check returns its problems, or none.</summary>
public interface ILegacyStatementOfWorkAgreementRule
{
    List<string> Check(LegacyStatementOfWorkAgreement item);
}

public sealed record LegacyStatementOfWorkAgreementAllowedTitleRule(List<string> Allowed) : ILegacyStatementOfWorkAgreementRule
{
    public List<string> Check(LegacyStatementOfWorkAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LegacyStatementOfWorkAgreementCodeLengthRule(int MaxLength) : ILegacyStatementOfWorkAgreementRule
{
    public List<string> Check(LegacyStatementOfWorkAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyStatementOfWorkAgreementBudgetRangeRule(double Min, double Max) : ILegacyStatementOfWorkAgreementRule
{
    public List<string> Check(LegacyStatementOfWorkAgreement item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyStatementOfWorkAgreementRequiredTitleRule(string Message) : ILegacyStatementOfWorkAgreementRule
{
    public List<string> Check(LegacyStatementOfWorkAgreement item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy statement of work agreement.</summary>
public sealed record LegacyStatementOfWorkAgreementValidator(List<ILegacyStatementOfWorkAgreementRule> Rules)
{
    public List<string> Validate(LegacyStatementOfWorkAgreement item)
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

    public void RequireValid(LegacyStatementOfWorkAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyStatementOfWorkAgreement> items)
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

/// <summary>Functions of legacy statement of work agreement rules.</summary>
public static class LegacyStatementOfWorkAgreementRules
{
    public static LegacyStatementOfWorkAgreementValidator DefaultLegacyStatementOfWorkAgreementValidator()
    {
        return new LegacyStatementOfWorkAgreementValidator(
            new List<ILegacyStatementOfWorkAgreementRule>
            {
                new LegacyStatementOfWorkAgreementAllowedTitleRule(new List<string> { "manual" }),
                new LegacyStatementOfWorkAgreementCodeLengthRule(64),
                new LegacyStatementOfWorkAgreementBudgetRangeRule(1.6, 1585.6),
                new LegacyStatementOfWorkAgreementRequiredTitleRule(
                    "legacy statement of work agreement needs this field"),
            });
    }
}
