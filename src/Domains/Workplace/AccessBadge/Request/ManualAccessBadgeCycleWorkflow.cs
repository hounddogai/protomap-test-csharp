namespace Acme.Workforce.Workplace.AccessBadge.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves manual access badge cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualAccessBadgeCycleWorkflow(IManualAccessBadgeCycleStore Store, ManualAccessBadgeCycleValidator Validator, IClock Clock, List<string> Journal, ManualAccessBadgeCycleEngine Engine)
{
    public static ManualAccessBadgeCycleWorkflow NewManualAccessBadgeCycleWorkflow(IManualAccessBadgeCycleStore store, IClock clock, Registry flags)
    {
        return new ManualAccessBadgeCycleWorkflow(
            store,
            ManualAccessBadgeCycleRules.DefaultManualAccessBadgeCycleValidator(),
            clock,
            new List<string>(),
            ManualAccessBadgeCycleEngine.DefaultManualAccessBadgeCycleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualAccessBadgeCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualAccessBadgeCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual access badge cycle {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
