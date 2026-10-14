namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped desk adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedDeskAdjustmentWorkflow(ICappedDeskAdjustmentStore Store, CappedDeskAdjustmentValidator Validator, IClock Clock, List<string> Journal, CappedDeskAdjustmentEngine Engine)
{
    public static CappedDeskAdjustmentWorkflow NewCappedDeskAdjustmentWorkflow(ICappedDeskAdjustmentStore store, IClock clock, Registry flags)
    {
        return new CappedDeskAdjustmentWorkflow(
            store,
            CappedDeskAdjustmentRules.DefaultCappedDeskAdjustmentValidator(),
            clock,
            new List<string>(),
            CappedDeskAdjustmentEngine.DefaultCappedDeskAdjustmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped desk adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedDeskAdjustmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped desk adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedDeskAdjustmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped desk adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
