namespace Acme.Workforce.Workplace.Desk.DeskProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional desk claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalDeskClaimWorkflow(IRegionalDeskClaimStore Store, RegionalDeskClaimValidator Validator, IClock Clock, List<string> Journal, RegionalDeskClaimEngine Engine)
{
    public static RegionalDeskClaimWorkflow NewRegionalDeskClaimWorkflow(IRegionalDeskClaimStore store, IClock clock, Registry flags)
    {
        return new RegionalDeskClaimWorkflow(
            store,
            RegionalDeskClaimRules.DefaultRegionalDeskClaimValidator(),
            clock,
            new List<string>(),
            RegionalDeskClaimEngine.DefaultRegionalDeskClaimEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional desk claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalDeskClaimStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional desk claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalDeskClaimStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional desk claim {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}
