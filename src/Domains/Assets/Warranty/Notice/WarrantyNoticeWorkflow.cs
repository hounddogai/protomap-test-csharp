namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyNoticeWorkflow(IWarrantyNoticeStore Store, WarrantyNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    public static WarrantyNoticeWorkflow NewWarrantyNoticeWorkflow(IWarrantyNoticeStore store, IClock clock)
    {
        return new WarrantyNoticeWorkflow(
            store,
            WarrantyNoticeRules.DefaultWarrantyNoticeValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
