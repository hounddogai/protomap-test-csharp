namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted garnishment requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedGarnishmentRequestWorkflow(IWeightedGarnishmentRequestStore Store, WeightedGarnishmentRequestValidator Validator, IClock Clock, List<string> Journal)
{
    public static WeightedGarnishmentRequestWorkflow NewWeightedGarnishmentRequestWorkflow(IWeightedGarnishmentRequestStore store, IClock clock)
    {
        return new WeightedGarnishmentRequestWorkflow(
            store,
            WeightedGarnishmentRequestRules.DefaultWeightedGarnishmentRequestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted garnishment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedGarnishmentRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted garnishment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedGarnishmentRequestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
