namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary refresh requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryRefreshRequestService(IPrimaryRefreshRequestStore Store, PrimaryRefreshRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary refresh request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryRefreshRequestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary refresh request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryRefreshRequestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of primary refresh request workflow.</summary>
public static class PrimaryRefreshRequestWorkflow
{
    public static PrimaryRefreshRequestService NewPrimaryRefreshRequestService(IPrimaryRefreshRequestStore store, IClock clock)
    {
        return new PrimaryRefreshRequestService(
            store,
            PrimaryRefreshRequestRules.DefaultPrimaryRefreshRequestValidator(),
            clock,
            new List<string>());
    }
}
