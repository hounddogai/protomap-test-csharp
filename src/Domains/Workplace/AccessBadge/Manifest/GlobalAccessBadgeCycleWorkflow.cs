namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global access badge cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalAccessBadgeCycleWorkflow(IGlobalAccessBadgeCycleStore Store, GlobalAccessBadgeCycleValidator Validator, IClock Clock, List<string> Journal)
{
    public static GlobalAccessBadgeCycleWorkflow NewGlobalAccessBadgeCycleWorkflow(IGlobalAccessBadgeCycleStore store, IClock clock)
    {
        return new GlobalAccessBadgeCycleWorkflow(
            store,
            GlobalAccessBadgeCycleRules.DefaultGlobalAccessBadgeCycleValidator(),
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
            throw new InvalidOperationException($"global access badge cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalAccessBadgeCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global access badge cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalAccessBadgeCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
