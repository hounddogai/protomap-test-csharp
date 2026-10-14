namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling access badge profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingAccessBadgeProfileWorkflow(IRollingAccessBadgeProfileStore Store, RollingAccessBadgeProfileValidator Validator, IClock Clock, List<string> Journal, RollingAccessBadgeProfileEngine Engine)
{
    public static RollingAccessBadgeProfileWorkflow NewRollingAccessBadgeProfileWorkflow(IRollingAccessBadgeProfileStore store, IClock clock, Registry flags)
    {
        return new RollingAccessBadgeProfileWorkflow(
            store,
            RollingAccessBadgeProfileRules.DefaultRollingAccessBadgeProfileValidator(),
            clock,
            new List<string>(),
            RollingAccessBadgeProfileEngine.DefaultRollingAccessBadgeProfileEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling access badge profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingAccessBadgeProfileStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling access badge profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingAccessBadgeProfileStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling access badge profile {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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
