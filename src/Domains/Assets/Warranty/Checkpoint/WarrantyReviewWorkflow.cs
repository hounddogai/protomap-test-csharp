namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyReviewWorkflow(IWarrantyReviewStore Store, WarrantyReviewValidator Validator, IClock Clock, List<string> Journal, WarrantyReviewEngine Engine)
{
    public static WarrantyReviewWorkflow NewWarrantyReviewWorkflow(IWarrantyReviewStore store, IClock clock)
    {
        return new WarrantyReviewWorkflow(
            store,
            WarrantyReviewRules.DefaultWarrantyReviewValidator(),
            clock,
            new List<string>(),
            WarrantyReviewEngine.DefaultWarrantyReviewEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyReviewStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyReviewStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty review {code} not found");
        }
        var result = Engine.Price(item);
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
