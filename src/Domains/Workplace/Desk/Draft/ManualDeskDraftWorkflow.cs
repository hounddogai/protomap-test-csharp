namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual desk drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualDeskDraftService(IManualDeskDraftStore Store, ManualDeskDraftValidator Validator, IClock Clock, List<string> Journal, ManualDeskDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual desk draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualDeskDraftStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual desk draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualDeskDraftStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual desk draft {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of manual desk draft workflow.</summary>
public static class ManualDeskDraftWorkflow
{
    public static ManualDeskDraftService NewManualDeskDraftService(IManualDeskDraftStore store, IClock clock)
    {
        return new ManualDeskDraftService(
            store,
            ManualDeskDraftRules.DefaultManualDeskDraftValidator(),
            clock,
            new List<string>(),
            ManualDeskDraftEngine.DefaultManualDeskDraftEngine());
    }
}
