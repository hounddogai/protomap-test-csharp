namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted access badge exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedAccessBadgeExceptionDesk(IWeightedAccessBadgeExceptionStore Store, WeightedAccessBadgeExceptionValidator Validator, IClock Clock, List<string> Journal, WeightedAccessBadgeExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted access badge exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedAccessBadgeExceptionStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted access badge exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedAccessBadgeExceptionStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted access badge exception {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of weighted access badge exception workflow.</summary>
public static class WeightedAccessBadgeExceptionWorkflow
{
    public static WeightedAccessBadgeExceptionDesk NewWeightedAccessBadgeExceptionDesk(IWeightedAccessBadgeExceptionStore store, IClock clock, Registry flags)
    {
        return new WeightedAccessBadgeExceptionDesk(
            store,
            WeightedAccessBadgeExceptionRules.DefaultWeightedAccessBadgeExceptionValidator(),
            clock,
            new List<string>(),
            WeightedAccessBadgeExceptionEngine.DefaultWeightedAccessBadgeExceptionEngine(flags));
    }
}
