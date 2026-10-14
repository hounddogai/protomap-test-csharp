namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary warranty rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryWarrantyRollupWorkflow(IPrimaryWarrantyRollupStore Store, PrimaryWarrantyRollupValidator Validator, IClock Clock, List<string> Journal, PrimaryWarrantyRollupEngine Engine)
{
    public static PrimaryWarrantyRollupWorkflow NewPrimaryWarrantyRollupWorkflow(IPrimaryWarrantyRollupStore store, IClock clock)
    {
        return new PrimaryWarrantyRollupWorkflow(
            store,
            PrimaryWarrantyRollupRules.DefaultPrimaryWarrantyRollupValidator(),
            clock,
            new List<string>(),
            PrimaryWarrantyRollupEngine.DefaultPrimaryWarrantyRollupEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary warranty rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryWarrantyRollupStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary warranty rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryWarrantyRollupStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary warranty rollup {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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
