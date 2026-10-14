namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a headcount agreement must pass. Check returns its problems, or none.</summary>
public interface IHeadcountAgreementRule
{
    List<string> Check(HeadcountAgreement item);
}

public sealed record HeadcountAgreementAllowedOwnerIdRule(List<string> Allowed) : IHeadcountAgreementRule
{
    public List<string> Check(HeadcountAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.OwnerId)))
        {
            return new List<string> { $"owner id {item.OwnerId} is below minimum" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAgreementContactEmailLengthRule(int MaxLength) : IHeadcountAgreementRule
{
    public List<string> Check(HeadcountAgreement item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountAgreementFloorValueRangeRule(double Min, double Max) : IHeadcountAgreementRule
{
    public List<string> Check(HeadcountAgreement item)
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

/// <summary>Runs every rule of a headcount agreement.</summary>
public sealed record HeadcountAgreementValidator(List<IHeadcountAgreementRule> Rules)
{
    public List<string> Validate(HeadcountAgreement item)
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

    public void RequireValid(HeadcountAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountAgreement> items)
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

/// <summary>Functions of headcount agreement rules.</summary>
public static class HeadcountAgreementRules
{
    public static HeadcountAgreementValidator DefaultHeadcountAgreementValidator()
    {
        return new HeadcountAgreementValidator(
            new List<IHeadcountAgreementRule>
            {
                new HeadcountAgreementAllowedOwnerIdRule(new List<string> { "default" }),
                new HeadcountAgreementContactEmailLengthRule(80),
                new HeadcountAgreementFloorValueRangeRule(3.8, 4296.4),
            });
    }
}
