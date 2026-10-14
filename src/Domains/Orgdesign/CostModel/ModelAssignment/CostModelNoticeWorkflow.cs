namespace Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves cost model notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelNoticeDesk(ICostModelNoticeStore Store, CostModelNoticeValidator Validator, IClock Clock, List<string> Journal, CostModelNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelNoticeStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelNoticeStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model notice {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of cost model notice workflow.</summary>
public static class CostModelNoticeWorkflow
{
    public static CostModelNoticeDesk NewCostModelNoticeDesk(ICostModelNoticeStore store, IClock clock, Registry flags)
    {
        return new CostModelNoticeDesk(
            store,
            CostModelNoticeRules.DefaultCostModelNoticeValidator(),
            clock,
            new List<string>(),
            CostModelNoticeEngine.DefaultCostModelNoticeEngine(flags));
    }
}
