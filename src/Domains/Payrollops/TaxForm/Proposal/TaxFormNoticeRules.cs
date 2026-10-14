namespace Acme.Workforce.Payrollops.TaxForm.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form notice must pass. Check returns its problems, or none.</summary>
public interface ITaxFormNoticeRule
{
    List<string> Check(TaxFormNotice item);
}

public sealed record TaxFormNoticeCodeLengthRule(int MaxLength) : ITaxFormNoticeRule
{
    public List<string> Check(TaxFormNotice item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormNoticeExpiresAtRangeRule(int Min, int Max) : ITaxFormNoticeRule
{
    public List<string> Check(TaxFormNotice item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormNoticeVisibleGuardRule(int Limit) : ITaxFormNoticeRule
{
    public List<string> Check(TaxFormNotice item)
    {
        if (item.Visible && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormNoticeRequiredLabelRule(string Message) : ITaxFormNoticeRule
{
    public List<string> Check(TaxFormNotice item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormNoticeAllowedCodeRule(List<string> Allowed) : ITaxFormNoticeRule
{
    public List<string> Check(TaxFormNotice item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormNoticeCodePrefixRule(string Prefix) : ITaxFormNoticeRule
{
    public List<string> Check(TaxFormNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form notice.</summary>
public sealed record TaxFormNoticeValidator(List<ITaxFormNoticeRule> Rules)
{
    public List<string> Validate(TaxFormNotice item)
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

    public void RequireValid(TaxFormNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormNotice> items)
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

/// <summary>Functions of tax form notice rules.</summary>
public static class TaxFormNoticeRules
{
    public static TaxFormNoticeValidator DefaultTaxFormNoticeValidator()
    {
        return new TaxFormNoticeValidator(
            new List<ITaxFormNoticeRule>
            {
                new TaxFormNoticeCodeLengthRule(3600),
                new TaxFormNoticeExpiresAtRangeRule(80, 86400),
                new TaxFormNoticeVisibleGuardRule(255),
                new TaxFormNoticeRequiredLabelRule("tax form notice needs this field"),
                new TaxFormNoticeAllowedCodeRule(new List<string> { "batched", "batched" }),
                new TaxFormNoticeCodePrefixRule("X"),
            });
    }
}
