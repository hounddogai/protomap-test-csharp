namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves weighted adjustment templates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedAdjustmentTemplateDesk(IWeightedAdjustmentTemplateStore Store, WeightedAdjustmentTemplateValidator Validator, IClock Clock, List<string> Journal, WeightedAdjustmentTemplateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted adjustment template {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedAdjustmentTemplateStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted adjustment template {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedAdjustmentTemplateStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted adjustment template {code} not found");
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

/// <summary>Functions of weighted adjustment template workflow.</summary>
public static class WeightedAdjustmentTemplateWorkflow
{
    public static WeightedAdjustmentTemplateDesk NewWeightedAdjustmentTemplateDesk(IWeightedAdjustmentTemplateStore store, IClock clock, Registry flags)
    {
        return new WeightedAdjustmentTemplateDesk(
            store,
            WeightedAdjustmentTemplateRules.DefaultWeightedAdjustmentTemplateValidator(),
            clock,
            new List<string>(),
            WeightedAdjustmentTemplateEngine.DefaultWeightedAdjustmentTemplateEngine(flags));
    }
}
