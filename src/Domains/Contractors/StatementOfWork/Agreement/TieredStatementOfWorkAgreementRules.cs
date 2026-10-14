namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tiered statement of work agreement must pass. Check returns its problems, or none.</summary>
public interface ITieredStatementOfWorkAgreementRule
{
    List<string> Check(TieredStatementOfWorkAgreement item);
}

public sealed record TieredStatementOfWorkAgreementRequiredNameRule(string Message) : ITieredStatementOfWorkAgreementRule
{
    public List<string> Check(TieredStatementOfWorkAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredStatementOfWorkAgreementManagerIdLengthRule(int MaxLength) : ITieredStatementOfWorkAgreementRule
{
    public List<string> Check(TieredStatementOfWorkAgreement item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredStatementOfWorkAgreementUpdatedAtRangeRule(int Min, int Max) : ITieredStatementOfWorkAgreementRule
{
    public List<string> Check(TieredStatementOfWorkAgreement item)
    {
        var valueValue = item.UpdatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"updated at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredStatementOfWorkAgreementRequiredManagerIdRule(string Message) : ITieredStatementOfWorkAgreementRule
{
    public List<string> Check(TieredStatementOfWorkAgreement item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredStatementOfWorkAgreementAllowedManagerIdRule(List<string> Allowed) : ITieredStatementOfWorkAgreementRule
{
    public List<string> Check(TieredStatementOfWorkAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is inconsistent" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered statement of work agreement.</summary>
public sealed record TieredStatementOfWorkAgreementValidator(List<ITieredStatementOfWorkAgreementRule> Rules)
{
    public List<string> Validate(TieredStatementOfWorkAgreement item)
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

    public void RequireValid(TieredStatementOfWorkAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredStatementOfWorkAgreement> items)
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

/// <summary>Functions of tiered statement of work agreement rules.</summary>
public static class TieredStatementOfWorkAgreementRules
{
    public static TieredStatementOfWorkAgreementValidator DefaultTieredStatementOfWorkAgreementValidator()
    {
        return new TieredStatementOfWorkAgreementValidator(
            new List<ITieredStatementOfWorkAgreementRule>
            {
                new TieredStatementOfWorkAgreementRequiredNameRule(
                    "tiered statement of work agreement needs this field"),
                new TieredStatementOfWorkAgreementManagerIdLengthRule(255),
                new TieredStatementOfWorkAgreementUpdatedAtRangeRule(64, 3600),
                new TieredStatementOfWorkAgreementRequiredManagerIdRule(
                    "tiered statement of work agreement needs this field"),
                new TieredStatementOfWorkAgreementAllowedManagerIdRule(new List<string>()),
            });
    }
}
