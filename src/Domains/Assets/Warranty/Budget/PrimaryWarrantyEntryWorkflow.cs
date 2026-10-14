namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary warranty entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryWarrantyEntryDesk(IPrimaryWarrantyEntryStore Store, PrimaryWarrantyEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary warranty entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryWarrantyEntryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary warranty entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryWarrantyEntryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of primary warranty entry workflow.</summary>
public static class PrimaryWarrantyEntryWorkflow
{
    public static PrimaryWarrantyEntryDesk NewPrimaryWarrantyEntryDesk(IPrimaryWarrantyEntryStore store, IClock clock)
    {
        return new PrimaryWarrantyEntryDesk(
            store,
            PrimaryWarrantyEntryRules.DefaultPrimaryWarrantyEntryValidator(),
            clock,
            new List<string>());
    }
}
