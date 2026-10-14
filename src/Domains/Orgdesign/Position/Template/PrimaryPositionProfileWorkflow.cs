namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary position profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryPositionProfileManager(IPrimaryPositionProfileStore Store, PrimaryPositionProfileValidator Validator, IClock Clock, List<string> Journal, PrimaryPositionProfileEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary position profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPositionProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary position profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPositionProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary position profile {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of primary position profile workflow.</summary>
public static class PrimaryPositionProfileWorkflow
{
    public static PrimaryPositionProfileManager NewPrimaryPositionProfileManager(IPrimaryPositionProfileStore store, IClock clock, Registry flags)
    {
        return new PrimaryPositionProfileManager(
            store,
            PrimaryPositionProfileRules.DefaultPrimaryPositionProfileValidator(),
            clock,
            new List<string>(),
            PrimaryPositionProfileEngine.DefaultPrimaryPositionProfileEngine(flags));
    }
}
