namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form rule is in its life cycle.</summary>
public enum TaxFormRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One tax form rule, with what the planners need to act on it.</summary>
public sealed class TaxFormRule(string code, string title, int leaveDays, List<string> tags, double margin, string region, int endsAt, TaxFormRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public List<string> Tags { get; set; } = tags;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public int EndsAt { get; set; } = endsAt;
    public TaxFormRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tax form rule in status proposed.</summary>
    public static TaxFormRule NewTaxFormRule(string code, string title)
    {
        return new TaxFormRule(code, title, 0, new List<string>(), 0.0, "", 0, TaxFormRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormRuleStatus next)
    {
        switch (Status)
        {
            case TaxFormRuleStatus.Proposed:
                return next == TaxFormRuleStatus.Approved;
            case TaxFormRuleStatus.Approved:
                return next == TaxFormRuleStatus.Rejected || next == TaxFormRuleStatus.Withdrawn;
            case TaxFormRuleStatus.Rejected:
                switch (next)
                {
                    case TaxFormRuleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveLoad()
    {
        return Math.Round(Margin * (double)LeaveDays, 2);
    }

    public TaxFormRule Copy()
    {
        return new TaxFormRule(Code, Title, LeaveDays, Tags, Margin, Region, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
