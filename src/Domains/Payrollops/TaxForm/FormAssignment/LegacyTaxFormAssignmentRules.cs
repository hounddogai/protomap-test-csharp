namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy tax form assignment must pass. Check returns its problems, or none.</summary>
public interface ILegacyTaxFormAssignmentRule
{
    List<string> Check(LegacyTaxFormAssignment item);
}

public sealed record LegacyTaxFormAssignmentRequiredDescriptionRule(string Message) : ILegacyTaxFormAssignmentRule
{
    public List<string> Check(LegacyTaxFormAssignment item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormAssignmentContactEmailLengthRule(int MaxLength) : ILegacyTaxFormAssignmentRule
{
    public List<string> Check(LegacyTaxFormAssignment item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormAssignmentLabelLengthRule(int MaxLength) : ILegacyTaxFormAssignmentRule
{
    public List<string> Check(LegacyTaxFormAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormAssignmentAllowedTeamRule(List<string> Allowed) : ILegacyTaxFormAssignmentRule
{
    public List<string> Check(LegacyTaxFormAssignment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormAssignmentCodePrefixRule(string Prefix) : ILegacyTaxFormAssignmentRule
{
    public List<string> Check(LegacyTaxFormAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyTaxFormAssignmentRequiredLabelRule(string Message) : ILegacyTaxFormAssignmentRule
{
    public List<string> Check(LegacyTaxFormAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy tax form assignment.</summary>
public sealed record LegacyTaxFormAssignmentValidator(List<ILegacyTaxFormAssignmentRule> Rules)
{
    public List<string> Validate(LegacyTaxFormAssignment item)
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

    public void RequireValid(LegacyTaxFormAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyTaxFormAssignment> items)
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

/// <summary>Functions of legacy tax form assignment rules.</summary>
public static class LegacyTaxFormAssignmentRules
{
    public static LegacyTaxFormAssignmentValidator DefaultLegacyTaxFormAssignmentValidator()
    {
        return new LegacyTaxFormAssignmentValidator(
            new List<ILegacyTaxFormAssignmentRule>
            {
                new LegacyTaxFormAssignmentRequiredDescriptionRule("legacy tax form assignment needs an entry"),
                new LegacyTaxFormAssignmentContactEmailLengthRule(80),
                new LegacyTaxFormAssignmentLabelLengthRule(255),
                new LegacyTaxFormAssignmentAllowedTeamRule(new List<string>()),
                new LegacyTaxFormAssignmentCodePrefixRule("OPS"),
                new LegacyTaxFormAssignmentRequiredLabelRule("legacy tax form assignment needs a value"),
            });
    }
}
