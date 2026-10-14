namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves cost model assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelAssignmentWorkflow(ICostModelAssignmentStore Store, CostModelAssignmentValidator Validator, IClock Clock, List<string> Journal, CostModelAssignmentEngine Engine)
{
    public static CostModelAssignmentWorkflow NewCostModelAssignmentWorkflow(ICostModelAssignmentStore store, IClock clock, Registry flags)
    {
        return new CostModelAssignmentWorkflow(
            store,
            CostModelAssignmentRules.DefaultCostModelAssignmentValidator(),
            clock,
            new List<string>(),
            CostModelAssignmentEngine.DefaultCostModelAssignmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelAssignmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelAssignmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model assignment {code} not found");
        }
        var result = Engine.Run(item);
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
