namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary headcount agreement must pass. Check returns its problems, or none.</summary>
public interface IPrimaryHeadcountAgreementRule
{
    List<string> Check(PrimaryHeadcountAgreement item);
}

public sealed record PrimaryHeadcountAgreementRequiredCodeRule(string Message) : IPrimaryHeadcountAgreementRule
{
    public List<string> Check(PrimaryHeadcountAgreement item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountAgreementVisibleGuardRule(int Limit) : IPrimaryHeadcountAgreementRule
{
    public List<string> Check(PrimaryHeadcountAgreement item)
    {
        if (item.Visible && item.CreatedAt > Limit)
        {
            return new List<string> { $"created at {item.CreatedAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountAgreementCreatedAtRangeRule(int Min, int Max) : IPrimaryHeadcountAgreementRule
{
    public List<string> Check(PrimaryHeadcountAgreement item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryHeadcountAgreementAllowedTitleRule(List<string> Allowed) : IPrimaryHeadcountAgreementRule
{
    public List<string> Check(PrimaryHeadcountAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary headcount agreement.</summary>
public sealed record PrimaryHeadcountAgreementValidator(List<IPrimaryHeadcountAgreementRule> Rules)
{
    public List<string> Validate(PrimaryHeadcountAgreement item)
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

    public void RequireValid(PrimaryHeadcountAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryHeadcountAgreement> items)
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

/// <summary>Functions of primary headcount agreement rules.</summary>
public static class PrimaryHeadcountAgreementRules
{
    public static PrimaryHeadcountAgreementValidator DefaultPrimaryHeadcountAgreementValidator()
    {
        return new PrimaryHeadcountAgreementValidator(
            new List<IPrimaryHeadcountAgreementRule>
            {
                new PrimaryHeadcountAgreementRequiredCodeRule("primary headcount agreement needs a value"),
                new PrimaryHeadcountAgreementVisibleGuardRule(255),
                new PrimaryHeadcountAgreementCreatedAtRangeRule(16, 1000),
                new PrimaryHeadcountAgreementAllowedTitleRule(new List<string> { "lenient" }),
            });
    }
}
