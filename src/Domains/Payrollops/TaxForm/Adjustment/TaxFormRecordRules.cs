namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form record must pass. Check returns its problems, or none.</summary>
public interface ITaxFormRecordRule
{
    List<string> Check(TaxFormRecord item);
}

public sealed record TaxFormRecordCodePrefixRule(string Prefix) : ITaxFormRecordRule
{
    public List<string> Check(TaxFormRecord item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRecordFloorValueRangeRule(double Min, double Max) : ITaxFormRecordRule
{
    public List<string> Check(TaxFormRecord item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormRecordRequiredContactEmailRule(string Message) : ITaxFormRecordRule
{
    public List<string> Check(TaxFormRecord item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRecordRequiredLabelRule(string Message) : ITaxFormRecordRule
{
    public List<string> Check(TaxFormRecord item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRecordAllowedContactEmailRule(List<string> Allowed) : ITaxFormRecordRule
{
    public List<string> Check(TaxFormRecord item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form record.</summary>
public sealed record TaxFormRecordValidator(List<ITaxFormRecordRule> Rules)
{
    public List<string> Validate(TaxFormRecord item)
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

    public void RequireValid(TaxFormRecord item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormRecord> items)
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

/// <summary>Functions of tax form record rules.</summary>
public static class TaxFormRecordRules
{
    public static TaxFormRecordValidator DefaultTaxFormRecordValidator()
    {
        return new TaxFormRecordValidator(
            new List<ITaxFormRecordRule>
            {
                new TaxFormRecordCodePrefixRule("X"),
                new TaxFormRecordFloorValueRangeRule(7.7, 404.2),
                new TaxFormRecordRequiredContactEmailRule("tax form record needs a value"),
                new TaxFormRecordRequiredLabelRule("tax form record needs a value"),
                new TaxFormRecordAllowedContactEmailRule(new List<string>()),
            });
    }
}
