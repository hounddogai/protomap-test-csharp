namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentSnapshotWorkflow(IGarnishmentSnapshotStore Store, GarnishmentSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    public static GarnishmentSnapshotWorkflow NewGarnishmentSnapshotWorkflow(IGarnishmentSnapshotStore store, IClock clock)
    {
        return new GarnishmentSnapshotWorkflow(
            store,
            GarnishmentSnapshotRules.DefaultGarnishmentSnapshotValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentSnapshotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentSnapshotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.EndsAt > cutoff)
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
