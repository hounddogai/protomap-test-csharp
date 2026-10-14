namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted reorg claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedReorgClaimWorkflow(IWeightedReorgClaimStore Store, WeightedReorgClaimValidator Validator, IClock Clock, List<string> Journal)
{
    public static WeightedReorgClaimWorkflow NewWeightedReorgClaimWorkflow(IWeightedReorgClaimStore store, IClock clock)
    {
        return new WeightedReorgClaimWorkflow(
            store,
            WeightedReorgClaimRules.DefaultWeightedReorgClaimValidator(),
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
            throw new InvalidOperationException($"weighted reorg claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedReorgClaimStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted reorg claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedReorgClaimStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
