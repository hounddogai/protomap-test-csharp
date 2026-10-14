namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty records through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyRecordManager(IWarrantyRecordStore Store, WarrantyRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyRecordStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyRecordStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of warranty record workflow.</summary>
public static class WarrantyRecordWorkflow
{
    public static WarrantyRecordManager NewWarrantyRecordManager(IWarrantyRecordStore store, IClock clock)
    {
        return new WarrantyRecordManager(
            store,
            WarrantyRecordRules.DefaultWarrantyRecordValidator(),
            clock,
            new List<string>());
    }
}
