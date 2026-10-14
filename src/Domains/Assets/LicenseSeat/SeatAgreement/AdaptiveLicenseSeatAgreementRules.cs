namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a adaptive license seat agreement must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveLicenseSeatAgreementRule
{
    List<string> Check(AdaptiveLicenseSeatAgreement item);
}

public sealed record AdaptiveLicenseSeatAgreementWeightRangeRule(double Min, double Max) : IAdaptiveLicenseSeatAgreementRule
{
    public List<string> Check(AdaptiveLicenseSeatAgreement item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AdaptiveLicenseSeatAgreementCodeLengthRule(int MaxLength) : IAdaptiveLicenseSeatAgreementRule
{
    public List<string> Check(AdaptiveLicenseSeatAgreement item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAgreementPinnedGuardRule(double Limit) : IAdaptiveLicenseSeatAgreementRule
{
    public List<string> Check(AdaptiveLicenseSeatAgreement item)
    {
        if (item.Pinned && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAgreementRequiredSourceRule(string Message) : IAdaptiveLicenseSeatAgreementRule
{
    public List<string> Check(AdaptiveLicenseSeatAgreement item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveLicenseSeatAgreementAllowedSourceRule(List<string> Allowed) : IAdaptiveLicenseSeatAgreementRule
{
    public List<string> Check(AdaptiveLicenseSeatAgreement item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive license seat agreement.</summary>
public sealed record AdaptiveLicenseSeatAgreementValidator(List<IAdaptiveLicenseSeatAgreementRule> Rules)
{
    public List<string> Validate(AdaptiveLicenseSeatAgreement item)
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

    public void RequireValid(AdaptiveLicenseSeatAgreement item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveLicenseSeatAgreement> items)
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

/// <summary>Functions of adaptive license seat agreement rules.</summary>
public static class AdaptiveLicenseSeatAgreementRules
{
    public static AdaptiveLicenseSeatAgreementValidator DefaultAdaptiveLicenseSeatAgreementValidator()
    {
        return new AdaptiveLicenseSeatAgreementValidator(
            new List<IAdaptiveLicenseSeatAgreementRule>
            {
                new AdaptiveLicenseSeatAgreementWeightRangeRule(1.2, 3481.1),
                new AdaptiveLicenseSeatAgreementCodeLengthRule(120),
                new AdaptiveLicenseSeatAgreementPinnedGuardRule(8.9),
                new AdaptiveLicenseSeatAgreementRequiredSourceRule("adaptive license seat agreement needs an entry"),
                new AdaptiveLicenseSeatAgreementAllowedSourceRule(
                    new List<string> { "pending", "seasonal", "strict" }),
            });
    }
}
