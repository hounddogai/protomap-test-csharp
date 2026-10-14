namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves occupancy reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyReviewWorkflow(IOccupancyReviewStore Store, OccupancyReviewValidator Validator, IClock Clock, List<string> Journal, OccupancyReviewEngine Engine)
{
    public static OccupancyReviewWorkflow NewOccupancyReviewWorkflow(IOccupancyReviewStore store, IClock clock)
    {
        return new OccupancyReviewWorkflow(
            store,
            OccupancyReviewRules.DefaultOccupancyReviewValidator(),
            clock,
            new List<string>(),
            OccupancyReviewEngine.DefaultOccupancyReviewEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyReviewStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyReviewStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy review {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.StartsAt > cutoff)
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
