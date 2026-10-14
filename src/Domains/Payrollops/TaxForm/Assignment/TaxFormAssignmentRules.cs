namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form assignment must pass. Check returns its problems, or none.</summary>
public interface ITaxFormAssignmentRule
{
    List<string> Check(TaxFormAssignment item);
}

public sealed record TaxFormAssignmentCodePrefixRule(string Prefix) : ITaxFormAssignmentRule
{
    public List<string> Check(TaxFormAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAssignmentRequiredPhoneRule(string Message) : ITaxFormAssignmentRule
{
    public List<string> Check(TaxFormAssignment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAssignmentAllowedPhoneRule(List<string> Allowed) : ITaxFormAssignmentRule
{
    public List<string> Check(TaxFormAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Phone)))
        {
            return new List<string> { $"phone {item.Phone} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form assignment.</summary>
public sealed record TaxFormAssignmentValidator(List<ITaxFormAssignmentRule> Rules)
{
    public List<string> Validate(TaxFormAssignment item)
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

    public void RequireValid(TaxFormAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormAssignment> items)
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

/// <summary>Functions of tax form assignment rules.</summary>
public static class TaxFormAssignmentRules
{
    public static TaxFormAssignmentValidator DefaultTaxFormAssignmentValidator()
    {
        return new TaxFormAssignmentValidator(
            new List<ITaxFormAssignmentRule>
            {
                new TaxFormAssignmentCodePrefixRule("Q"),
                new TaxFormAssignmentRequiredPhoneRule("tax form assignment needs a value"),
                new TaxFormAssignmentAllowedPhoneRule(new List<string> { "global", "strict" }),
            });
    }
}
