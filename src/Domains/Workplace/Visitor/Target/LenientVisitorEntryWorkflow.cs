namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient visitor entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientVisitorEntryWorkflow(ILenientVisitorEntryStore Store, LenientVisitorEntryValidator Validator, IClock Clock, List<string> Journal)
{
    public static LenientVisitorEntryWorkflow NewLenientVisitorEntryWorkflow(ILenientVisitorEntryStore store, IClock clock)
    {
        return new LenientVisitorEntryWorkflow(
            store,
            LenientVisitorEntryRules.DefaultLenientVisitorEntryValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient visitor entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientVisitorEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient visitor entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientVisitorEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
