namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskProfileWorkflow(IDeskProfileStore Store, DeskProfileValidator Validator, IClock Clock, List<string> Journal, DeskProfileEngine Engine)
{
    public static DeskProfileWorkflow NewDeskProfileWorkflow(IDeskProfileStore store, IClock clock, Registry flags)
    {
        return new DeskProfileWorkflow(
            store,
            DeskProfileRules.DefaultDeskProfileValidator(),
            clock,
            new List<string>(),
            DeskProfileEngine.DefaultDeskProfileEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskProfileStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskProfileStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk profile {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.UpdatedAt > cutoff)
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
