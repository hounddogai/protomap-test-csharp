namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tax form batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormBatchWorkflow(ITaxFormBatchStore Store, TaxFormBatchValidator Validator, IClock Clock, List<string> Journal, TaxFormBatchEngine Engine)
{
    public static TaxFormBatchWorkflow NewTaxFormBatchWorkflow(ITaxFormBatchStore store, IClock clock)
    {
        return new TaxFormBatchWorkflow(
            store,
            TaxFormBatchRules.DefaultTaxFormBatchValidator(),
            clock,
            new List<string>(),
            TaxFormBatchEngine.DefaultTaxFormBatchEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormBatchStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormBatchStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form batch {code} not found");
        }
        var result = Engine.Evaluate(item);
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
