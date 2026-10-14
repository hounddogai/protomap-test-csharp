namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped cost model draft is in its life cycle.</summary>
public enum CappedCostModelDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A capped cost model draft. Its code is unique within a tenant.</summary>
public sealed class CappedCostModelDraft(string code, string title, string reviewer, double margin, string skill, int tenureMonths, double ratio, string badge, string department, CappedCostModelDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Margin { get; set; } = margin;
    public string Skill { get; set; } = skill;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ratio { get; set; } = ratio;
    public string Badge { get; set; } = badge;
    public string Department { get; set; } = department;
    public CappedCostModelDraftStatus Status { get; set; } = status;

    /// <summary>Starts a capped cost model draft in status requested.</summary>
    public static CappedCostModelDraft NewCappedCostModelDraft(string code, string title)
    {
        return new CappedCostModelDraft(
            code,
            title,
            "",
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            CappedCostModelDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCostModelDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCostModelDraftStatus next)
    {
        switch (Status)
        {
            case CappedCostModelDraftStatus.Requested:
                switch (next)
                {
                    case CappedCostModelDraftStatus.Revoked:
                    case CappedCostModelDraftStatus.Expired:
                        return true;
                }
                return false;
            case CappedCostModelDraftStatus.Granted:
                switch (next)
                {
                    case CappedCostModelDraftStatus.Revoked:
                        return true;
                }
                return false;
            case CappedCostModelDraftStatus.Revoked:
                return next == CappedCostModelDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(CappedCostModelDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped cost model draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped cost model draft"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Ratio - Margin, 2);
    }

    public CappedCostModelDraft Copy()
    {
        return new CappedCostModelDraft(
            Code,
            Title,
            Reviewer,
            Margin,
            Skill,
            TenureMonths,
            Ratio,
            Badge,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
