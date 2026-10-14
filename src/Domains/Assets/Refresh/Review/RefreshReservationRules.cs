namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a refresh reservation must pass. Check returns its problems, or none.</summary>
public interface IRefreshReservationRule
{
    List<string> Check(RefreshReservation item);
}

public sealed record RefreshReservationCodePrefixRule(string Prefix) : IRefreshReservationRule
{
    public List<string> Check(RefreshReservation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RefreshReservationPinnedGuardRule(double Limit) : IRefreshReservationRule
{
    public List<string> Check(RefreshReservation item)
    {
        if (item.Pinned && item.Fte > Limit)
        {
            return new List<string> { $"fte {item.Fte} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RefreshReservationRequiredNameRule(string Message) : IRefreshReservationRule
{
    public List<string> Check(RefreshReservation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RefreshReservationAllowedCodeRule(List<string> Allowed) : IRefreshReservationRule
{
    public List<string> Check(RefreshReservation item)
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

/// <summary>Runs every rule of a refresh reservation.</summary>
public sealed record RefreshReservationValidator(List<IRefreshReservationRule> Rules)
{
    public List<string> Validate(RefreshReservation item)
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

    public void RequireValid(RefreshReservation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RefreshReservation> items)
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

/// <summary>Functions of refresh reservation rules.</summary>
public static class RefreshReservationRules
{
    public static RefreshReservationValidator DefaultRefreshReservationValidator()
    {
        return new RefreshReservationValidator(
            new List<IRefreshReservationRule>
            {
                new RefreshReservationCodePrefixRule("Q"),
                new RefreshReservationPinnedGuardRule(9.0),
                new RefreshReservationRequiredNameRule("refresh reservation needs this field"),
                new RefreshReservationAllowedCodeRule(new List<string>()),
            });
    }
}
