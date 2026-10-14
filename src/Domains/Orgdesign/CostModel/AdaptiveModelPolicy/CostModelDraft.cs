namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model draft is in its life cycle.</summary>
public enum CostModelDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a cost model draft and its current status.</summary>
public sealed class CostModelDraft(string code, string label, string title, int capacity, double ratio, double maximum, double ceiling, List<string> tags, string employeeId, CostModelDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public double Ratio { get; set; } = ratio;
    public double Maximum { get; set; } = maximum;
    public double Ceiling { get; set; } = ceiling;
    public List<string> Tags { get; set; } = tags;
    public string EmployeeId { get; set; } = employeeId;
    public CostModelDraftStatus Status { get; set; } = status;

    /// <summary>Starts a cost model draft in status draft.</summary>
    public static CostModelDraft NewCostModelDraft(string code, string label)
    {
        return new CostModelDraft(
            code,
            label,
            "",
            0,
            0.0,
            0.0,
            0.0,
            new List<string>(),
            "",
            CostModelDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelDraftStatus next)
    {
        switch (Status)
        {
            case CostModelDraftStatus.Draft:
                return next == CostModelDraftStatus.Suspended || next == CostModelDraftStatus.Closed;
            case CostModelDraftStatus.Pending:
                return next == CostModelDraftStatus.Active || next == CostModelDraftStatus.Suspended;
            case CostModelDraftStatus.Active:
                return next == CostModelDraftStatus.Suspended || next == CostModelDraftStatus.Closed;
            case CostModelDraftStatus.Suspended:
                switch (next)
                {
                    case CostModelDraftStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CostModelDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double ProjectedTotal()
    {
        return Math.Round(Maximum + Ceiling, 2);
    }

    public CostModelDraft Copy()
    {
        return new CostModelDraft(Code, Label, Title, Capacity, Ratio, Maximum, Ceiling, Tags, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
