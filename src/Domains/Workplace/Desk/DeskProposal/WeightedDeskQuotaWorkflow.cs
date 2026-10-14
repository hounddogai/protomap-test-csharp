namespace Acme.Workforce.Workplace.Desk.DeskProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted desk quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedDeskQuotaCoordinator(IWeightedDeskQuotaStore Store, WeightedDeskQuotaValidator Validator, IClock Clock, List<string> Journal, WeightedDeskQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted desk quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedDeskQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted desk quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedDeskQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted desk quota {code} not found");
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

/// <summary>Functions of weighted desk quota workflow.</summary>
public static class WeightedDeskQuotaWorkflow
{
    public static WeightedDeskQuotaCoordinator NewWeightedDeskQuotaCoordinator(IWeightedDeskQuotaStore store, IClock clock, Registry flags)
    {
        return new WeightedDeskQuotaCoordinator(
            store,
            WeightedDeskQuotaRules.DefaultWeightedDeskQuotaValidator(),
            clock,
            new List<string>(),
            WeightedDeskQuotaEngine.DefaultWeightedDeskQuotaEngine(flags));
    }
}
