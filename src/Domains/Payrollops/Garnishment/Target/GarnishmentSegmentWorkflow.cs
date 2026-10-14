namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves garnishment segments through their life cycle and keeps a journal of what it did.</summary>
public sealed record GarnishmentSegmentManager(IGarnishmentSegmentStore Store, GarnishmentSegmentValidator Validator, IClock Clock, List<string> Journal, GarnishmentSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GarnishmentSegmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GarnishmentSegmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"garnishment segment {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of garnishment segment workflow.</summary>
public static class GarnishmentSegmentWorkflow
{
    public static GarnishmentSegmentManager NewGarnishmentSegmentManager(IGarnishmentSegmentStore store, IClock clock)
    {
        return new GarnishmentSegmentManager(
            store,
            GarnishmentSegmentRules.DefaultGarnishmentSegmentValidator(),
            clock,
            new List<string>(),
            GarnishmentSegmentEngine.DefaultGarnishmentSegmentEngine());
    }
}
