namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshDigestWorkflow(IRefreshDigestStore Store, RefreshDigestValidator Validator, IClock Clock, List<string> Journal, RefreshDigestEngine Engine)
{
    public static RefreshDigestWorkflow NewRefreshDigestWorkflow(IRefreshDigestStore store, IClock clock)
    {
        return new RefreshDigestWorkflow(
            store,
            RefreshDigestRules.DefaultRefreshDigestValidator(),
            clock,
            new List<string>(),
            RefreshDigestEngine.DefaultRefreshDigestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshDigestStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshDigestStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh digest {code} not found");
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
