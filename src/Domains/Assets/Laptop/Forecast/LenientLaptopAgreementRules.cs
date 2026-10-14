namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient laptop agreement must pass. Check returns its problems, or none.</summary>
public interface ILenientLaptopAgreementRule
{
    List<string> Check(LenientLaptopAgreement item);
}

public sealed record LenientLaptopAgreementRequiredChecksumRule(string Message) : ILenientLaptopAgreementRule
{
    public List<string> Check(LenientLaptopAgreement item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopAgreementAllowedCodeRule(List<string> Allowed) : ILenientLaptopAgreementRule
{
    public List<string> Check(LenientLaptopAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is stale" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopAgreementLeaveDaysRangeRule(int Min, int Max) : ILenientLaptopAgreementRule
{
    public List<string> Check(LenientLaptopAgreement item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientLaptopAgreementRequiredNameRule(string Message) : ILenientLaptopAgreementRule
{
    public List<string> Check(LenientLaptopAgreement item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient laptop agreement.</summary>
public sealed record LenientLaptopAgreementValidator(List<ILenientLaptopAgreementRule> Rules)
{
    public List<string> Validate(LenientLaptopAgreement item)
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

    public void RequireValid(LenientLaptopAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientLaptopAgreement> items)
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

/// <summary>Functions of lenient laptop agreement rules.</summary>
public static class LenientLaptopAgreementRules
{
    public static LenientLaptopAgreementValidator DefaultLenientLaptopAgreementValidator()
    {
        return new LenientLaptopAgreementValidator(
            new List<ILenientLaptopAgreementRule>
            {
                new LenientLaptopAgreementRequiredChecksumRule("lenient laptop agreement needs this field"),
                new LenientLaptopAgreementAllowedCodeRule(new List<string> { "global", "batched" }),
                new LenientLaptopAgreementLeaveDaysRangeRule(64, 64),
                new LenientLaptopAgreementRequiredNameRule("lenient laptop agreement needs an entry"),
            });
    }
}
