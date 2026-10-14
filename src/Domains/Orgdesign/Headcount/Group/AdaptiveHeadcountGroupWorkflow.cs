namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive headcount groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptiveHeadcountGroupManager(IAdaptiveHeadcountGroupStore Store, AdaptiveHeadcountGroupValidator Validator, IClock Clock, List<string> Journal, AdaptiveHeadcountGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive headcount group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveHeadcountGroupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive headcount group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveHeadcountGroupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive headcount group {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of adaptive headcount group workflow.</summary>
public static class AdaptiveHeadcountGroupWorkflow
{
    public static AdaptiveHeadcountGroupManager NewAdaptiveHeadcountGroupManager(IAdaptiveHeadcountGroupStore store, IClock clock, Registry flags)
    {
        return new AdaptiveHeadcountGroupManager(
            store,
            AdaptiveHeadcountGroupRules.DefaultAdaptiveHeadcountGroupValidator(),
            clock,
            new List<string>(),
            AdaptiveHeadcountGroupEngine.DefaultAdaptiveHeadcountGroupEngine(flags));
    }
}
