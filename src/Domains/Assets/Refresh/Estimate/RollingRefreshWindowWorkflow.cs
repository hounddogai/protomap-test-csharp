namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling refresh windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingRefreshWindowWorkflow(IRollingRefreshWindowStore Store, RollingRefreshWindowValidator Validator, IClock Clock, List<string> Journal, RollingRefreshWindowEngine Engine)
{
    public static RollingRefreshWindowWorkflow NewRollingRefreshWindowWorkflow(IRollingRefreshWindowStore store, IClock clock, Registry flags)
    {
        return new RollingRefreshWindowWorkflow(
            store,
            RollingRefreshWindowRules.DefaultRollingRefreshWindowValidator(),
            clock,
            new List<string>(),
            RollingRefreshWindowEngine.DefaultRollingRefreshWindowEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingRefreshWindowStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingRefreshWindowStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling refresh window {code} not found");
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
