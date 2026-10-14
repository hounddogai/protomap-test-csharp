namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual statement of work baseline must pass. Check returns its problems, or none.</summary>
public interface IManualStatementOfWorkBaselineRule
{
    List<string> Check(ManualStatementOfWorkBaseline item);
}

public sealed record ManualStatementOfWorkBaselineRequiredContactEmailRule(string Message) : IManualStatementOfWorkBaselineRule
{
    public List<string> Check(ManualStatementOfWorkBaseline item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualStatementOfWorkBaselineCodeLengthRule(int MaxLength) : IManualStatementOfWorkBaselineRule
{
    public List<string> Check(ManualStatementOfWorkBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualStatementOfWorkBaselineCodePrefixRule(string Prefix) : IManualStatementOfWorkBaselineRule
{
    public List<string> Check(ManualStatementOfWorkBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualStatementOfWorkBaselineAllowedContactEmailRule(List<string> Allowed) : IManualStatementOfWorkBaselineRule
{
    public List<string> Check(ManualStatementOfWorkBaseline item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual statement of work baseline.</summary>
public sealed record ManualStatementOfWorkBaselineValidator(List<IManualStatementOfWorkBaselineRule> Rules)
{
    public List<string> Validate(ManualStatementOfWorkBaseline item)
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

    public void RequireValid(ManualStatementOfWorkBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualStatementOfWorkBaseline> items)
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

/// <summary>Functions of manual statement of work baseline rules.</summary>
public static class ManualStatementOfWorkBaselineRules
{
    public static ManualStatementOfWorkBaselineValidator DefaultManualStatementOfWorkBaselineValidator()
    {
        return new ManualStatementOfWorkBaselineValidator(
            new List<IManualStatementOfWorkBaselineRule>
            {
                new ManualStatementOfWorkBaselineRequiredContactEmailRule(
                    "manual statement of work baseline needs an entry"),
                new ManualStatementOfWorkBaselineCodeLengthRule(80),
                new ManualStatementOfWorkBaselineCodePrefixRule("REF"),
                new ManualStatementOfWorkBaselineAllowedContactEmailRule(
                    new List<string> { "global", "pending", "manual" }),
            });
    }
}
