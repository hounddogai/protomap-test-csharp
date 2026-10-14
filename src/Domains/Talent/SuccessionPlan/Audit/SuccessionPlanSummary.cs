namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan summary is in its life cycle.</summary>
public enum SuccessionPlanSummaryStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A succession plan summary. Its code is unique within a tenant.</summary>
public sealed class SuccessionPlanSummary(string code, string label, int windowDays, string salaryBand, string name, bool pinned, bool archived, int revision, double amount, string category, SuccessionPlanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public bool Archived { get; set; } = archived;
    public int Revision { get; set; } = revision;
    public double Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public SuccessionPlanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan summary in status requested.</summary>
    public static SuccessionPlanSummary NewSuccessionPlanSummary(string code, string label)
    {
        return new SuccessionPlanSummary(
            code,
            label,
            0,
            "",
            "",
            false,
            false,
            0,
            0.0,
            "",
            SuccessionPlanSummaryStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanSummaryStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanSummaryStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanSummaryStatus.Requested:
                return next == SuccessionPlanSummaryStatus.Revoked;
            case SuccessionPlanSummaryStatus.Granted:
                return next == SuccessionPlanSummaryStatus.Revoked || next == SuccessionPlanSummaryStatus.Expired;
            case SuccessionPlanSummaryStatus.Revoked:
                return next == SuccessionPlanSummaryStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays * Amount, 2);
    }

    public SuccessionPlanSummary Copy()
    {
        return new SuccessionPlanSummary(
            Code,
            Label,
            WindowDays,
            SalaryBand,
            Name,
            Pinned,
            Archived,
            Revision,
            Amount,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
